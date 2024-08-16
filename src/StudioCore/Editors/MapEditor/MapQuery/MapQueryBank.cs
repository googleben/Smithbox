using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.MapQuery;

public class MapQueryBank
{
    private MapQueryEngine Engine;

    public bool MapBankInitialized = false;

    private List<ResourceDescriptor> MapResources = new List<ResourceDescriptor>();

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public MapQueryBank(MapQueryEngine engine) 
    {
        Engine = engine;
    }

    public void OnProjectChanged()
    {
        MapBankInitialized = false;
        MapList = new Dictionary<string, IMsb>();
    }

    public Dictionary<string, IMsb> GetMaps()
    {
        return MapList;
    }

    public async void SetupData()
    {
        ReadMapResources();

        // Load the maps async so the main thread isn't blocked
        Task<bool> loadMapsTask = ReadMaps();

        bool result = await loadMapsTask;
        MapBankInitialized = result;
    }

    public void ReadMapResources()
    {
        MapResources = new List<ResourceDescriptor>();

        var fs = Engine._targetProjectFiles ? Smithbox.ProjectFS : Smithbox.VanillaFS;
        string mapDir = "/map/mapstudio";

        foreach (var entry in fs.GetFileNamesWithExtensions(mapDir, ".msb.dcx"))
        {
            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
            ResourceDescriptor ad = MapLocator.GetMapMSB(name);
            if (ad.AssetPath != null)
            {
                MapResources.Add(ad);
            }
        }
    }

    public async Task<bool> ReadMaps()
    {
        await Task.Delay(1000);

        foreach (var resource in MapResources)
        {
            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(resource.AssetPath));
            IMsb msb = null;

            if (Smithbox.ProjectType == ProjectType.DES)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSBD>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSB1>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.DS2 || Smithbox.ProjectType == ProjectType.DS2S)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSB2>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSB3>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.BB)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSBB>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.SDT)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSBS>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.ER)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSBE>(resource.AssetPath);
            }
            if (Smithbox.ProjectType == ProjectType.AC6)
            {
                msb = Smithbox.FS.ReadSoulsFile<MSB_AC6>(resource.AssetPath);
            }

            if (msb != null && !MapList.ContainsKey(name))
            {
                MapList.Add(name, msb);
            }
        }

        return true;
    }

}
