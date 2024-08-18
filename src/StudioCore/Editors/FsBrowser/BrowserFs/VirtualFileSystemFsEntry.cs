using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace StudioCore.Editors.FsBrowser.BrowserFs
{
    public class VirtualFileSystemFsEntry : FsEntry
    {
        public override bool IsInitialized => inner.IsInitialized;
        private string name;
        public override string Name => name;
        public override bool CanHaveChildren => true;
        public override bool CanView => false;
        private VirtualFileSystemDirectoryFsEntry inner;
        public override List<FsEntry> Children => inner?.Children ?? [];

        private VirtualFileSystem vfs;

        public VirtualFileSystemFsEntry(VirtualFileSystem vfs, string name)
        {
            this.vfs = vfs;
            this.name = $"{name} ({vfs.GetType().Name})";
            this.inner = new VirtualFileSystemDirectoryFsEntry(vfs, "", "");
        }
        
        public override void Load()
        {
            inner.Load();
        }

        internal override void UnloadInner()
        {
            inner.UnloadInner();
        }
    }

    public class VirtualFileSystemDirectoryFsEntry : FsEntry
    {
        private bool isInitialized = false;
        public override bool IsInitialized => isInitialized;
        private string name;
        public override string Name => name;
        public override bool CanHaveChildren => true;
        public override bool CanView => false;
        private List<FsEntry> children = [];
        public override List<FsEntry> Children => children;

        private VirtualFileSystem vfs;
        private string path;

        public VirtualFileSystemDirectoryFsEntry(VirtualFileSystem vfs, string parentPath, string name)
        {
            this.name = name;
            this.vfs = vfs;
            this.path = $"{parentPath}/{name}";
        }
        
        public override void Load()
        {
            var dir = vfs.GetDirectory(path);
            if (dir == null)
            {
                TaskLogs.AddLog($"Failed to load dir {path}", LogLevel.Warning);
                return;
            }

            foreach (var dirname in dir.EnumerateDirectoryNames())
            {
                children.Add(new VirtualFileSystemDirectoryFsEntry(vfs, path, dirname));
            }

            foreach (var filename in dir.EnumerateFileNames())
            {
                var child = 
                    TryGetFor(filename, () => vfs.ReadFile($"{path}/{filename}").Value) 
                    ?? (FsEntry)new VirtualFileSystemFileFsEntry(filename);
                children.Add(child);
            }

            isInitialized = true;
        }

        internal override void UnloadInner()
        {
            children.ForEach(c => c.UnloadInner());
            children.Clear();
            isInitialized = false;
        }

    }
    
    /// <summary>
    /// Represents a file in a VirtualFileSystem for which we have no bespoke FsEntry
    /// </summary>
    public class VirtualFileSystemFileFsEntry : FsEntry {
        public override bool IsInitialized => true;
        private string name;
        public override string Name => name;
        public override bool CanHaveChildren => false;
        public override bool CanView => false;
        private static List<FsEntry> children = [];
        public override List<FsEntry> Children => children;

        public VirtualFileSystemFileFsEntry(string name)
        {
            this.name = name;
        }
            
        public override void Load() { }

        internal override void UnloadInner() { }
    
    }
}