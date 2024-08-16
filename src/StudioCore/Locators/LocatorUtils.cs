using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Locators;

public static class LocatorUtils
{
    public static bool IsTPF(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".TPF") || upperPath.EndsWith(".TPF.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsFLVER(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".FLVER") || upperPath.EndsWith(".FLV") || upperPath.EndsWith(".FLVER.DCX") || upperPath.EndsWith(".FLV.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsNavmesh(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".NVM"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsHavokNavmesh(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsHavokCollision(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static List<string> GetAssetFiles(string paramDir, string paramExt, bool ignoreProject = false)
    {
        try
        {
            HashSet<string> fileList = new();
            List<string> ret = new();
            foreach (var f in Smithbox.VanillaFS.GetFileNamesWithExtensions(paramDir, paramExt))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                fileList.Add(name);
            }

            if (ignoreProject)
                return ret;
            
            foreach (var f in Smithbox.ProjectFS.GetFileNamesWithExtensions(paramDir, paramExt))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                fileList.Add(name);
            }
            
            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static string GetAssetPath(string relpath)
    {
        if (Smithbox.FS.FileExists(relpath))
            return relpath;
        if (Smithbox.FS.FileExists(relpath + ".dcx"))
            return relpath + ".dcx";
        //throw new Exception($"Failed to locate file {relpath}");
        TaskLogs.AddLog($"Failed to locate file {relpath}", LogLevel.Warning);
        return relpath;
        //TODO
        if (Smithbox.ProjectRoot != null)
        {
            var modpath = $@"{Smithbox.ProjectRoot}\{relpath}";
            if (File.Exists(modpath))
                return modpath;
        }

        return $@"{Smithbox.GameRoot}\{relpath}";
    }
    public static string GetAssetPath_CollisionHack(string relpath)
    {
        return $@"{CFG.Current.PTDE_Collision_Root}\{relpath}";
    }

    public static bool CheckFilesExpanded(string gamepath, ProjectType game)
    {
        if (game is ProjectType.ER or ProjectType.AC6)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\asset"))
                return false;
        }

        if (game is ProjectType.DS1 or ProjectType.DS3 or ProjectType.SDT)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\obj"))
                return false;
        }

        if (game == ProjectType.DS2S || game == ProjectType.DS2)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\model\obj"))
                return false;
        }

        return true;
    }

    public static bool FileExists(string relpath)
    {
        return Smithbox.FS.FileExists(relpath);
        //TODO
        if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{relpath}"))
            return true;

        if (File.Exists($@"{Smithbox.GameRoot}\{relpath}"))
            return true;

        return false;
    }

    public static string GetOverridenFilePath(string relpath)
    {
        return Smithbox.FS.FileExists(relpath) ? relpath : null;
        //TODO
        var rootPath = $@"{Smithbox.GameRoot}\{relpath}";
        var modPath = $@"{Smithbox.ProjectRoot}\{relpath}";

        if (Smithbox.ProjectRoot != null && File.Exists(modPath))
            return modPath;

        if (File.Exists($@"{rootPath}"))
            return rootPath;

        return null;
    }

    

    
}
