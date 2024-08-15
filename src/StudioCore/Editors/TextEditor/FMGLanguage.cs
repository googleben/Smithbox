﻿using Microsoft.Extensions.Logging;
using Silk.NET.Core;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.HKXPWV;
using static StudioCore.TextEditor.FMGBank;

namespace StudioCore.Editors.TextEditor;

/*
 * FMGLanguage represents a grouped set of FMGFileSets containing FMGInfos sourced from the same language, within a project's FMGBank.
 */
public class FMGLanguage
{
    internal FMGLanguage(string language)
    {
        LanguageFolder = language;
    }

    internal readonly string LanguageFolder;
    internal bool IsLoaded => _FmgInfoBanks.Count != 0 && _FmgInfoBanks.All((fs) => fs.Value.IsLoaded);
    internal bool IsLoading => _FmgInfoBanks.Count != 0 && _FmgInfoBanks.Any((fs) => fs.Value.IsLoading);
    internal readonly Dictionary<FmgFileCategory, FMGFileSet> _FmgInfoBanks = new();

    /// <summary>
    ///     Loads item and menu MsgBnds from paths, generates FMGInfo, and fills FmgInfoBank.
    /// </summary>
    /// <returns>True if successful; false otherwise.</returns>
    internal bool LoadItemMenuMsgBnds(ResourceDescriptor itemMsgPath, ResourceDescriptor menuMsgPath)
    {
        FMGFileSet itemMsgBnd = new FMGFileSet(FmgFileCategory.Item);

        if (itemMsgBnd.LoadMsgBnd(itemMsgPath.AssetPath, "item.msgbnd"))
            _FmgInfoBanks.Add(itemMsgBnd.FileCategory, itemMsgBnd);

        FMGFileSet menuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);

        if (menuMsgBnd.LoadMsgBnd(menuMsgPath.AssetPath, "menu.msgbnd"))
            _FmgInfoBanks.Add(menuMsgBnd.FileCategory, menuMsgBnd);

        if (_FmgInfoBanks.Count == 0)
            return false;

        return true;
    }

    internal bool LoadNormalFmgs()
    {
        ResourceDescriptor itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder);
        ResourceDescriptor menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder);

        if(Smithbox.ProjectType is ProjectType.ER)
        {
            itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "_dlc02");
            menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "_dlc02");
        }
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "_dlc2");
            menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "_dlc2");
        }

        if (LoadItemMenuMsgBnds(itemMsgPath, menuMsgPath))
        {
            return true;
        }
        return false;

    }
    internal bool LoadDS2FMGs()
    {
        ResourceDescriptor desc = TextLocator.GetItemMsgbnd(LanguageFolder, true);

        if (desc.AssetPath == null)
        {
            if (LanguageFolder != "")
            {
                TaskLogs.AddLog($"Could not locate text data files when using \"{LanguageFolder}\" folder",
                    LogLevel.Warning);
            }
            else
            {
                TaskLogs.AddLog("Could not locate text data files when using Default English folder",
                    LogLevel.Warning);
            }
            return false;
        }

        List<string> files = Directory
            .GetFileSystemEntries($@"{Smithbox.GameRoot}\{desc.AssetPath}", @"*.fmg").ToList();

        FMGFileSet looseMsg = new FMGFileSet(FmgFileCategory.Loose);
        if (looseMsg.LoadLooseMsgsDS2(files))
        {
            _FmgInfoBanks.Add(looseMsg.FileCategory, looseMsg);
            return true;
        }
        return false;
    }

    public void SaveFMGs()
    {
        try
        {
            if (!IsLoaded)
            {
                return;
            }

            if (Smithbox.ProjectType == ProjectType.Undefined)
            {
                return;
            }

            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                SaveFMGsDS2();
            }
            else
            {
                SaveFMGsNormal();
            }
            TaskLogs.AddLog("Saved FMG text");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddLog(e.Wrapped.Message,
                LogLevel.Error, TaskLogs.LogPriority.High, e.Wrapped);
        }
    }

    private void SaveFMGsDS2()
    {
        foreach (FMGInfo info in _FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos))
        {
            Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS,
                $@"menu\text\{LanguageFolder}\{info.Name}.fmg", info.Fmg);
        }
    }
    private void SaveFMGsNormal()
    {
        // Load the fmg bnd, replace fmgs, and save
        IBinder fmgBinderItem;
        IBinder fmgBinderMenu;
        ResourceDescriptor itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder);
        ResourceDescriptor menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder);

        // Handle output types for ER
        if(Smithbox.ProjectType is ProjectType.ER)
        {
            switch(Smithbox.EditorHandler.TextEditor.CurrentTargetOutputMode)
            {
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.Vanilla:
                    itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC1:
                    itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "_dlc01");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "_dlc01");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC2:
                    itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "_dlc02");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "_dlc02");
                    break;
            }
        }

        // Handle output types for DS3
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            switch (Smithbox.EditorHandler.TextEditor.CurrentTargetOutputMode)
            {
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.Vanilla:
                    itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC1:
                    itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "_dlc1");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "_dlc1");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC2:
                    itemMsgPath = TextLocator.GetItemMsgbnd(LanguageFolder, false, "_dlc2");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(LanguageFolder, false, "_dlc2");
                    break;
            }
        }

        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            fmgBinderItem = BND3.Read(Smithbox.FS.GetFile(itemMsgPath.AssetPath).GetData());
            fmgBinderMenu = BND3.Read(Smithbox.FS.GetFile(menuMsgPath.AssetPath).GetData());
        }
        else
        {
            fmgBinderItem = BND4.Read(Smithbox.FS.GetFile(itemMsgPath.AssetPath).GetData());
            fmgBinderMenu = BND4.Read(Smithbox.FS.GetFile(menuMsgPath.AssetPath).GetData());
        }

        // Item
        foreach (BinderFile file in fmgBinderItem.Files)
        {
            FMGInfo info = _FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos).FirstOrDefault(e => e.FmgID == (FmgIDType)file.ID);
            
            if (info != null)
            {
                file.Bytes = info.Fmg.Write();
            }
        }

        // Menu
        foreach (BinderFile file in fmgBinderMenu.Files)
        {
            FMGInfo info = _FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos).FirstOrDefault(e => e.FmgID == (FmgIDType)file.ID);
            if (info != null)
            {
                file.Bytes = info.Fmg.Write();
            }
        }

        ResourceDescriptor itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true);
        ResourceDescriptor menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true);

        // Handle output types for ER
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            switch (Smithbox.EditorHandler.TextEditor.CurrentTargetOutputMode)
            {
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.Vanilla:
                    itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true, "");
                    menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true, "");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC1:
                    itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true, "_dlc01");
                    menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true, "_dlc01");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC2:
                    itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true, "_dlc02");
                    menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true, "_dlc02");
                    break;
            }
        }

        // Handle output types for ER
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            switch (Smithbox.EditorHandler.TextEditor.CurrentTargetOutputMode)
            {
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.Vanilla:
                    itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true, "");
                    menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true, "");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC1:
                    itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true, "_dlc1");
                    menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true, "_dlc1");
                    break;
                case StudioCore.TextEditor.TextEditorScreen.TargetOutputMode.DLC2:
                    itemMsgPathDest = TextLocator.GetItemMsgbnd(LanguageFolder, true, "_dlc2");
                    menuMsgPathDest = TextLocator.GetMenuMsgbnd(LanguageFolder, true, "_dlc2");
                    break;
            }
        }

        if (fmgBinderItem is BND3 bnd3)
        {
            Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS, itemMsgPathDest.AssetPath, bnd3);
            Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS, menuMsgPathDest.AssetPath, (BND3)fmgBinderMenu);
            if (Smithbox.ProjectType is ProjectType.DES)
            {
                bnd3.Compression = DCX.Type.None;
                ((BND3)fmgBinderMenu).Compression = DCX.Type.None;
                Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS, itemMsgPathDest.AssetPath[..^4], bnd3);
                Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS, menuMsgPathDest.AssetPath[..^4], (BND3)fmgBinderMenu);
            }
        }
        else if (fmgBinderItem is BND4 bnd4)
        {
            Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS, itemMsgPathDest.AssetPath, bnd4);
            Utils.WriteWithBackup(Smithbox.FS, Smithbox.ProjectFS, menuMsgPathDest.AssetPath, (BND4)fmgBinderMenu);
        }

        fmgBinderItem.Dispose();
        fmgBinderMenu.Dispose();
    }
}
