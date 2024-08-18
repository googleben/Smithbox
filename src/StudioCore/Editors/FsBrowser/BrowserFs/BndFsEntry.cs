using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.FsBrowser.BrowserFs;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.FsBrowser.BrowserFs
{
    public abstract class BndFsEntry : SoulsFileFsEntry
    {
        internal bool isInitialized = false;
        public override bool IsInitialized => isInitialized;
        private string name;
        public override string Name => name;
        public override bool CanHaveChildren => true;
        private List<FsEntry> children = [];
        public override List<FsEntry> Children => children;
        internal Func<Memory<byte>> getDataFunc;
        internal Memory<byte>? data = null;
        
        public BndFsEntry(string name, Func<Memory<byte>> getDataFunc)
        {
            this.name = name;
            this.getDataFunc = getDataFunc;
        }

        internal void SetupChildren(BinderReader bnd)
        {
            foreach (var file in bnd.Files)
            {
                children.Add(new BndFileFsEntry(file.Name, () => bnd.ReadFile(file)));
            }
        }

        internal void ChildrenGui(BinderReader bnd)
        {
            foreach (var file in bnd.Files)
            {
                if (ImGui.CollapsingHeader(file.Name))
                {
                    ImGui.TreePush($"{file.Name}##TreePush");
                    PropertyTable($"{file.Name}##table", (row) =>
                    {
                        row("Compression Type", file.CompressionType.ToString());
                        row("ID", file.ID.ToString());
                        row("Flags", Utils.FlagsEnumToString(file.Flags));
                        row("Compressed Size", file.CompressedSize.ToString());
                        row("Uncompressed Size", file.UncompressedSize.ToString());
                        row("Data Offset", file.DataOffset.ToString());
                    });
                    ImGui.TreePop();
                }
            }
        }

        internal void ReaderRows(BinderReader reader, Action<string, string> row)
        {
            row("Version", reader.Version);
            row("Format", reader.Format.ToString());
            row("BigEndian", reader.BigEndian.ToString());
            row("BitBigEndian", reader.BitBigEndian.ToString());
        }

    }

    public class Bnd3FsEntry : BndFsEntry
    {
        private BND3Reader reader = null;
        
        public Bnd3FsEntry(string name, Func<Memory<byte>> getDataFunc) : base(name, getDataFunc)
        {
        }

        public override bool CanView => true;
        public override void Load()
        {
            data = getDataFunc();
            reader = new(data.Value);
            SetupChildren(reader);
            isInitialized = true;
        }

        internal override void UnloadInner()
        {
            Children.ForEach(c => c.Unload());
            Children.Clear();
            reader?.Dispose();
            reader = null;
            data = null;
            isInitialized = false;
        }

        public override void OnGui()
        {
            ImGui.Text($"BND3: {Name}");
            PropertyTable("BND", (row) =>
            {
                ReaderRows(reader, row);
                row("Compression", reader.Compression.ToString());
                row("Unk18", reader.Unk18.ToString());
            });
            ChildrenGui(reader);
        }
    }
    
    public class Bnd4FsEntry : BndFsEntry
    {
        private BND3Reader reader = null;
        
        public Bnd4FsEntry(string name, Func<Memory<byte>> getDataFunc) : base(name, getDataFunc)
        {
        }

        public override bool CanView => true;
        public override void Load()
        {
            data = getDataFunc();
            reader = new(data.Value);
            SetupChildren(reader);
            isInitialized = true;
        }

        internal override void UnloadInner()
        {
            Children.ForEach(c => c.Unload());
            Children.Clear();
            reader?.Dispose();
            reader = null;
            data = null;
            isInitialized = false;
        }

        public override void OnGui()
        {
            ImGui.Text($"BND4: {Name}");
            PropertyTable("BND", (row) =>
            {
                ReaderRows(reader, row);
                row("Compression", reader.Compression.ToString());
                row("Unk18", reader.Unk18.ToString());
            });
            ChildrenGui(reader);
        }
    }
    
    public class BndFileFsEntry : FsEntry
    {
        private FsEntry? inner = null;
        private bool isInitialized = false;
        public override bool IsInitialized => isInitialized;
        private string name;
        public override string Name => name;
        public override bool CanHaveChildren => inner?.CanHaveChildren ?? false;
        public override bool CanView => inner?.CanView ?? false;
        public override List<FsEntry> Children => inner?.Children ?? [];

        private Func<Memory<byte>> getDataFunc;
        
        public BndFileFsEntry(string name, Func<Memory<byte>> getDataFunc)
        {
            this.name = name;
            this.getDataFunc = getDataFunc;
            inner = FsEntry.TryGetFor(name, getDataFunc);
        }
    
        public override void Load()
        {
            inner?.Load();
            isInitialized = true;
        }

        internal override void UnloadInner()
        {
            inner?.Unload();
            isInitialized = false;
        }

        public override void OnGui()
        {
            inner?.OnGui();
        }
    }
}
