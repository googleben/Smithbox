using Andre.IO.VFS;
using DotNext.Collections.Generic;
using DotNext.Threading.Tasks;
using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Interface.Modals;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using static StudioCore.CFG;

namespace StudioCore.Core;

public class ProjectHandler
{
    public Project CurrentProject;

    public ProjectModal ProjectModal;

    public Timer AutomaticSaveTimer;

    public CFG.RecentProject RecentProject;

    public bool IsInitialLoad = false;
    public bool ShowProjectLoadSelection = true;
    public bool RecentProjectLoad = false;

    public bool ImportRowNames = false;

    public bool IsLoadingProject = false;
    private Action? finishLoadingProject = null;
    private bool loadingPopupOpen = false;

    public ProjectHandler()
    {
        CurrentProject = new Project();
        ProjectModal = new ProjectModal();

        IsInitialLoad = true;
        UpdateProjectVariables();
    }
    public void OnGui()
    {
        if (IsLoadingProject)
        {
            if (!loadingPopupOpen)
            {
                ImGui.OpenPopup("Project Loading");
            }
            if (ImGui.BeginPopupModal("Project Loading", ref IsLoadingProject, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Loading project. If your bhds are encrypted, this may take a while.");
                ImGui.EndPopup();
            }
            return;
        }

        if (loadingPopupOpen)
        {
            loadingPopupOpen = false;
        }

        if (finishLoadingProject != null)
        {
            finishLoadingProject();
            finishLoadingProject = null;
        }
        if (!RecentProjectLoad && CFG.Current.Project_LoadRecentProjectImmediately)
        {
            RecentProjectLoad = true;
            IsInitialLoad = false;
            try
            {
                Smithbox.ProjectHandler.LoadProjectFromJSON(CFG.Current.LastProjectFile);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog("Failed to load recent project.");
            }
        }

        if (IsInitialLoad)
        {
            ImGui.OpenPopup("Project Creation");
        }

        if (ImGui.BeginPopupModal("Project Creation", ref IsInitialLoad, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ProjectModal.Display();

            ImGui.EndPopup();
        }
    }

    public void ReloadCurrentProject()
    {
        LoadProjectFromJSON(CurrentProject.ProjectJsonPath);
        Smithbox.ProjectHandler.IsInitialLoad = false;
    }

    public Task<bool> LoadProject(string path)
    {
        IsLoadingProject = true;
        async Task<bool> LoadProjectInner()
        {
            if (CurrentProject.Config == null)
            {
                PlatformUtils.Instance.MessageBox("Failed to load last project. Project will not be loaded after restart.", "Project Load Error", MessageBoxButtons.OK);
                return false;
            }

            if (path == "")
            {
                PlatformUtils.Instance.MessageBox($"Path parameter was empty: {path}", "Project Load Error", MessageBoxButtons.OK);
                return false;
            }

            CurrentProject.ProjectJsonPath = path;

            SetGameRootPrompt(CurrentProject);
            CheckUnpackedState(CurrentProject);

            // Only proceed if dll are found
            if (!CheckDecompressionDLLs(CurrentProject)) return false;

            var projectType = CurrentProject.Config.GameType;
            var gameRoot = CurrentProject.Config.GameRoot;
            var projectRoot = Path.GetDirectoryName(path) ?? "";
            
            //load filesystems
            await UpdateFilesystems(projectRoot, gameRoot, projectType);

            finishLoadingProject = () =>
            {
                Smithbox.ProjectType = projectType;
                Smithbox.GameRoot = gameRoot;
                Smithbox.ProjectRoot = projectRoot;
                Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";

                if (Smithbox.ProjectRoot == "") TaskLogs.AddLog("Smithbox.ProjectRoot is empty!");

                Smithbox.SetProgramTitle($"{CurrentProject.Config.ProjectName} - Smithbox");

                MapLocator.FullMapList = null;
                Smithbox.InitializeBanks();
                Smithbox.InitializeNameCaches();
                Smithbox.EditorHandler.UpdateEditors();

                CFG.Current.LastProjectFile = path;
                CFG.Save();

                AddProjectToRecentList(CurrentProject);

                UpdateTimer();

                // Re-create this so project setup settings don't persist between projects (e.g. Import Row Names)
                ProjectModal = new ProjectModal();
            };
            

            return true;
        }

        var ret = Task.Run(LoadProjectInner);

        ret.ContinueWith(t =>
        {
            IsLoadingProject = false;
            return t.Result;
        });
        return ret;
    }

    public async Task<bool> LoadProjectFromJSON(string jsonPath)
    {
        if (CurrentProject == null)
        {
            CurrentProject = new Project();
        }

        // Fill CurrentProject.Config with contents
        CurrentProject.Config = ReadProjectConfig(jsonPath);

        if (CurrentProject.Config == null)
        {
            return false;
        }

        return await LoadProject(jsonPath);
    }

    public void ClearProject()
    {
        CurrentProject = null;
        Smithbox.SetProgramTitle("No Project - Smithbox");
        Smithbox.ProjectType = ProjectType.Undefined;
        Smithbox.GameRoot = "";
        Smithbox.ProjectRoot = "";
        Smithbox.SmithboxDataRoot = "";

        MapLocator.FullMapList = null;
    }

    public async void UpdateProjectVariables()
    {
        var projectType = CurrentProject.Config.GameType;
        var gameRoot = CurrentProject.Config.GameRoot;
        var projectRoot = 
            Smithbox.ProjectRoot = Path.GetDirectoryName(CurrentProject.ProjectJsonPath) ?? "";
        await UpdateFilesystems(projectRoot, gameRoot, projectType);
        Smithbox.SetProgramTitle($"{CurrentProject.Config.ProjectName} - Smithbox");
        Smithbox.ProjectType = projectType;
        Smithbox.GameRoot = gameRoot;
        Smithbox.ProjectRoot = projectRoot;
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";
    }

    private static Task UpdateFilesystems(string? projectRoot, string? gameRoot, ProjectType projectType)
    {
        return Task.Run(() =>
        {
            List<VirtualFileSystem> fileSystems = [];
            Smithbox.ProjectFS.Dispose();
            Smithbox.VanillaRealFS.Dispose();
            Smithbox.VanillaBinderFS.Dispose();
            Smithbox.VanillaFS.Dispose();
            Smithbox.FS.Dispose();

            //it's important that we do the project FS first, because it needs to be the first element of the
            //filesystem list, so that its files take precedence over vanilla files.
            if ((projectRoot ?? "") != "")
            {
                Smithbox.ProjectFS = new RealVirtualFileSystem(projectRoot, false);
                fileSystems.Add(Smithbox.ProjectFS);
            }
            else
            {
                Smithbox.ProjectFS = EmptyVirtualFileSystem.Instance;
            }

            if ((gameRoot ?? "") != "")
            {
                Smithbox.VanillaRealFS = new RealVirtualFileSystem(gameRoot, false);
                fileSystems.Add(Smithbox.VanillaRealFS);
                var andreGame = projectType.AsAndreGame();
                if (andreGame != null)
                {
                    Smithbox.VanillaBinderFS =
                        ArchiveBinderVirtualFileSystem.FromGameFolder(gameRoot, andreGame.Value);
                    fileSystems.Add(Smithbox.VanillaBinderFS);
                    Smithbox.VanillaFS =
                        new CompundVirtualFileSystem([Smithbox.VanillaRealFS, Smithbox.VanillaBinderFS]);
                }
                else
                {
                    Smithbox.VanillaBinderFS = EmptyVirtualFileSystem.Instance;
                    Smithbox.VanillaFS = Smithbox.VanillaRealFS;
                }
            }
            else
            {
                Smithbox.VanillaRealFS = EmptyVirtualFileSystem.Instance;
                Smithbox.VanillaFS = EmptyVirtualFileSystem.Instance;
            }

            if (fileSystems.Count == 0) Smithbox.FS = EmptyVirtualFileSystem.Instance;
            else Smithbox.FS = new CompundVirtualFileSystem(fileSystems);
        });
    }

    public void AddProjectToRecentList(Project targetProject)
    {
        // Add to recent project list
        CFG.RecentProject recent = new()
        {
            Name = targetProject.Config.ProjectName,
            GameType = targetProject.Config.GameType,
            ProjectFile = targetProject.ProjectJsonPath
        };
        CFG.AddMostRecentProject(recent);
    }

    public ProjectConfiguration ReadProjectConfig(string path)
    {
        var config = new ProjectConfiguration();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                config = JsonSerializer.Deserialize(stream, ProjectConfigurationSerializationContext.Default.ProjectConfiguration);
            }
        }

        return config;
    }

    public void WriteProjectConfig(Project targetProject)
    {
        if(targetProject == null) 
            return;

        var config = targetProject.Config;
        var writePath = targetProject.ProjectJsonPath;

        if (writePath != "")
        {
            string jsonString = JsonSerializer.Serialize(config, typeof(ProjectConfiguration), ProjectConfigurationSerializationContext.Default);

            try
            {
                var fs = new FileStream(writePath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    public void SetGameRootPrompt(Project targetProject)
    {
        if (targetProject == null)
            return;

        if (!Directory.Exists(targetProject.Config.GameRoot))
        {
            PlatformUtils.Instance.MessageBox(
                $@"Could not find game data directory for {targetProject.Config.GameType}. Please select the game executable.",
                "Error",
                MessageBoxButtons.OK);

            while (true)
            {
                if (PlatformUtils.Instance.OpenFileDialog(
                        $"Select executable for {targetProject.Config.GameType}...",
                        new[] { FilterStrings.GameExecutableFilter },
                        out var path))
                {
                    targetProject.Config.GameRoot = path;
                    ProjectType gametype = GetProjectTypeFromExecutable(targetProject.Config.GameRoot);

                    if (gametype == targetProject.Config.GameType)
                    {
                        targetProject.Config.GameRoot = Path.GetDirectoryName(targetProject.Config.GameRoot);

                        if (targetProject.Config.GameType == ProjectType.BB)
                        {
                            targetProject.Config.GameRoot += @"\dvdroot_ps4";
                        }

                        WriteProjectConfig(targetProject);

                        break;
                    }

                    PlatformUtils.Instance.MessageBox(
                        $@"Selected executable was not for {CurrentProject.Config.GameType}. Please select the correct game executable.",
                        "Error",
                        MessageBoxButtons.OK);
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void CheckUnpackedState(Project targetProject)
    {
        if (targetProject == null)
            return;

        /*if (!LocatorUtils.CheckFilesExpanded(targetProject.Config.GameRoot, targetProject.Config.GameType))
        {
            if (targetProject.Config.GameType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
            {
                TaskLogs.AddLog(
                    $"The files for {targetProject.Config.GameType} do not appear to be unpacked. Please use UDSFM for DS1:PTDE and UXM for DS2 to unpack game files",
                    LogLevel.Error, TaskLogs.LogPriority.High);
            }

            TaskLogs.AddLog(
                $"The files for {targetProject.Config.GameType} do not appear to be fully unpacked. Functionality will be limited. Please use UXM selective unpacker to unpack game files",
                LogLevel.Warning);
        }*/
    }

    public bool CheckDecompressionDLLs(Project targetProject)
    {
        if (targetProject == null)
            return false;

        bool success = true;

        if (targetProject.Config.GameType == ProjectType.SDT || targetProject.Config.GameType == ProjectType.ER)
        {
            success = false;
            success = StealGameDllIfMissing(targetProject, "oo2core_6_win64");
        }
        else if (targetProject.Config.GameType == ProjectType.AC6)
        {
            success = false;
            success = StealGameDllIfMissing(targetProject, "oo2core_8_win64");
        }

        return success;
    }

    public bool StealGameDllIfMissing(Project targetProject, string dllName)
    {
        if (targetProject == null)
            return false;

        dllName = dllName + ".dll";

        var rootDllPath = Path.Join(targetProject.Config.GameRoot, dllName);
        var projectDllPath = Path.Join(Path.GetFullPath("."), dllName);

        if (!File.Exists(rootDllPath))
        {
            PlatformUtils.Instance.MessageBox(
                $"Could not find file \"{dllName}\" in \"{targetProject.Config.GameRoot}\", which should be included by default.\n\nTry verifying or reinstalling the game.",
                "Error",
                MessageBoxButtons.OK);
            return false;
        }
        else
        {
            if(!File.Exists(projectDllPath))
            {
                File.Copy(rootDllPath, projectDllPath);
            }
        }

        return true;
    }

    public ProjectType GetProjectTypeFromExecutable(string exePath)
    {
        var type = ProjectType.Undefined;

        if (exePath.ToLower().Contains("darksouls.exe"))
        {
            type = ProjectType.DS1;
        }
        else if (exePath.ToLower().Contains("darksoulsremastered.exe"))
        {
            type = ProjectType.DS1R;
        }
        else if (exePath.ToLower().Contains("darksoulsii.exe"))
        {
            type = ProjectType.DS2S; // Default to SOTFS
        }
        else if (exePath.ToLower().Contains("darksoulsiii.exe"))
        {
            type = ProjectType.DS3;
        }
        else if (exePath.ToLower().Contains("eboot.bin"))
        {
            var path = Path.GetDirectoryName(exePath);
            if (Directory.Exists($@"{path}\dvdroot_ps4"))
            {
                type = ProjectType.BB;
            }
            else
            {
                type = ProjectType.DES;
            }
        }
        else if (exePath.ToLower().Contains("sekiro.exe"))
        {
            type = ProjectType.SDT;
        }
        else if (exePath.ToLower().Contains("eldenring.exe"))
        {
            type = ProjectType.ER;
        }
        else if (exePath.ToLower().Contains("armoredcore6.exe"))
        {
            type = ProjectType.AC6;
        }

        return type;
    }

    public void UpdateTimer()
    {
        if (AutomaticSaveTimer != null)
        {
            AutomaticSaveTimer.Close();
        }

        if (CFG.Current.System_EnableAutoSave)
        {
            var interval = CFG.Current.System_AutoSaveIntervalSeconds * 1000;
            if (interval < 10000)
                interval = 10000;

            AutomaticSaveTimer = new Timer(interval);
            AutomaticSaveTimer.Elapsed += OnAutomaticSave;
            AutomaticSaveTimer.AutoReset = true;
            AutomaticSaveTimer.Enabled = true;
        }
    }

    public void SaveCurrentProject()
    {
        WriteProjectConfig(CurrentProject);
    }

    public void OnAutomaticSave(object source, ElapsedEventArgs e)
    {
        if (CFG.Current.System_EnableAutoSave)
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {
                if (CFG.Current.System_EnableAutoSave_Project)
                {
                    WriteProjectConfig(CurrentProject);
                }

                if (CFG.Current.System_EnableAutoSave_MapEditor)
                {
                    Smithbox.EditorHandler.MapEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_ModelEditor)
                {
                    Smithbox.EditorHandler.ModelEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_ParamEditor)
                {
                    Smithbox.EditorHandler.ParamEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_TextEditor)
                {
                    Smithbox.EditorHandler.TextEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_GparamEditor)
                {
                    Smithbox.EditorHandler.GparamEditor.SaveAll();
                }

                TaskLogs.AddLog($"Automatic Save occured at {e.SignalTime}");
            }
        }
    }

    public bool CreateRecoveryProject()
    {
        if (Smithbox.GameRoot == null || Smithbox.ProjectRoot == null)
            return false;

        try
        {
            var time = DateTime.Now.ToString("dd-MM-yyyy-(hh-mm-ss)", CultureInfo.InvariantCulture);

            Smithbox.ProjectRoot = Smithbox.ProjectRoot + $@"\recovery\{time}";

            if (!Directory.Exists(Smithbox.ProjectRoot))
            {
                Directory.CreateDirectory(Smithbox.ProjectRoot);
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public void OpenProjectDialog()
    {
        var success = PlatformUtils.Instance.OpenFileDialog("Choose the project json file", new[] { FilterStrings.ProjectJsonFilter }, out var projectJsonPath);

        if (projectJsonPath != null)
        {
            if (projectJsonPath.Contains("project.json"))
            {
                LoadProjectFromJSON(projectJsonPath).ContinueWith((t) =>
                {
                    if (t.Result)
                        Smithbox.ProjectHandler.IsInitialLoad = false;
                });
            }
        }
    }

    public void LoadRecentProject()
    {
        // Only set this to false if recent project load is sucessful
        LoadProjectFromJSON(Current.LastProjectFile).ContinueWith((t) =>
        {
            if (t.Result) Smithbox.ProjectHandler.IsInitialLoad = false;
        });
    }


    public void DisplayRecentProjects()
    {
        RecentProject = null;
        var id = 0;

        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DES
            if (p.GameType == ProjectType.DES)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS1
            if (p.GameType == ProjectType.DS1)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS1R
            if (p.GameType == ProjectType.DS1R)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS2
            if (p.GameType == ProjectType.DS2)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS2S
            if (p.GameType == ProjectType.DS2S)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // BB
            if (p.GameType == ProjectType.BB)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS3
            if (p.GameType == ProjectType.DS3)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // SDT
            if (p.GameType == ProjectType.SDT)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // ER
            if (p.GameType == ProjectType.ER)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // AC6
            if (p.GameType == ProjectType.AC6)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
    }

    public void RecentProjectEntry(CFG.RecentProject p, int id)
    {
        // Just remove invalid recent projects immediately
        if(!File.Exists(p.ProjectFile))
        {
            CFG.RemoveRecentProject(p);
        }

        if (ImGui.MenuItem($@"{p.GameType}: {p.Name}##{id}"))
        {
            if (File.Exists(p.ProjectFile))
            {
                var path = p.ProjectFile;

                LoadProjectFromJSON(path).ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        Smithbox.ProjectHandler.IsInitialLoad = false;
                    }
                    else
                    {
                        // Remove it if it failed
                        CFG.RemoveRecentProject(p);
                    }
                });
            }
            else
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"Project file at \"{p.ProjectFile}\" does not exist.\n\n" +
                    $"Remove project from list of recent projects?",
                    $"Project.json cannot be found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CFG.RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove from list"))
            {
                CFG.RemoveRecentProject(p);
                CFG.Save();
            }

            ImGui.EndPopup();
        }
    }
}
