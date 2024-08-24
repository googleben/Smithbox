using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GraphicsEditor;
public static class GparamParamBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static SortedDictionary<string, GparamInfo> VanillaParamBank { get; set; }
    public static SortedDictionary<string, GparamInfo> ParamBank { get; set; }

    public static void SaveGraphicsParams()
    {
        foreach (var (name, info) in ParamBank)
        {
            if (info.WasModified)
            {
                SaveGraphicsParam(info);
                info.WasModified = false;
            }
        }
    }

    public static void SaveGraphicsParam(GparamInfo info)
    {
        if (info == null)
            return;

        GPARAM param = info.Gparam;

        TaskLogs.AddLog($"SaveGraphicsParams: {info.Path}");

        byte[] fileBytes = null;

        switch (Smithbox.ProjectType)
        {
            case ProjectType.DS2:
            case ProjectType.DS2S:
                fileBytes = param.Write(DCX.Type.None);
                break;
            case ProjectType.BB:
            case ProjectType.DS3:
                fileBytes = param.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.SDT:
                fileBytes = param.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.ER:
                fileBytes = param.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.AC6:
                fileBytes = param.Write(DCX.Type.DCX_KRAK_MAX);
                break;
            default:
                TaskLogs.AddLog($"Invalid ProjectType during SaveGraphicsParam");
                return;
        }

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Smithbox.ProjectType == ProjectType.DS2S)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        var assetRoot = $@"{paramDir}\{info.Name}{paramExt}";

        Utils.TrySaveFile(assetRoot, fileBytes);
    }

    public static void LoadGraphicsParams()
    {
        IsLoaded = false;
        IsLoading = true;

        ParamBank = new();
        VanillaParamBank = new();

        var fs = Smithbox.FS;

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if(Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        // TODO: add support for DS2
        if(Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            return;
        }

        foreach (var name in GetGparamFileNames(fs))
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            LoadGraphicsParam($"{filePath}", true);

            LoadVanillaGraphicsParam($"{filePath}", false);
        }

        IsLoaded = true;
        IsLoading = false;

        //TaskLogs.AddLog($"Graphics Param Bank - Load Complete");
    }

    private static void LoadGraphicsParam(string path, bool isModFile)
    {
        var fs = Smithbox.FS;
        try
        {
            if (path == null)
            {
                TaskLogs.AddLog($"Could not locate {path} when loading GraphicsParam file.",
                        LogLevel.Warning);
                return;
            }
            if (path == "")
            {
                TaskLogs.AddLog($"Could not locate {path} when loading GraphicsParam file.",
                        LogLevel.Warning);
                return;
            }

            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
            GparamInfo gStruct = new GparamInfo(name, path);
            gStruct.Gparam = new GPARAM();
            gStruct.IsModFile = isModFile;

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                gStruct.Gparam = GPARAM.Read(fs.GetFile(path).GetData());
            }
            else
            {
                gStruct.Gparam = GPARAM.Read(DCX.Decompress(fs.GetFile(path).GetData()));
            }

            ParamBank.Add(name, gStruct);
        }
        catch(Exception e) 
        {
            TaskLogs.AddLog($"Failed to load {path}: {e.Message}");
        }
    }

    private static void LoadVanillaGraphicsParam(string path, bool isModFile)
    {
        var fs = Smithbox.VanillaFS;
        try
        {
            if (path == null)
            {
                TaskLogs.AddLog($"Could not locate {path} when loading GraphicsParam file.",
                        LogLevel.Warning);
                return;
            }
            if (path == "")
            {
                TaskLogs.AddLog($"Could not locate {path} when loading GraphicsParam file.",
                        LogLevel.Warning);
                return;
            }

            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
            GparamInfo gStruct = new GparamInfo(name, path);
            gStruct.Gparam = new GPARAM();
            gStruct.IsModFile = isModFile;

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                gStruct.Gparam = GPARAM.Read(fs.GetFile(path).GetData());
            }
            else
            {
                gStruct.Gparam = GPARAM.Read(DCX.Decompress(fs.GetFile(path).GetData()));
            }

            VanillaParamBank.Add(name, gStruct);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load {path}: {e.Message}");
        }
    }

    public static List<string> GetGparamFileNames(VirtualFileSystem fs)
    {
        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        List<string> ret = new();
        if (fs.DirectoryExists(paramDir))
        {
            var dir = fs.GetDirectory(paramDir)!;
            foreach (var file in dir.EnumerateFileNames())
            {
                if (!file.EndsWith(paramExt)) continue;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                ret.Add(name);
            }
        }

        return ret;
    }

    public class GparamInfo : IComparable<string>
    {
        public GparamInfo(string name, string path)
        {
            Name = name;
            Path = path;
            WasModified = false;
        }

        public GPARAM Gparam { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public bool IsModFile { get; set; }

        public bool WasModified { get; set; }

        public int CompareTo(string other)
        {
            return Name.CompareTo(other);
        }
    }
}
