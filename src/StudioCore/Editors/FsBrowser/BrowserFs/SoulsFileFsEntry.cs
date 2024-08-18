﻿using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.FsBrowser.BrowserFs
{
    public abstract class SoulsFileFsEntry : FsEntry
    {
        public static SoulsFileFsEntry? TryGetFor(string fileName, Func<Memory<byte>> getDataFunc, VirtualFileSystem? vfs = null, string? path = null)
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

            if (fileName.EndsWith("bdt"))
            {
                if (vfs == null)
                {
                    TaskLogs.AddLog($"Bdt file {fileName} can't construct an FsEntry without a vfs!", LogLevel.Warning);
                    return null;
                }

                //check if this bdt file is a dvdbnd (top-level binder)
                var vfile = vfs.GetFile(path);
                if (vfile is RealVirtualFileSystem.RealVirtualFile vf)
                {
                    bool isDvdbnd = Regex.IsMatch(fileName.ToLower(), @"(.*dvdbnd.*)|(.*ebl\.bdt)|(.*data\d\.bdt)");
                    if (!isDvdbnd)
                    {
                        using (var fs = vf.GetFileStream())
                        {
                            var buf = new byte[0x50];
                            fs.ReadExactly(buf.AsSpan());
                            isDvdbnd = !BXF3.IsBDT(buf) && !BXF4.IsBDT(buf);
                        }
                    }
                    if (isDvdbnd)
                    {
                        TaskLogs.AddLog($"Binder file {fileName} appears to be a dvdbnd bdt", LogLevel.Debug, TaskLogs.LogPriority.Low);
                        string bhdPath = path.Replace(".bdt", ".bhd");
                        if (Smithbox.ProjectType == ProjectType.DS1) bhdPath += "5";
                        if (!vfs.FileExists(bhdPath))
                        {
                            TaskLogs.AddLog($"Couldn't find bhd for bdt file {fileName}", LogLevel.Warning);
                            return null;
                        }

                        return new DvdBndFsEntry(
                            fileName,
                            () => vf.GetFileStream(),
                            () => vfs.ReadFile(bhdPath).Value);
                    }
                }

                var getBhdData = FindBhd(fileName, vfs, path);
                if (getBhdData == null)
                {
                    TaskLogs.AddLog($"Could not find corresponding bhd for bdt \"{fileName}\"!", LogLevel.Warning);
                    return null;
                }

                if (Smithbox.ProjectType == ProjectType.DS1
                    || Smithbox.ProjectType == ProjectType.DS1R
                    || Smithbox.ProjectType == ProjectType.DES)
                    return new Bxf3FsEntry(fileName, getDataFunc, getBhdData);
                else
                    return new Bxf4FsEntry(fileName, getDataFunc, getBhdData);
            }

            if (fileName.EndsWith("bhd") || fileName.EndsWith("bhd5"))
            {
                var data = getDataFunc();
                if (!BXF3.IsBHD(data) && !BXF4.IsBHD(data))
                {
                    TaskLogs.AddLog($"Binder file {fileName} appears to be a dvdbnd bhd", LogLevel.Debug, TaskLogs.LogPriority.Low);
                    return new DvdBndFsEntry(fileName, null, getDataFunc);
                }
                if (Smithbox.ProjectType == ProjectType.DS1
                    || Smithbox.ProjectType == ProjectType.DS1R
                    || Smithbox.ProjectType == ProjectType.DES)
                    return new Bhd3FsEntry(fileName, getDataFunc);
                else
                    return new Bhd4FsEntry(fileName, getDataFunc);
            }

            if (fileName.EndsWith("tpf") || fileName.EndsWith("tpf.dcx"))
                return new TpfFsEntry(fileName, getDataFunc);
            if (fileName.EndsWith("flver"))
                return new FlverFsEntry(fileName, getDataFunc);
            return null;
        }

        /// <summary>
        /// Try to find the bhd corresponding to a given bdt. If found, returns a function that reads the bhd.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="vfs"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Func<Memory<byte>>? FindBhd(string fileName, VirtualFileSystem vfs, string path)
        {
            var bhdName = fileName.Replace("bdt", "bhd");
            var bhdPath = path.Replace("bdt", "bhd");
            Func<Memory<byte>>? getDataFuncBhd = null;
            if (vfs.FileExists(bhdPath))
            {
                getDataFuncBhd = () => vfs.ReadFile(bhdPath).Value;
            }
            else
            {
                if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                {
                    if (fileName.EndsWith("chrtpfbdt"))
                    {
                        //chrtpfbdt files have their bhd in the chrbnd
                        string? chrbndPath = null;
                        var tmp = path.Replace(".chrtpfbdt", ".chrbnd");
                        if (vfs.FileExists(tmp))
                            chrbndPath = tmp;
                        else if (vfs.FileExists(tmp + ".dcx"))
                            chrbndPath = tmp + ".dcx";
                        else
                        {
                            TaskLogs.AddLog($"Could not find chdbnd corresponding to chrbdt {fileName}!",
                                LogLevel.Warning);
                        }

                        if (chrbndPath != null)
                        {
                            getDataFuncBhd = () =>
                            {
                                using var bnd = new BND3Reader(vfs.ReadFile(chrbndPath).Value);
                                BinderFileHeader? bhd = null;
                                foreach (var f in bnd.Files)
                                {
                                    if (f.Name.EndsWith(bhdName))
                                    {
                                        bhd = f;
                                        break;
                                    }
                                }

                                if (bhd == null)
                                {
                                    throw new FileNotFoundException(
                                        $"Could not find {bhdName} expected to be in {chrbndPath}!");
                                }

                                //we have to copy the data to an owned array because we'll be releasing the bnd
                                //at return, meaning we are no longer owning or leasing the Memory<byte> the
                                //bnd was constructed with, which in turn means our lease on the Memory returned
                                //from bnd.ReadFile is invalid, if that memory was leased and not created.

                                //This really makes me appreciate just how much the Rust borrow checker is doing.
                                var read = bnd.ReadFile(bhd);
                                var newmem = new Memory<byte>(new byte[read.Length]);
                                read.CopyTo(newmem);
                                return newmem;
                            };
                        }
                    }
                }
            }

            return getDataFuncBhd;
        }
    }
}