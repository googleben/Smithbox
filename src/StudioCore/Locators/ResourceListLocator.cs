using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;

public static class ResourceListLocator
{
    // Used to get the map model list from within the mapbhd/bdt
    public static List<ResourceDescriptor> GetMapModelsFromBXF(string mapid)
    {
        List<ResourceDescriptor> ret = new();

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            var path = $@"model/map/{mapid}.mapbdt";

            if (Smithbox.FS.FileExists(path))
            {
                var bdtPath = path;
                var bhdPath = path.Replace("bdt", "bhd");

                var bxf = BXF4.Read(Smithbox.FS.ReadFile(bhdPath).Value, Smithbox.FS.ReadFile(bdtPath).Value);

                if (bxf != null)
                {
                    foreach (var file in bxf.Files)
                    {
                        if (file.Name.Contains(".flv"))
                        {
                            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.Name));

                            ResourceDescriptor ad = new();
                            ad.AssetName = name;
                            ad.AssetArchiveVirtualPath = $@"map/{name}/model/";

                            ret.Add(ad);
                        }
                    }
                }
            }
        }

        return ret;
    }

    public static List<ResourceDescriptor> GetMapModels(string mapid)
    {
        List<ResourceDescriptor> ret = new();
        if (Smithbox.ProjectType == ProjectType.DS3 || Smithbox.ProjectType == ProjectType.SDT)
        {
            foreach (var f in Smithbox.FS.GetFileNamesWithExtensions($"map/{mapid}", ".mapbnd.dcx"))
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            var mapPath = $@"\map\{mapid[..3]}\{mapid}";

            foreach (var f in Smithbox.FS.GetFileNamesWithExtensions(mapPath, ".mapbnd.dcx"))
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else if (Smithbox.ProjectType == ProjectType.AC6)
        {
            var mapPath = Smithbox.GameRoot + $@"\map\{mapid[..3]}\{mapid}";

            foreach (var f in Smithbox.FS.GetFileNamesWithExtensions(mapPath, ".mapbnd.dcx"))
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else
        {
            var ext = Smithbox.ProjectType == ProjectType.DS1 ? @".flver" : @".flver.dcx";
            foreach (var f in Smithbox.FS.GetFileNamesWithExtensions($@"\map\{mapid}\", ext))
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                // ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }

        ret.Sort();

        return ret;
    }

    public static List<string> GetChrModels()
    {
        try
        {
            HashSet<string> chrs = new();
            List<string> ret = new();

            var modelDir = @"\chr";
            var modelExt = @".chrbnd.dcx";

            if (Smithbox.ProjectType == ProjectType.DS1)
                modelExt = ".chrbnd";
            else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                modelDir = @"\model\chr";
                modelExt = ".bnd";
            }

            if (Smithbox.ProjectType == ProjectType.DES)
            {
                foreach (var f in Smithbox.FS.GetDirectory(modelDir).EnumerateDirectoryNames())
                {
                    var name = Path.GetFileNameWithoutExtension(f + ".dummy");
                    if (name.StartsWith("c"))
                        ret.Add(name);
                }

                return ret;
            }

            foreach (var f in Smithbox.FS.GetFileNamesWithExtensions(modelDir, modelExt))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                chrs.Add(name);
            }

            ret.Sort();

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static List<string> GetObjModels(bool useProject = false)
    {
        HashSet<string> objs = new();
        List<string> ret = new();

        var modelDir = @"\obj";
        var modelExt = @".objbnd.dcx";

        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            modelExt = ".objbnd";
        }
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            modelDir = @"\model\obj";
            modelExt = ".bnd";
        }
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            // AEGs are objs in my heart :(
            modelDir = @"\asset\aeg";
            modelExt = ".geombnd.dcx";
        }
        else if (Smithbox.ProjectType == ProjectType.AC6)
        {
            // AEGs are objs in my heart :(
            modelDir = @"\asset\environment\geometry";
            modelExt = ".geombnd.dcx";
        }

        var fs = Smithbox.FS;

        if(!fs.DirectoryExists(modelDir))
        {
            return ret;
        }

        foreach (var f in Smithbox.FS.GetFileNamesWithExtensions(modelDir, modelExt).ToList())
        {
            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
            ret.Add(name);
            objs.Add(name);
        }

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            foreach (var folder in fs.GetDirectory(modelDir).EnumerateDirectoryNames())
            {
                foreach (var f in fs.GetFileNamesWithExtensions($"{modelDir}/{folder[^6..]}", modelExt))
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    if (!objs.Contains(name))
                    {
                        ret.Add(name);
                        objs.Add(name);
                    }
                }
            }
        }

        ret.Sort();

        return ret;
    }

    public static List<string> GetPartsModels()
    {
        try
        {
            HashSet<string> parts = new();
            List<string> ret = new();

            var modelDir = @"\parts";
            var modelExt = @".partsbnd.dcx";

            if (Smithbox.ProjectType == ProjectType.DS1)
            {
                modelExt = ".partsbnd";
            }
            else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                modelDir = @"\model\parts";
                modelExt = ".bnd";

                foreach (var f in Smithbox.FS.GetFileNamesMatchingRecursive(modelDir, ".*"))
                {
                    if (!f.EndsWith("common.commonbnd.dcx") && !f.EndsWith("common_cloth.commonbnd.dcx") &&
                        !f.EndsWith("facepreset.bnd"))
                        ret.Add(Path.GetFileNameWithoutExtension(f));
                }

                return ret;
            }

            foreach (var f in Smithbox.FS.GetFileNamesWithExtensions(modelDir, modelExt))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                parts.Add(name);
            }

            ret.Sort();

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }
}
