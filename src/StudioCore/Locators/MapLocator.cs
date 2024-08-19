using Andre.IO;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Locators;
public static class MapLocator
{
    public static List<string> FullMapList;

    /// <summary>
    /// Get a MSB asset.
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static ResourceDescriptor GetMapMSB(string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
            return ad;

        var l =  Locator.FindMsbForId(mapid, Smithbox.ProjectType.AsAndreGame().Value, Smithbox.FS, writemode);
        ad.AssetPath = l.Value.AssetPath;
        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    /// Get a BTL asset.
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static List<ResourceDescriptor> GetMapBTLs(string mapid, bool writemode = false)
    {
        List<ResourceDescriptor> adList = new();
        if (mapid.Length != 12)
            return adList;

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            // DS2 BTL is located inside map's .gibdt file
            ResourceDescriptor ad = new();
            var path = $@"model\map\g{mapid[1..]}.gibhd";

            if (Smithbox.FS.FileExists(path) || (writemode && !Smithbox.ProjectFS.IsReadOnly))
            {
                ad.AssetPath = path;
            }

            if (ad.AssetPath != null)
            {
                ad.AssetName = $@"g{mapid[1..]}";
                ad.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad);
            }

            ResourceDescriptor ad2 = new();
            path = $@"model_lq\map\g{mapid[1..]}.gibhd";

            if (Smithbox.FS.FileExists(path) || (writemode && !Smithbox.ProjectFS.IsReadOnly))
            {
                ad2.AssetPath = path;
            }

            if (ad2.AssetPath != null)
            {
                ad2.AssetName = $@"g{mapid[1..]}_lq";
                ad2.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad2);
            }
        }
        else if (Smithbox.ProjectType is ProjectType.BB or ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            string path;
            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                path = $@"map\{mapid[..3]}\{mapid}";
            }
            else
            {
                path = $@"map\{mapid}";
            }

            List<string> files = new();
            
            files.AddRange(Smithbox.FS.GetFileNamesWithExtensions(path, ".btl", ".btl.dcx"));

            foreach (var file in files)
            {
                ResourceDescriptor ad = new();
                var fileName = Path.GetFileName(file);
                
                if (Smithbox.FS.FileExists($@"{path}\{fileName}") || (writemode && !Smithbox.ProjectFS.IsReadOnly))
                {
                    ad.AssetPath = $@"{path}\{fileName}";
                }

                if (ad.AssetPath != null)
                {
                    ad.AssetName = fileName;
                    adList.Add(ad);
                }
            }
        }

        return adList;
    }

    /// <summary>
    /// Get a NVA asset
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static ResourceDescriptor GetMapNVA(string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;

        if (mapid.Length != 12)
            return ad;

        if (Smithbox.ProjectType == ProjectType.BB && mapid.StartsWith("m29"))
        {
            var path = $@"\map\{mapid[..9]}_00\{mapid}";

            if (Smithbox.FS.FileExists($@"{path}.nva.dcx") || writemode && Smithbox.ProjectRoot != null && Smithbox.ProjectType != ProjectType.DS1)
            {
                ad.AssetPath = $"{path}.nva.dcx";
            }
        }
        else
        {
            var path = $@"\map\{mapid}\{mapid}";

            if (Smithbox.FS.FileExists($"{path}.nva.dcx") || writemode && Smithbox.ProjectRoot != null && Smithbox.ProjectType != ProjectType.DS1)
            {
                ad.AssetPath = $@"{path}.nva.dcx";
            }
            else if (Smithbox.FS.FileExists($@"{path}.nva") || writemode && Smithbox.ProjectRoot != null)
            {
                ad.AssetPath = $@"{path}.nva";
            }
        }

        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    /// Gets the full list of maps in the game (excluding chalice dungeons). 
    /// Basically if there's an msb for it, it will be in this list.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetFullMapList()
    {
        if (Smithbox.GameRoot == null)
            return null;

        if (FullMapList != null)
            return FullMapList;

        HashSet<string> mapSet = new();

        // DS2 has its own structure for msbs, where they are all inside individual folders
        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            Smithbox.FS.GetFileSystemEntriesMatching(@"\map", @"m.*")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList().ForEach(s => mapSet.Add(s));
        }
        else
        {
            Smithbox.FS.GetFileNamesWithExtensions("map/MapStudio", ".msb")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList().ForEach(f => mapSet.Add(f));
            Smithbox.FS.GetFileNamesWithExtensions("map/MapStudio", ".msb.dcx")
                .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                .ToList().ForEach(f => mapSet.Add(f));
        }

        Regex mapRegex = new(@"^m\d{2}_\d{2}_\d{2}_\d{2}$");
        var mapList = mapSet.Where(x => mapRegex.IsMatch(x)).ToList();

        mapList.Sort();

        FullMapList = mapList;
        return FullMapList;
    }

    /// <summary>
    /// Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public static string GetAssetMapID(string mapid)
    {
        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            return mapid;

        if (Smithbox.ProjectType is ProjectType.DS1R)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DES)
        {
            return mapid;
        }
        else if (Smithbox.ProjectType is ProjectType.BB)
        {
            if (mapid.StartsWith("m29"))
            {
                // Special case for chalice dungeon assets
                return "m29_00_00_00";
            }
        }

        // Default
        return mapid[..6] + "_00_00";
    }

    /// <summary>
    /// Get a BTAB asset.
    /// </summary>
    public static List<ResourceDescriptor> GetMapBTABs(string mapid)
    {
        List<ResourceDescriptor> resourceDescriptors = new();

        // Get the names
        var names = Smithbox.FS.GetFileNamesWithExtensions($"map/{mapid}", ".btab.dcx")
            .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
            .ToList();

        var paths = new List<string>();

        // Get the resource descriptors
        foreach(var name in names)
        {
            var path = LocatorUtils.GetAssetPath($"\\map\\{mapid}\\{name}.btab.dcx");
            paths.Add(path);
        }

        foreach(var path in paths)
        {
            ResourceDescriptor resource = new ResourceDescriptor();

            resource.AssetPath = path;

            resourceDescriptors.Add(resource);
        }

        return resourceDescriptors;
    }

    /// <summary>
    /// Get a HKX Collision asset.
    /// </summary>
    public static List<ResourceDescriptor> GetMapCollisions(string mapid)
    {
        List<ResourceDescriptor> resourceDescriptors = new();

        // Get the names
        //TODO: check whether the .Replace call is redundant
        var names = Smithbox.FS
            .GetFileNamesWithExtensions($"/map/{mapid[..3]}\\{mapid}", ".hkxbhd")
            .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
            .Select(s => s.Replace(".hkxbhd", ""))
            .ToList();

        var paths = new List<string>();

        // Get the resource descriptors
        foreach (var name in names)
        {
            var path = LocatorUtils.GetAssetPath($"\\map\\{mapid}\\{name}.hkxbhd");
            paths.Add(path);
        }

        foreach (var path in paths)
        {
            ResourceDescriptor resource = new ResourceDescriptor();

            resource.AssetPath = path;

            resourceDescriptors.Add(resource);
        }

        return resourceDescriptors;
    }
}
