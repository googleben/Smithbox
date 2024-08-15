using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialResourceBank
{
    private Dictionary<string, MaterialInfo> _mtds = new();
    private Dictionary<string, MaterialInfo> _matbins = new();

    public IReadOnlyDictionary<string, MaterialInfo> Mtds => _mtds;

    public IReadOnlyDictionary<string, MaterialInfo> Matbins => _matbins;


    public void LoadBank()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        TaskManager.Run(new TaskManager.LiveTask("Resource - Load Materials", TaskManager.RequeueType.WaitThenRequeue, false,
        () =>
        {
            LoadMatBins();
            LoadMtds();
        }));
    }

    public void LoadMatBins()
    {
        _matbins = new Dictionary<string, MaterialInfo>();
        string dir = "material/";
        if (Smithbox.ProjectFS.TryGetDirectory(dir, out var vd))
        {
            foreach (var file in vd.EnumerateFileNames())
            {
                LoadMatbinFile(dir+file, Smithbox.ProjectFS);
            }
        }

        if (Smithbox.VanillaFS.TryGetDirectory(dir, out vd))
        {
            foreach (var file in vd.EnumerateFileNames())
            {
                LoadMatbinFile(dir+file, Smithbox.VanillaFS);
            }
        }
    }

    public void LoadMatbinFile(string file, VirtualFileSystem fs)
    {
        IBinder binder = null;

        if (file.Contains(".matbinbnd.dcx"))
        {
            binder = BND4.Read(fs.GetFile(file).GetData());
            using (binder)
            {
                foreach (BinderFile f in binder.Files)
                {
                    var path = f.Name;
                    var matname = Path.GetFileNameWithoutExtension(f.Name);

                    MaterialInfo info = new MaterialInfo(matname, path, MATBIN.Read(f.Bytes), null);

                    if (!_matbins.ContainsKey(matname))
                        _matbins.Add(matname, info);
                }
            }
        }
    }

    public void LoadMtds()
    {
        _mtds = new Dictionary<string, MaterialInfo>();
        string dir = "mtd/";
        if (Smithbox.ProjectFS.TryGetDirectory(dir, out var vd))
        {
            foreach (var file in vd.EnumerateFileNames())
            {
                LoadMtdFile(dir+file, Smithbox.ProjectFS);
            }
        }
        
        if (Smithbox.VanillaFS.TryGetDirectory(dir, out vd))
        {
            foreach (var file in vd.EnumerateFileNames())
            {
                LoadMtdFile(dir+file, Smithbox.VanillaFS);
            }
        }
    }

    public void LoadMtdFile(string file, VirtualFileSystem fs)
    {
        IBinder binder = null;

        if (file.Contains(".mtd.dcx"))
        {
            binder = BND4.Read(fs.GetFile(file).GetData());
            using (binder)
            {
                foreach (BinderFile f in binder.Files)
                {
                    var path = f.Name;
                    var matname = Path.GetFileNameWithoutExtension(f.Name);

                    MaterialInfo info = new MaterialInfo(matname, path, null, MTD.Read(f.Bytes));

                    if (!_mtds.ContainsKey(matname))
                        _mtds.Add(matname, info);
                }
            }
        }
    }

    public struct MaterialInfo
    {
        public string Name;
        public string Path;
        public MATBIN Matbin;
        public MTD Mtd;

        public MaterialInfo(string name, string path, MATBIN matbin, MTD mtd)
        {
            Name = name;
            Path = path;
            Matbin = matbin;
            Mtd = mtd;
        }
    }
}
