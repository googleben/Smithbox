using ImGuiNET;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.FsBrowser.BrowserFs
{
    public class TextFsEntry : FsEntry
    {
        internal bool isInitialized = false;
        public override bool IsInitialized => isInitialized;
        private string name;
        public override string Name => name;
        public override bool CanHaveChildren => false;
        public override bool CanView => true;
        public override List<FsEntry> Children => [];

        internal Func<Memory<byte>> getDataFunc;
        internal Memory<byte>? data = null;
        internal string? contents = null;
        
        public TextFsEntry(string name, Func<Memory<byte>> getDataFunc)
        {
            this.name = name;
            this.getDataFunc = getDataFunc;
        }
        
        public override void Load()
        {
            data = getDataFunc();
            try
            {
                contents = System.Text.Encoding.UTF8.GetString(data.Value.Span);
            }
            catch (Exception e1)
            {
                TaskLogs.AddLog($"Failed to decode file {name} as UTF-8, trying UTF-16...", LogLevel.Debug, ex: e1);
                try
                {
                    contents = System.Text.Encoding.Unicode.GetString(data.Value.Span);
                }
                catch (Exception e)
                {
                    contents = $"ERROR: COULD NOT DECODE FILE CONTENTS: {e.ToString()}";
                    TaskLogs.AddLog($"Failed to decode contents of file {name} as UTF-8 or UTF-16", LogLevel.Warning, ex: e);
                }
            }

            isInitialized = true;
        }

        internal override void UnloadInner()
        {
            contents = null;
            data = null;
            isInitialized = false;
        }

        public override void OnGui()
        {
            ImGui.Text($"Text file: {name}");
            ImGui.InputTextMultiline("Text File Contents", ref contents, uint.MaxValue,
                ImGui.GetWindowSize() - new Vector2(5, 5), ImGuiInputTextFlags.ReadOnly);
        }
    }
}