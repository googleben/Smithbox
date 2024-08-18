using Andre.IO.VFS;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Resource;
using StudioCore.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        private BlockingCollection<Action>? tasks;

        private Task? loadingTask = null;
        public bool IsLoading => loadingTask != null;

        //TODO: Replace this with a more general solution for project-wide off-gui tasks
        public Task LoadAsync()
        {
            if (loadingTask != null) return loadingTask;
            var cts = new TaskCompletionSource();
            loadingTask = cts.Task;
            if (tasks == null)
            {
                tasks = new();
                var t = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            tasks.Take()();
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog("Error on worker thread", LogLevel.Error, TaskLogs.LogPriority.High, e);
                        }
                    }
                }) { IsBackground = true };
                t.Start();
            }
            tasks.Add(() =>
            {
                Load();
                cts.SetResult();
                loadingTask = null;
            });
            return loadingTask;
        }

        internal void Load(Action<FsEntry> onUnload)
        {
            this.onUnload = onUnload;
            Load();
        }
        
        internal abstract void Load();

        internal abstract void UnloadInner();

        public void Unload()
        {
            if (!IsInitialized) return;
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

        public static FsEntry? TryGetFor(string fileName, Func<Memory<byte>> getDataFunc, VirtualFileSystem? vfs = null, string? path = null)
        {
            if (fileName.EndsWith(".txt"))
                return new TextFsEntry(fileName, getDataFunc);
            return SoulsFileFsEntry.TryGetFor(fileName, getDataFunc, vfs, path);
        }
    }
}