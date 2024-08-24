﻿using SoulsFormats;

namespace Andre.IO
{
    public class MapStaticLocationInfo
    {
        public string id;
        public AssetLocation? Msb;
        public List<AssetLocation> Btls;
        public AssetLocation? Nva;
        public List<AssetLocation> Btabs;
    }

    public class MapLocationInfo
    {
        public IMsb msb;
    }
}