﻿using Andre.Formats;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorView
{
    private readonly ParamRowEditor _propEditor;
    private readonly Dictionary<string, string> lastRowSearch = new();
    private bool _arrowKeyPressed;
    private bool _focusRows;
    private int _gotoParamRow = -1;
    private bool _mapParamView;
    private bool _eventParamView;
    private bool _gConfigParamView;

    internal ParamEditorScreen _paramEditor;

    internal ParamEditorSelectionState _selection;
    internal int _viewIndex;

    private string lastParamSearch = "";

    public ParamEditorView(ParamEditorScreen parent, int index)
    {
        _paramEditor = parent;
        _viewIndex = index;
        _propEditor = new ParamRowEditor(parent.EditorActionManager, _paramEditor);
        _selection = new ParamEditorSelectionState(_paramEditor);
    }

    //------------------------------------
    // Param
    //------------------------------------
    private void ParamView_ParamList_Header(bool isActiveView)
    {
        ImGui.Text("Params");
        ImGui.Separator();

        if (ParamBank.PrimaryBank.ParamVersion != 0)
        {
            ImGui.Text($"Param version {Utils.ParseParamVersion(ParamBank.PrimaryBank.ParamVersion)}");

            if (ParamBank.PrimaryBank.ParamVersion < ParamBank.VanillaBank.ParamVersion)
            {
                ImGui.SameLine();
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Warning_Text_Color, "(out of date)");
            }
        }

        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchParam))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.InputText($"Search <{KeyBindings.Current.PARAM_SearchParam.HintText}>",
            ref _selection.currentParamSearchString, 256);
        var resAutoParam = AutoFill.ParamSearchBarAutoFill();

        if (resAutoParam != null)
        {
            _selection.SetCurrentParamSearchString(resAutoParam);
        }

        if (!_selection.currentParamSearchString.Equals(lastParamSearch))
        {
            UICache.ClearCaches();
            lastParamSearch = _selection.currentParamSearchString;
        }

        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            // This game has DrawParams, add UI element to toggle viewing DrawParam and GameParams.
            if (ImGui.Checkbox("Edit Drawparams", ref _mapParamView))
            {
                UICache.ClearCaches();
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            // DS2 has map params, add UI element to toggle viewing map params and GameParams.
            if (ImGui.Checkbox("Edit Map Params", ref _mapParamView))
            {
                UICache.ClearCaches();
            }
        }
        else if (Smithbox.ProjectType is ProjectType.ER || Smithbox.ProjectType is ProjectType.AC6)
        {
            // Only show if the user actually has the eventparam file
            if (Path.Exists($"{Smithbox.GameRoot}\\param\\eventparam\\eventparam.parambnd.dcx"))
            {
                if (ImGui.Checkbox("Edit Event Params", ref _eventParamView))
                {
                    _gConfigParamView = false;
                    UICache.ClearCaches();
                }
            }

            if (ImGui.Checkbox("Edit Graphics Config Params", ref _gConfigParamView))
            {
                _eventParamView = false;
                UICache.ClearCaches();
            }
        }

        ImGui.Separator();
    }

    private void ParamView_ParamList_Pinned(float scale)
    {
        List<string> pinnedParamKeyList = new(Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            //ImGui.Text("        Pinned Params");
            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = ParamBank.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);

                if (ParamBank.PrimaryBank.Params.ContainsKey(paramKey))
                {
                    Param p = ParamBank.PrimaryBank.Params[paramKey];
                    if (p != null)
                    {
                        var meta = ParamMetaData.Get(p.AppliedParamdef);
                        var Wiki = meta?.Wiki;
                        if (Wiki != null)
                        {
                            if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                            {
                                meta.Wiki = Wiki;
                            }
                        }
                    }

                    ImGui.Indent(15.0f * scale);
                    if (ImGui.Selectable($"{paramKey}##pin{paramKey}", paramKey == _selection.GetActiveParam()))
                    {
                        EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.Selectable("Unpin " + paramKey))
                        {
                            Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.Remove(paramKey);
                        }

                        EditorDecorations.PinListReorderOptions(Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams, paramKey);

                        if (ImGui.Selectable("Unpin all"))
                        {
                            Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.RemoveAll(x => true);
                        }

                        ImGui.EndPopup();
                    }

                    DisplayDS2MapNameAlias(paramKey);

                    ImGui.Unindent(15.0f * scale);
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    private void ParamView_ParamList_Main(bool doFocus, float scale, float scrollTo)
    {
        List<string> paramKeyList = UICache.GetCached(_paramEditor, _viewIndex, () =>
        {
            List<(ParamBank, Param)> list =
                ParamSearchEngine.pse.Search(true, _selection.currentParamSearchString, true, true);
            var keyList = list.Where(param => param.Item1 == ParamBank.PrimaryBank)
                .Select(param => ParamBank.PrimaryBank.GetKeyForParam(param.Item2)).ToList();

            if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
            {
                if (_mapParamView)
                {
                    keyList = keyList.FindAll(p => p.EndsWith("Bank"));
                }
                else
                {
                    keyList = keyList.FindAll(p => !p.EndsWith("Bank"));
                }
            }
            else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            {
                if (_mapParamView)
                {
                    keyList = keyList.FindAll(p => ParamBank.DS2MapParamlist.Contains(p.Split('_')[0]));
                }
                else
                {
                    keyList = keyList.FindAll(p => !ParamBank.DS2MapParamlist.Contains(p.Split('_')[0]));
                }
            }
            else if (Smithbox.ProjectType is ProjectType.ER || Smithbox.ProjectType is ProjectType.AC6)
            {
                if (_eventParamView)
                {
                    keyList = keyList.FindAll(p => p.StartsWith("EFID"));
                }
                else
                {
                    keyList = keyList.FindAll(p => !p.StartsWith("EFID"));
                }

                if (_gConfigParamView)
                {
                    if(Smithbox.ProjectType == ProjectType.AC6)
                    {
                        keyList = keyList.FindAll(p => p.StartsWith("GraphicsConfig"));
                    }
                    else
                    {
                        keyList = keyList.FindAll(p => p.StartsWith("Gconfig"));
                    }
                }
                else
                {
                    if (Smithbox.ProjectType == ProjectType.AC6)
                    {
                        keyList = keyList.FindAll(p => !p.StartsWith("GraphicsConfig"));
                    }
                    else
                    {
                        keyList = keyList.FindAll(p => !p.StartsWith("Gconfig"));
                    }
                }
            }

            if (CFG.Current.Param_AlphabeticalParams)
            {
                keyList.Sort();
            }

            return keyList;
        });

        foreach (var paramKey in paramKeyList)
        {
            HashSet<int> primary = ParamBank.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
            Param p = ParamBank.PrimaryBank.Params[paramKey];

            if (p != null)
            {
                var meta = ParamMetaData.Get(p.AppliedParamdef);
                var Wiki = meta?.Wiki;
                if (Wiki != null)
                {
                    if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                    {
                        meta.Wiki = Wiki;
                    }
                }
            }

            ImGui.Indent(15.0f * scale);

            if (primary != null ? primary.Any() : false)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_PrimaryChanged_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            }

            if (ImGui.Selectable($"{paramKey}", paramKey == _selection.GetActiveParam()))
            {
                //_selection.setActiveParam(param.Key);
                EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
            }

            ImGui.PopStyleColor();

            if (doFocus && paramKey == _selection.GetActiveParam())
            {
                scrollTo = ImGui.GetCursorPosY();
            }

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("Pin " + paramKey) &&
                    !Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.Contains(paramKey))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.Add(paramKey);
                }

                if (ParamEditorScreen.EditorMode && p != null)
                {
                    var meta = ParamMetaData.Get(p.AppliedParamdef);

                    if (meta != null && meta.Wiki == null && ImGui.MenuItem("Add wiki..."))
                    {
                        meta.Wiki = "Empty wiki...";
                    }

                    if (meta?.Wiki != null && ImGui.MenuItem("Remove wiki"))
                    {
                        meta.Wiki = null;
                    }
                }

                ImGui.EndPopup();
            }

            // Map alias
            DisplayDS2MapNameAlias(paramKey);

            ImGui.Unindent(15.0f * scale);
        }

        if (doFocus)
        {
            ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
        }
    }

    private void DisplayDS2MapNameAlias(string paramKey)
    {
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (_mapParamView)
            {
                Regex pattern = new Regex(@"(m[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2})");
                Match match = pattern.Match(paramKey);

                var alias = "";

                if (match.Captures.Count > 0)
                {
                    var aliasRef = Smithbox.BankHandler.MapAliases.GetEntries()
                        .Where(e => e.Key == match.Captures[0].Value)
                        .FirstOrDefault();

                    if (aliasRef.Key != null)
                    {
                        alias = aliasRef.Value.name;
                    }
                }

                AliasUtils.DisplayAlias(alias);
            }
        }
    }

    private void ParamView_ParamList(bool doFocus, bool isActiveView, float scale, float scrollTo)
    {
        ParamView_ParamList_Header(isActiveView);

        if(CFG.Current.Param_PinnedRowsStayVisible)
        {
            ParamView_ParamList_Pinned(scale);
        }

        ImGui.BeginChild("paramTypes");

        if (!CFG.Current.Param_PinnedRowsStayVisible)
        {
            ParamView_ParamList_Pinned(scale);
        }

        if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedParams)
        {
            ParamView_ParamList_Main(doFocus, scale, scrollTo);
        }

        ImGui.EndChild();
    }
    //------------------------------------
    // Row
    //------------------------------------
    private void ParamView_RowList_Header(ref bool doFocus, bool isActiveView, ref float scrollTo,
        string activeParam)
    {
        ImGui.Text("Rows");
        ImGui.Separator();

        scrollTo = 0;

        if (ImGui.Button($"Go to selected <{KeyBindings.Current.PARAM_GoToSelectedRow.HintText}>") ||
            isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_GoToRowID))
        {
            _paramEditor.GotoSelectedRow = true;
        }

        ImGui.SameLine();

        if (ImGui.Button($"Go to ID <{KeyBindings.Current.PARAM_GoToRowID.HintText}>") ||
            isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_GoToRowID))
        {
            ImGui.OpenPopup("gotoParamRow");
        }

        if (ImGui.BeginPopup("gotoParamRow"))
        {
            var gotorow = 0;
            ImGui.SetKeyboardFocusHere();
            ImGui.InputInt("Goto Row ID", ref gotorow);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                _gotoParamRow = gotorow;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        //Row ID/name search
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchRow))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.InputText($"Search <{KeyBindings.Current.PARAM_SearchRow.HintText}>",
            ref _selection.GetCurrentRowSearchString(), 256);
        var resAutoRow = AutoFill.RowSearchBarAutoFill();

        if (resAutoRow != null)
        {
            _selection.SetCurrentRowSearchString(resAutoRow);
        }

        if (!lastRowSearch.ContainsKey(_selection.GetActiveParam()) || !lastRowSearch[_selection.GetActiveParam()]
                .Equals(_selection.GetCurrentRowSearchString()))
        {
            UICache.ClearCaches();
            lastRowSearch[_selection.GetActiveParam()] = _selection.GetCurrentRowSearchString();
            doFocus = true;
        }

        if (ImGui.IsItemActive())
        {
            _paramEditor._isSearchBarActive = true;
        }
        else
        {
            _paramEditor._isSearchBarActive = false;
        }

        UIHints.AddImGuiHintButton("MassEditHint", ref UIHints.SearchBarHint);

        ImGui.Separator();
    }

    private void ParamView_RowList(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        if (!_selection.ActiveParamExists())
        {
            ImGui.Text("Select a param to see rows");
        }
        else
        {
            IParamDecorator decorator = null;

            if (_paramEditor._decorators.ContainsKey(activeParam))
            {
                decorator = _paramEditor._decorators[activeParam];
            }

            ParamView_RowList_Header(ref doFocus, isActiveView, ref scrollTo, activeParam);

            Param para = ParamBank.PrimaryBank.Params[activeParam];
            HashSet<int> vanillaDiffCache = ParamBank.PrimaryBank.GetVanillaDiffRows(activeParam);
            var auxDiffCaches = ParamBank.AuxBanks.Select((bank, i) =>
                (bank.Value.GetVanillaDiffRows(activeParam), bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Param.Column compareCol = _selection.GetCompareCol();
            PropertyInfo compareColProp = typeof(Param.Cell).GetProperty("Value");

            //ImGui.BeginChild("rows" + activeParam);
            if (EditorDecorations.ImGuiTableStdColumns("rowList", compareCol == null ? 1 : 2, false))
            {
                var meta = ParamMetaData.Get(ParamBank.PrimaryBank.Params[activeParam].AppliedParamdef);
                var pinnedRowList = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows
                    .GetValueOrDefault(activeParam, new List<int>()).Select(id => para[id]).ToList();

                ImGui.TableSetupColumn("rowCol", ImGuiTableColumnFlags.None, 1f);
                if (compareCol != null)
                {
                    ImGui.TableSetupColumn("rowCol2", ImGuiTableColumnFlags.None, 0.4f);
                    if (CFG.Current.Param_PinnedRowsStayVisible)
                    {
                        ImGui.TableSetupScrollFreeze(2, 1 + pinnedRowList.Count);
                    }
                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text("ID\t\tName");
                    }

                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text(compareCol.Def.InternalName);
                    }
                }
                else
                {
                    if (CFG.Current.Param_PinnedRowsStayVisible)
                    {
                        ImGui.TableSetupScrollFreeze(1, pinnedRowList.Count);
                    }
                }

                ImGui.PushID("pinned");

                var selectionCachePins = _selection.GetSelectionCache(pinnedRowList, "pinned");
                if (pinnedRowList.Count != 0)
                {
                    var lastCol = false;
                    for (var i = 0; i < pinnedRowList.Count(); i++)
                    {
                        Param.Row row = pinnedRowList[i];
                        if (row == null)
                        {
                            continue;
                        }

                        lastCol = ParamView_RowList_Entry(selectionCachePins, i, activeParam, null, row,
                            vanillaDiffCache, auxDiffCaches, decorator, ref scrollTo, false, true, compareCol,
                            compareColProp, meta);
                    }

                    if (lastCol)
                    {
                        ImGui.Spacing();
                    }

                    if (EditorDecorations.ImguiTableSeparator())
                    {
                        ImGui.Spacing();
                    }
                }

                ImGui.PopID();

                // Up/Down arrow key input
                if ((InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)) && !ImGui.IsAnyItemActive())
                {
                    _arrowKeyPressed = true;
                }

                if (_focusRows)
                {
                    ImGui.SetNextWindowFocus();
                    _arrowKeyPressed = false;
                    _focusRows = false;
                }

                List<Param.Row> rows = UICache.GetCached(_paramEditor, (_viewIndex, activeParam),
                    () => RowSearchEngine.rse.Search((ParamBank.PrimaryBank, para),
                        _selection.GetCurrentRowSearchString(), true, true));

                var enableGrouping = !CFG.Current.Param_DisableRowGrouping && meta.ConsecutiveIDs;

                // Rows
                var selectionCache = _selection.GetSelectionCache(rows, "regular");
                
                bool allowReorder = CFG.Current.Param_AllowRowReorder;
                if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedRows && allowReorder)
                {
                    List<string> rowOrder;
                    if (!(meta?.AlternateRowOrders?.TryGetValue(activeParam, out var defRowOrder) ?? false))
                    {
                        rowOrder = [];
                        defRowOrder = null;
                    }
                    else
                    {
                        rowOrder = [..defRowOrder];
                    }
                    HashSet<string> rowOrderSet = [..rowOrder];

                    foreach (var row in para.Rows)
                    {
                        if (!rowOrderSet.Contains(row.ID.ToString()))
                        {
                            rowOrder.Add(row.ID.ToString());
                            rowOrderSet.Add(row.ID.ToString());
                        }
                    }

                    if (meta != null && ParamEditorScreen.EditorMode)
                    {
                        meta.AlternateRowOrders ??= [];
                        if (defRowOrder == null)
                            meta.AlternateRowOrders.Add(activeParam, [..rowOrder]);
                        else if (defRowOrder.Count != rowOrder.Count)
                            meta.AlternateRowOrders[activeParam] = [..rowOrder];
                    }

                    Dictionary<string, (int, Param.Row)> rowD = rows
                        .Select((r, i) => (i, r))
                        .ToDictionary(t => t.r.ID.ToString());

                    foreach (string rowId in rowOrder)
                    {
                        if (!rowD.TryGetValue(rowId, out var row))
                            continue;
                        ParamView_RowList_Entry(selectionCache, row.Item1, activeParam, rows, row.Item2, vanillaDiffCache,
                            auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp,
                            meta);
                    }
                } 
                else if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedRows)
                {
                    for (var i = 0; i < rows.Count; i++)
                    {
                        Param.Row currentRow = rows[i];
                        if (enableGrouping)
                        {
                            Param.Row prev = i - 1 > 0 ? rows[i - 1] : null;
                            Param.Row next = i + 1 < rows.Count ? rows[i + 1] : null;
                            if (prev != null && next != null && prev.ID + 1 != currentRow.ID &&
                                currentRow.ID + 1 == next.ID)
                            {
                                EditorDecorations.ImguiTableSeparator();
                            }

                            ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                                auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp,
                                meta);

                            if (prev != null && next != null && prev.ID + 1 == currentRow.ID &&
                                currentRow.ID + 1 != next.ID)
                            {
                                EditorDecorations.ImguiTableSeparator();
                            }
                        }
                        else
                        {
                            ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                                auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp,
                                meta);
                        }
                    }
                }

                if (doFocus)
                {
                    ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
                }

                ImGui.EndTable();
            }
            //ImGui.EndChild();
        }
    }

    //------------------------------------
    // Field
    //------------------------------------
    private void ParamView_FieldList_Header(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ImGui.Text("Fields");
        ImGui.Separator();
    }

    private void ParamView_FieldList(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ParamView_FieldList_Header(isActiveView, activeParam, activeRow);

        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");
            ImGui.Text("Select a row to see properties");
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);
            Param vanillaParam = ParamBank.VanillaBank.Params?.GetValueOrDefault(activeParam);

            _propEditor.PropEditorParamRow(
                ParamBank.PrimaryBank,
                activeRow,
                vanillaParam?[activeRow.ID],
                ParamBank.AuxBanks.Select((bank, i) =>
                    (bank.Key, bank.Value.Params?.GetValueOrDefault(activeParam)?[activeRow.ID])).ToList(),
                _selection.GetCompareRow(),
                ref _selection.GetCurrentPropSearchString(),
                activeParam,
                isActiveView,
                _selection);

            ImGui.EndChild();
        }
    }

    public void ParamView(bool doFocus, bool isActiveView)
    {
        var scale = Smithbox.GetUIScale();

        if (EditorDecorations.ImGuiTableStdColumns("paramsT", 3, true))
        {
            ImGui.TableSetupColumn("paramsCol", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn("paramsCol2", ImGuiTableColumnFlags.None, 0.5f);
            var scrollTo = 0f;
            if (ImGui.TableNextColumn())
            {
                ParamView_ParamList(doFocus, isActiveView, scale, scrollTo);
            }

            var activeParam = _selection.GetActiveParam();
            if (ImGui.TableNextColumn())
            {
                ParamView_RowList(doFocus, isActiveView, scrollTo, activeParam);
            }

            Param.Row activeRow = _selection.GetActiveRow();
            if (ImGui.TableNextColumn())
            {
                ParamView_FieldList(isActiveView, activeParam, activeRow);
            }

            ImGui.EndTable();
        }
    }

    private void ParamView_RowList_Entry_Row(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, IParamDecorator decorator, ref float scrollTo,
        bool doFocus, bool isPinned, ParamMetaData? meta)
    {
        var diffVanilla = vanillaDiffCache.Contains(r.ID);
        var auxDiffVanilla = auxDiffCaches.Where(cache => cache.Item1.Contains(r.ID)).Count() > 0;

        if (diffVanilla)
        {
            // If the auxes are changed bu
            var auxDiffPrimaryAndVanilla = (auxDiffVanilla ? 1 : 0) + auxDiffCaches
                .Where(cache => cache.Item1.Contains(r.ID) && cache.Item2.Contains(r.ID)).Count() > 1;

            if (auxDiffVanilla && auxDiffPrimaryAndVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_AuxConflict_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_PrimaryChanged_Text);
            }
        }
        else
        {
            if (auxDiffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_AuxAdded_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            }
        }

        var selected = selectionCache != null && selectionCacheIndex < selectionCache.Length
            ? selectionCache[selectionCacheIndex]
            : false;

        if (_gotoParamRow != -1 && !isPinned)
        {
            // Goto row was activated. As soon as a corresponding ID is found, change selection to it.
            if (r.ID == _gotoParamRow)
            {
                selected = true;
                _selection.SetActiveRow(r, true);
                _gotoParamRow = -1;
                ImGui.SetScrollHereY();
            }
        }

        if (_paramEditor.GotoSelectedRow && !isPinned)
        {
            var activeRow = _selection.GetActiveRow();

            if (activeRow == null)
            {
                _paramEditor.GotoSelectedRow = false;
            }
            else if (activeRow.ID == r.ID)
            {
                ImGui.SetScrollHereY();
                _paramEditor.GotoSelectedRow = false;
            }
        }

        var label = $@"{r.ID} {Utils.ImGuiEscape(r.Name)}";
        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            CFG.Current.Param_DisableLineWrapping ? 1 : 3);

        if (ImGui.Selectable($@"{label}##{selectionCacheIndex}", selected))
        {
            _focusRows = true;

            if (InputTracker.GetKey(Key.LControl) || InputTracker.GetKey(Key.RControl))
            {
                _selection.ToggleRowInSelection(r);
            }
            else if (p != null && (InputTracker.GetKey(Key.LShift) || InputTracker.GetKey(Key.RShift)) && _selection.GetActiveRow() != null)
            {
                _selection.CleanSelectedRows();
                var start = p.IndexOf(_selection.GetActiveRow());
                var end = p.IndexOf(r);

                if (start != end && start != -1 && end != -1)
                {
                    foreach (Param.Row r2 in p.GetRange(start < end ? start : end, Math.Abs(end - start)))
                    {
                        _selection.AddRowToSelection(r2);
                    }
                }

                _selection.AddRowToSelection(r);
            }
            else
            {
                _selection.SetActiveRow(r, true);
            }
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused() && r != _selection.GetActiveRow())
        {
            if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
            {
                // Add to selection
                _selection.AddRowToSelection(r);
            }
            else
            {
                // Exclusive selection
                _selection.SetActiveRow(r, true);
            }

            _arrowKeyPressed = false;
        }

        ImGui.PopStyleColor();

        // Param Context Menu
        if (ImGui.BeginPopupContextItem(r.ID.ToString()))
        {
            if (CFG.Current.Param_RowContextMenu_NameInput)
            {
                if (_selection.RowSelectionExists())
                {
                    var name = _selection.GetActiveRow().Name;
                    if (name != null)
                    {
                        ImGui.InputText("##rowName", ref name, 255);
                        _selection.GetActiveRow().Name = name;
                    }
                }
            }

            if (CFG.Current.Param_RowContextMenu_ShortcutTools)
            {
                if (ImGui.Selectable(@$"Copy selection ({KeyBindings.Current.PARAM_CopyToClipboard.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.CopySelectionToClipboard(_selection);
                }

                ImGui.Separator();

                if (ImGui.Selectable(@$"Paste clipboard ({KeyBindings.Current.PARAM_PasteClipboard.HintText})", false,
                        ParamBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }

                ImGui.Separator();

                if (ImGui.Selectable(@$"Delete selection ({KeyBindings.Current.CORE_DeleteSelectedEntry.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.DeleteSelection(_selection);
                }

                ImGui.Separator();

                if (ImGui.Selectable(@$"Duplicate selection ({KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.ActionSubMenu.Handler.DuplicateHandler();
                }

                ImGui.Separator();
            }

            if (CFG.Current.Param_RowContextMenu_PinOptions)
            {
                if (ImGui.Selectable((isPinned ? "Unpin " : "Pin ") + r.ID))
                {
                    if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.ContainsKey(activeParam))
                    {
                        Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Add(activeParam, new List<int>());
                    }

                    List<int> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam];

                    if (isPinned)
                    {
                        pinned.Remove(r.ID);
                    }
                    else if (!pinned.Contains(r.ID))
                    {
                        pinned.Add(r.ID);
                    }
                }

                if(_selection.GetSelectedRows().Count > 0)
                {
                    if (ImGui.Selectable($"Pin Selection##pinSelection{r.ID}"))
                    {
                        foreach(var entry in _selection.GetSelectedRows())
                        {
                            if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.ContainsKey(activeParam))
                            {
                                Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Add(activeParam, new List<int>());
                            }

                            List<int> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam];

                            if (!pinned.Contains(entry.ID))
                            {
                                pinned.Add(entry.ID);
                            }
                        }
                    }
                }
                if (_selection.GetSelectedRows().Count > 0)
                {
                    if (ImGui.Selectable($"Unpin Selection##unpinSelection{r.ID}"))
                    {
                        foreach (var entry in _selection.GetSelectedRows())
                        {
                            if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.ContainsKey(activeParam))
                            {
                                Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Add(activeParam, new List<int>());
                            }

                            List<int> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam];

                            if (pinned.Contains(entry.ID))
                            {
                                pinned.Remove(entry.ID);
                            }
                        }
                    }
                }

                if (isPinned)
                {
                    EditorDecorations.PinListReorderOptions(Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam], r.ID);
                }

                if (ImGui.Selectable("Unpin all"))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Clear();
                }

                ImGui.Separator();
            }

            if (decorator != null)
            {
                decorator.DecorateContextMenuItems(r);
            }

            if (CFG.Current.Param_RowContextMenu_CompareOptions)
            {
                if (ImGui.Selectable("Compare..."))
                {
                    _selection.SetCompareRow(r);
                }
            }

            if (CFG.Current.Param_RowContextMenu_ReverseLoopup)
            {
                EditorDecorations.ParamRefReverseLookupSelectables(_paramEditor, ParamBank.PrimaryBank, activeParam, r.ID);
            }

            if (CFG.Current.Param_RowContextMenu_CopyID)
            {
                if (ImGui.Selectable("Copy ID to clipboard"))
                {
                    PlatformUtils.Instance.SetClipboardText($"{r.ID}");
                }
            }
            
            if (ParamEditorScreen.EditorMode && !isPinned && CFG.Current.Param_AllowRowReorder && 
                (meta?.AlternateRowOrders?.TryGetValue(activeParam, out var rowOrder) ?? false))
            {
                ImGui.Separator();
                EditorDecorations.ListReorderOptions(rowOrder, r.ID.ToString());
            }

            ImGui.EndPopup();
        }

        if (decorator != null)
        {
            decorator.DecorateParam(r);
        }

        if (doFocus && _selection.GetActiveRow() == r)
        {
            scrollTo = ImGui.GetCursorPosY();
        }
    }

    private bool ParamView_RowList_Entry(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, IParamDecorator decorator, ref float scrollTo,
        bool doFocus, bool isPinned, Param.Column compareCol, PropertyInfo compareColProp, ParamMetaData? meta)
    {
        var scale = Smithbox.GetUIScale();

        if (CFG.Current.UI_CompactParams)
        {
            // ItemSpacing only affects clickable area for selectables in tables. Add additional height to prevent gaps between selectables.
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 2.0f) * scale);
        }

        var lastCol = false;

        if (ImGui.TableNextColumn())
        {
            ParamView_RowList_Entry_Row(selectionCache, selectionCacheIndex, activeParam, p, r, vanillaDiffCache,
                auxDiffCaches, decorator, ref scrollTo, doFocus, isPinned, meta);
            lastCol = true;
        }

        if (compareCol != null)
        {
            if (ImGui.TableNextColumn())
            {
                Param.Cell c = r[compareCol];
                object newval = null;
                ImGui.PushID("compareCol_" + selectionCacheIndex);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ParamEditorCommon.PropertyField(compareCol.ValueType, c.Value, ref newval, false, false);

                if (ParamEditorCommon.UpdateProperty(_propEditor.ContextActionManager, c, compareColProp,
                        c.Value) && !ParamBank.VanillaBank.IsLoadingParams)
                {
                    ParamBank.PrimaryBank.RefreshParamRowDiffs(r, activeParam);
                }

                ImGui.PopStyleVar();
                ImGui.PopID();
                lastCol = true;
            }
            else
            {
                lastCol = false;
            }
        }

        if (CFG.Current.UI_CompactParams)
        {
            ImGui.PopStyleVar();
        }

        return lastCol;
    }
}
