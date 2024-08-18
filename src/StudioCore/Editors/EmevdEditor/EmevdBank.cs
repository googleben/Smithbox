using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;
public static class EmevdBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<EventScriptInfo, EMEVD> ScriptBank { get; private set; } = new();
    public static EMEDF InfoBank { get; private set; } = new();

    public static bool IsSupported = false;

    public static void LoadEMEDF()
    {
        IsSupported = false;

        var path = "";
        switch(Smithbox.ProjectType)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds1-common.emedf.json";
                break;
            case ProjectType.DS2:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds2-common.emedf.json";
                break;
            case ProjectType.DS2S:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds2scholar-common.emedf.json";
                break;
            case ProjectType.BB:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//bb-common.emedf.json";
                break;
            case ProjectType.DS3:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds3-common.emedf.json";
                break;
            case ProjectType.SDT:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//sekiro-common.emedf.json";
                break;
            case ProjectType.ER:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//er-common.emedf.json";
                break;
            case ProjectType.AC6:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ac6-common.emedf.json";
                break;
            default: break;
        }

        if(IsSupported)
            InfoBank = EMEDF.ReadFile(path);
    }

    public static void SaveEventScripts()
    {
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            SaveDS2EventScripts();
        }
        else
        {
            foreach (var (info, script) in ScriptBank)
            {
                SaveEventScript(info, script);
            }
        }
    }

    public static void SaveEventScript(EventScriptInfo info, EMEVD script)
    {
        if (script == null)
            return;

        // Ignore loaded scripts that have not been modified
        // This is to prevent mass-transfer to project folder on Save-All
        if(!info.IsModified)
            return;

        //TaskLogs.AddLog($"SaveEventScript: {info.Path}");

        byte[] fileBytes = null;

        switch (Smithbox.ProjectType)
        {
            case ProjectType.DS1:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS1R:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS3:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.SDT:
                fileBytes = script.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.ER:
                fileBytes = script.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.AC6:
                fileBytes = script.Write(DCX.Type.DCX_KRAK_MAX);
                break;
            default:
                TaskLogs.AddLog($"Invalid ProjectType during SaveEventScript");
                return;
        }

        var paramDir = @"\event\";
        var paramExt = @".emevd.dcx";
        var assetRoot = $@"{paramDir}\{info.Name}{paramExt}";

        Utils.TrySaveFile(assetRoot, fileBytes);
    }

    // parambank process here as emevd it within regulation.bin
    private static void SaveDS2EventScripts()
    {
        string regulation = "enc_regulation.bnd.dcx";
        
        if (!Smithbox.VanillaFS.FileExists(regulation))
        {
            TaskLogs.AddLog("Cannot locate regulation. Save failed.", LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        BND4 emevdBnd;
        var writeFs = Utils.GetFSForWrites();

        if (!writeFs.FileExists(regulation))
        {
            // If there is no mod file, check the base file. Decrypt it if you have to.
            var vanillaData = Smithbox.VanillaFS.ReadFile(regulation).Value;
            // Decrypt the file
            emevdBnd = SFUtil.DecryptDS2Regulation(vanillaData);

            // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
            if (!Smithbox.VanillaRealFS.FileExists($"{regulation}.bak"))
            {
                Smithbox.VanillaRealFS.WriteFile($"{regulation}.bak", vanillaData.ToArray());
                Smithbox.VanillaRealFS.WriteFile(regulation, emevdBnd.Write());
            }
        }
        else
        {
            emevdBnd = writeFs.ReadSoulsFile<BND4>(regulation);
        }

        // Write in edited EMEVD here
        foreach (var entry in ScriptBank)
        {
            var info = entry.Key;
            var script = entry.Value;

            if (info.IsModified)
            {
                foreach (BinderFile f in emevdBnd.Files)
                {
                    var scriptName = Path.GetFileNameWithoutExtension(f.Name);

                    if (!f.Name.ToUpper().EndsWith(".emevd"))
                    {
                        continue;
                    }

                    if (scriptName == info.Name)
                    {
                        var bytes = script.Write();
                        f.Bytes = bytes;
                    }
                }
            }
        }
        
        Utils.WriteWithBackup(Smithbox.VanillaRealFS, writeFs, @"enc_regulation.bnd.dcx", emevdBnd);
        emevdBnd.Dispose();
    }

    public static void LoadEventScripts()
    {
        IsLoaded = false;
        IsLoading = true;

        ScriptBank = new();

        var paramDir = @"\event";
        var paramExt = @".emevd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            LoadDS2EventScripts();
        }
        else
        {
            List<string> paramNames = MiscLocator.GetEventBinders();

            foreach (var name in paramNames)
            {
                var filePath = $"{paramDir}\\{name}{paramExt}";
                LoadEventScript(filePath);
            }

            IsLoaded = true;
            IsLoading = false;

            TaskLogs.AddLog($"Event Script Bank - Load Complete");
        }
    }

    private static void LoadEventScript(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading EMEVD file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading EMEVD file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        EventScriptInfo eventInfo = new EventScriptInfo(name, path);
        EMEVD eventScript = new EMEVD();

        try
        {
            eventScript = EMEVD.Read(DCX.Decompress(Smithbox.FS.GetFile(path).GetData()));
            ScriptBank.Add(eventInfo, eventScript);
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to read {path}");
        }
    }

    // parambank process here as emevd it within regulation.bin
    private static void LoadDS2EventScripts()
    {

        var regulationPath = $@"enc_regulation.bnd.dcx";

        BND4 emevdBnd = null;
        try
        {
            emevdBnd = SFUtil.DecryptDS2Regulation(Smithbox.FS.ReadFile(regulationPath).Value);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Regulation load failed: {regulationPath} - {e.Message}", LogLevel.Warning, TaskLogs.LogPriority.High, e);
        }

        LoadScriptsFromBinder(emevdBnd);
    }

    private static void LoadScriptsFromBinder(IBinder emevdBnd)
    {
        // Load every script in the regulation
        foreach (BinderFile f in emevdBnd.Files)
        {
            TaskLogs.AddLog(f.Name);
            var scriptName = Path.GetFileNameWithoutExtension(f.Name);
            EventScriptInfo info = new EventScriptInfo(scriptName, f.Name);

            if (!f.Name.ToUpper().EndsWith(".EMEVD"))
            {
                TaskLogs.AddLog("Skipped due to lacking .emevd");
                continue;
            }

            if (ScriptBank.ContainsKey(info))
            {
                TaskLogs.AddLog("Skipped as already added");
                continue;
            }

            try
            {
                EMEVD script = EMEVD.Read(f.Bytes);
                ScriptBank.Add(info, script);
                TaskLogs.AddLog($"{scriptName} added");
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load {scriptName}", LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Event Script Bank - Load Complete");
    }

    public class EventScriptInfo
    {
        public EventScriptInfo(string name, string path)
        {
            Name = name;
            Path = path;
            IsModified = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsModified { get; set; }
    }
}
