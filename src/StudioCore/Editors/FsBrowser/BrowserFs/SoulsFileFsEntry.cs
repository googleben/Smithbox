using StudioCore.Core;
using System;

namespace StudioCore.Editors.FsBrowser.BrowserFs
{
    public abstract class SoulsFileFsEntry : FsEntry
    {
        public static SoulsFileFsEntry? TryGetFor(string fileName, Func<Memory<byte>> getDataFunc)
        {
            if (fileName.EndsWith("bnd") || fileName.EndsWith("bnd.dcx"))
            {
                if (Smithbox.ProjectType == ProjectType.DS1
                    || Smithbox.ProjectType == ProjectType.DS1R
                    || Smithbox.ProjectType == ProjectType.DES)
                    return new Bnd3FsEntry(fileName, getDataFunc);
                else
                    return new Bnd4FsEntry(fileName, getDataFunc);
            }

            if (fileName.EndsWith("tpf") || fileName.EndsWith("tpf.dcx"))
                return new TpfFsEntry(fileName, getDataFunc);
            if (fileName.EndsWith("flver"))
                return new FlverFsEntry(fileName, getDataFunc);
            return null;
        }
    }
}