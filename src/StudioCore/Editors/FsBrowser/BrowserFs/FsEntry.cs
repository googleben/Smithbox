using ImGuiNET;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.FsBrowser.BrowserFs
{
    public abstract class FsEntry
    {
        public abstract bool IsInitialized { get; }

        public abstract string Name { get; }
        
        public abstract bool CanHaveChildren { get; }
        
        public abstract bool CanView { get; }
        
        public abstract List<FsEntry> Children { get; }

        public Action<FsEntry>? onUnload;

        public void Load(Action<FsEntry> onUnload)
        {
            this.onUnload = onUnload;
            Load();
        }
        
        public abstract void Load();

        internal abstract void UnloadInner();

        public void Unload()
        {
            UnloadInner();
            if (onUnload != null)
            {
                onUnload(this);
                onUnload = null;
            }
        }

        public virtual void OnGui()
        {
            throw new NotImplementedException($"Viewer not implemented for {GetType().FullName}");
        }

        internal static bool PropertyTable(string name, Action<Action<string, string>> rowsFunc)
        {
            if (!ImGui.BeginTable(name, 2))
                return false;
            ImGui.TableSetupColumn("Property");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            void Row(string a, string b)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(a);
                ImGui.TableNextColumn();
                ImGui.Text(b);
            }
            rowsFunc(Row);
            ImGui.EndTable();
            return true;
        }

        public static FsEntry? TryGetFor(string fileName, Func<Memory<byte>> getDataFunc)
        {
            if (fileName.EndsWith(".txt"))
                return new TextFsEntry(fileName, getDataFunc);
            return SoulsFileFsEntry.TryGetFor(fileName, getDataFunc);
        }
    }
}