using Octokit;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;
public static class TextLocator
{
    /// <summary>
    /// Get folders with msgbnds used in-game
    /// </summary>
    /// <returns>Dictionary with language name and path</returns>
    public static Dictionary<string, string> GetMsgLanguages()
    {
        Dictionary<string, string> dict = new();
        try
        {
            string rootPath;
            List<string> folders;
            if (Smithbox.ProjectType == ProjectType.DES)
            {
                rootPath = "msg";
                folders = Smithbox.FS.GetDirectory("msg").EnumerateDirectoryNames().ToList();
                // Japanese uses root directory
                if (Smithbox.FS.FileExists(@"\msg\menu.msgbnd.dcx") ||
                    Smithbox.FS.FileExists(@"\msg\item.msgbnd.dcx"))
                    dict.Add("Japanese", "");
            }
            else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                rootPath = "menu/text";
                folders = Smithbox.FS.GetDirectory("menu/text").EnumerateDirectoryNames().ToList();
            }
            else
            {
                rootPath = "msg";
                // Exclude folders that don't have typical msgbnds
                folders = Smithbox.FS.GetDirectory("msg").EnumerateDirectoryNames()
                    .Where(x => !"common,as,eu,jp,na,uk,japanese".Contains(x)).ToList();
            }

            foreach (var path in folders)
            {
                dict.Add(path, Path.Combine(rootPath, path));
            }
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
        }

        return dict;
    }

    /// <summary>
    /// Get path of item.msgbnd (english by default)
    /// </summary>
    public static ResourceDescriptor GetItemMsgbnd(string langFolder, bool writemode = false, string outputType = "")
    {
        return GetMsgbnd("item", langFolder, writemode, outputType);
    }

    /// <summary>
    /// Get path of menu.msgbnd (english by default)
    /// </summary>
    public static ResourceDescriptor GetMenuMsgbnd(string langFolder, bool writemode = false, string outputType = "")
    {
        return GetMsgbnd("menu", langFolder, writemode, outputType);
    }

    public static ResourceDescriptor GetMsgbnd(string msgBndType, string langFolder, bool writemode = false, string outputType = "")
    {
        ResourceDescriptor ad = new();
        var path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";

        if (Smithbox.ProjectType == ProjectType.DES)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
            // Demon's Souls has msgbnds directly in the msg folder
            if (!Smithbox.FS.FileExists(path))
                path = $@"msg\{msgBndType}.msgbnd.dcx";
        }
        else if (Smithbox.ProjectType == ProjectType.DS1)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd";
        }
        else if (Smithbox.ProjectType == ProjectType.DS1R)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        }
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            // DS2 does not have an msgbnd but loose fmg files instead
            path = $@"menu\text\{langFolder}";
            ResourceDescriptor ad2 = new();
            ad2.AssetPath = path;
            //TODO: doesn't support project files
            return ad2;
        }
        else if (Smithbox.ProjectType == ProjectType.DS3)
        {
            path = $@"msg\{langFolder}\{msgBndType}{outputType}.msgbnd.dcx";
        }
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            path = $@"msg\{langFolder}\{msgBndType}{outputType}.msgbnd.dcx";
        }

        if (writemode)
        {
            ad.AssetPath = path;
            return ad;
        }

        if (Smithbox.FS.FileExists(path))
            ad.AssetPath = path;

        return ad;
    }

    public static ResourceDescriptor GetMsgbnd_Vanilla_Upgrader(string msgBndType, string release, string langFolder, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        var path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";

        if (Smithbox.ProjectType == ProjectType.DS3)
        {
            path = $@"msg\{langFolder}\{msgBndType}{release}.msgbnd.dcx";
        }
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            path = $@"msg\{langFolder}\{msgBndType}{release}.msgbnd.dcx";
        }

        ad.AssetPath = $@"{path}";
        return ad;
    }

    public static ResourceDescriptor GetMsgbnd_Project_Upgrader(string msgBndType, string release, string langFolder, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        var path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";

        if (Smithbox.ProjectType == ProjectType.DS3)
        {
            path = $@"msg\{langFolder}\{msgBndType}{release}.msgbnd.dcx";
        }
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            path = $@"msg\{langFolder}\{msgBndType}{release}.msgbnd.dcx";
        }

        ad.AssetPath = $@"{path}";
        return ad;
    }
}
