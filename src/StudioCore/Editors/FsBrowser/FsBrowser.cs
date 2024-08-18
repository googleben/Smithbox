using Andre.IO.VFS;
using HKLib.hk2018.TypeRegistryTest;
using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.FsBrowser.BrowserFs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.FsBrowser
{
    public class FsBrowser : EditorScreen
    {
        public string EditorName => "FS Browser";
        public string CommandEndpoint => "fsbrowser";
        public string SaveType => "(no save type for this editor!)";
        public bool FirstFrame { get; set; }
        public bool ShowSaveOption { get; set; }

        private List<VirtualFileSystemFsEntry> roots = [];

        private FsEntry? selected = null;

        public FsBrowser(Sdl2Window window, GraphicsDevice device)
        {
            
        }
        
        public void Init()
        {
            SetupVFSes();
        }

        public void OnProjectChanged()
        {
            SetupVFSes();
        }

        public void DrawEditorMenu()
        {
        }

        public void Save()
        {
            throw new System.NotImplementedException();
        }

        public void SaveAll()
        {
            throw new System.NotImplementedException();
        }

        public void OnGUI(string[] commands)
        {
            var scale = Smithbox.GetUIScale();

            // Docking setup
            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
            Vector2 wins = ImGui.GetWindowSize();
            Vector2 winp = ImGui.GetWindowPos();
            winp.Y += 20.0f * scale;
            wins.Y -= 20.0f * scale;
            ImGui.SetNextWindowPos(winp);
            ImGui.SetNextWindowSize(wins);

            var dsid = ImGui.GetID("DockSpace_FsBrowser");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);
            if (roots.Count == 0)
            {
                if (ImGui.Begin("FS Tree"))
                {
                    ImGui.Text("No FS roots available. Load a project.");
                    ImGui.End();
                }

                return;
            }

            if (ImGui.Begin("FS Tree"))
            {
                foreach (var root in roots)
                {
                    Traverse(root, $"FS Tree");
                }
                ImGui.End();
            }
            

            if (ImGui.Begin("FS Item Viewer"))
            {
                if (selected == null)
                {
                    ImGui.Text("Nothing selected");
                }
                else
                {
                    if (selected.CanView)
                    {
                        if (!selected.IsInitialized)
                            selected.Load();
                        selected.OnGui();
                    }
                    else
                    {
                        ImGui.Text($"Selected: {selected.Name}");
                        ImGui.Text("This file has no Item Viewer.");
                    }
                }
                ImGui.End();
            }
            
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);
        }

        private void Traverse(FsEntry e, string parentIdStr)
        {
            string id = $"{parentIdStr}##{e.Name}";
            var flags = ImGuiTreeNodeFlags.None;
            if (e is VirtualFileSystemFsEntry)
                flags |= ImGuiTreeNodeFlags.CollapsingHeader;
            if (!e.CanHaveChildren)
                flags |= ImGuiTreeNodeFlags.Leaf;
            if (selected == e)
                flags |= ImGuiTreeNodeFlags.Selected;
            ImGui.SetNextItemOpen(e.CanHaveChildren && e.IsInitialized);
            bool isOpen = ImGui.TreeNodeEx(id, flags, e.Name);
            if (ImGui.IsItemClicked())
            {
                if (!e.IsInitialized)
                {
                    e.Load();
                    Select(e);
                }
                else if (!e.CanHaveChildren)
                    Select(e);
                else
                    e.Unload();
                
            }
            
            if (isOpen)
            {
                if (!e.IsInitialized) e.Load();
                foreach (var child in e.Children.OrderBy(f => f.Name.ToLower()))
                {
                    Traverse(child, id);
                }
                if (!flags.HasFlag(ImGuiTreeNodeFlags.NoTreePushOnOpen)) ImGui.TreePop();
            } else if (e.IsInitialized)
            {
                e.UnloadInner();
            }
        }

        private void SetupVFSes()
        {
            roots.Clear();
            bool anyFs = false;
            bool vanillaFs = false;
            if (Smithbox.VanillaRealFS is not EmptyVirtualFileSystem) {
                roots.Add(new VirtualFileSystemFsEntry(Smithbox.VanillaRealFS, "Game Directory"));
                anyFs = true;
                vanillaFs = true;
            }

            if (Smithbox.VanillaBinderFS is not EmptyVirtualFileSystem)
            {
                roots.Add(new VirtualFileSystemFsEntry(Smithbox.VanillaBinderFS, "Vanilla Dvdbnds"));
                anyFs = true;
                vanillaFs = true;
            }

            if (vanillaFs && Smithbox.VanillaFS is not EmptyVirtualFileSystem)
                roots.Add(new VirtualFileSystemFsEntry(Smithbox.VanillaFS, "Full Vanilla FS"));

            if (Smithbox.ProjectFS is not EmptyVirtualFileSystem)
            {
                roots.Add(new VirtualFileSystemFsEntry(Smithbox.ProjectFS, "Project Directory"));
                anyFs = true;
            }

            if (anyFs && Smithbox.FS is not EmptyVirtualFileSystem)
                roots.Add(new VirtualFileSystemFsEntry(Smithbox.FS, "Full Combined FS"));
        }

        private void TryDeselect(FsEntry e)
        {
            if (selected == e)
            {
                selected = null;
            }
        }
    
        private void Select(FsEntry e)
        {
            if (selected == e) return;
            if (selected != null) selected.onUnload = null;
            selected = e;
            e.onUnload = TryDeselect;
        }
    }
}