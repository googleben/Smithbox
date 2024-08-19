using Andre.Core;
using Andre.IO.VFS;
using DotNext.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andre.IO
{
    /// <summary>
    /// Describes the location and name of an asset which may or may not be contained within an archive
    /// </summary>
    public readonly struct AssetLocation(string? containingArchivePath, string? assetPath, string? assetName)
        : IComparable<AssetLocation>
    {
        /// <summary>
        /// The path to the archive that contains this asset, if this asset is contained by an archive.
        /// </summary>
        public readonly string? ContainingArchivePath = containingArchivePath;
        
        /// <summary>
        /// The path to this asset.
        /// </summary>
        public readonly string? AssetPath = assetPath;

        /// <summary>
        /// The name of this asset.
        /// </summary>
        public readonly string? AssetName = assetName;
        
        public bool IsValid => AssetPath != null;

        public AssetLocation(string? assetPath, string? assetName) : this(null, assetPath, assetName) {}

        public override int GetHashCode()
        {
            return AssetPath?.GetHashCode() ?? base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not AssetLocation a)
                return base.Equals(obj);

            if (AssetPath == null)
                return a.AssetPath == null;
            return AssetPath.Equals(a.AssetPath);
        }

        public int CompareTo(AssetLocation a)
            => string.Compare(AssetName, a.AssetName, StringComparison.CurrentCulture);

        public static bool operator ==(AssetLocation left, AssetLocation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetLocation left, AssetLocation right)
        {
            return !(left == right);
        }
    }
    public static partial class Locator
    {
        /// <summary>
        /// Given a map's numerical ID, finds its path.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the path ought to be even if the file doesn't exist</param>
        /// <returns></returns>
        public static AssetLocation? FindMsbForId(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            if (mapid.Length != 12)
                return null;
            string? path = game switch
            {
                Game.DS2S or Game.DS2 => $"map/{mapid}/{mapid}.msb",
                Game.BB when mapid.StartsWith("m29") => $@"map/MapStudio/{mapid[..9]}_00/{mapid}.msb",
                Game.DS1 or Game.DS1R or Game.DES or Game.BB or Game.DS3 or Game.ER or Game.AC6
                    => $@"map/MapStudio/{mapid}.msb",
                _ => null
            };
            if (path == null) return null;

            bool exists = fs.FileExists(path);
            bool dcxExists = fs.FileExists(path + ".dcx");
            bool preferNoDcx = game is Game.DS1 or Game.DS2 or Game.DS2S or Game.DS1R or Game.DES;
            if ((writemode && preferNoDcx) || (!dcxExists && exists))
                return new(path, mapid);
            if (writemode || dcxExists)
                return new(path+".dcx", mapid);
            return null;
        }

        /// <summary>
        /// Gets the paths of all BTLs for the given map ID.
        /// Returns an empty List if no BTLs are found.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the paths ought to be even if the files don't exist</param>
        /// <returns></returns>
        public static List<AssetLocation> GetMapBtls(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            List<AssetLocation> ans = [];
            if (mapid.Length != 12)
                return ans;
            if (game is Game.DS2S or Game.DS2)
            {
                //DS2 BTLs are located inside .gibdt files
                string gibhd = $@"model/map/g{mapid[1..]}.gibhd";
                if (fs.FileExists(gibhd) || writemode)
                {
                    ans.Add(new (gibhd,
                        $"{mapid}/light.btl.dcx",
                        $"g{mapid[1..]}"));
                }

                gibhd = $"model_lq/map/g{mapid[1..]}.gibhd";
                if (fs.FileExists(gibhd) || writemode)
                {
                    ans.Add(new (gibhd,
                        $"{mapid}/light.btl.dcx",
                        $"g{mapid[1..]}"));
                }
            }
            else if (game is Game.BB or Game.DS3 or Game.SDT or Game.ER or Game.AC6)
            {
                string folder;
                if (game is Game.ER or Game.AC6)
                    folder = $"map/{mapid[..3]}/{mapid}";
                else
                    folder = $"map/{mapid}";

                var names = fs.GetFileNamesWithExtensions(folder, ".btl", ".btl.dcx")
                    .Select(Path.GetFileName)
                    .Select(s => s?.Replace(".dcx", "").Replace(".btl", "") ?? "")
                    .Distinct();
                foreach (var name in names)
                {
                    string regularPath = $"{folder}/{name}.btl";
                    bool exists = fs.FileExists(regularPath);
                    string dcxPath = $"{regularPath}.dcx";
                    bool dcxExists = fs.FileExists(dcxPath);

                    string? path = null;
                    if (dcxExists || writemode)
                        path = dcxPath;
                    else if (exists)
                        path = regularPath;

                    if (path != null)
                    {
                        ans.Add(new(path, name));
                    }
                }
            }

            return ans;
        }

        /// <summary>
        /// Gets the location of the NVA corresponding to a map id.
        /// Returns null if no NVA is found.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the path ought to be even if the file doesn't exist</param>
        /// <returns></returns>
        public static AssetLocation? GetMapNva(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            if (mapid.Length != 12) return null;
            if (game is not (Game.BB or Game.DS3 or Game.SDT or Game.ER or Game.AC6))
                return null;

            string path = game switch
            {
                //BB chalice maps
                Game.BB when mapid.StartsWith("m29") => $"map/{mapid[..9]}_00/{mapid}.nva",
                Game.ER => $"map/{mapid[..3]}/{mapid}/{mapid}.nva",
                _ => $"map/{mapid}/{mapid}.nva"
            };

            string dcxPath = $"{path}.dcx";
            string ansPath;
            if (fs.FileExists(dcxPath) || writemode)
                ansPath = dcxPath;
            else if (fs.FileExists(path))
                ansPath = path;
            else
                return null;

            return new(ansPath, mapid);
        }

        /// <summary>
        /// Gets the IDs for all maps that exist in a given filesystem.
        /// Note that in some cases, these IDs may need to be adjusted to find their assets.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static List<string> GetAllMapIDs(Game game, VirtualFileSystem fs)
        {
            HashSet<string> mapSet = [];
            if (game is Game.DS2S or Game.DS2)
            {
                mapSet.AddAll(fs.GetFileSystemEntriesMatching("map", "m.*")
                    .Select(Path.GetFileNameWithoutExtension)
                    .SkipNulls());
            }
            else
            {
                mapSet.AddAll(fs.GetFileNamesWithExtensions("map/MapStudio", ".msb")
                    .Select(Path.GetFileNameWithoutExtension)
                    .SkipNulls());
                mapSet.AddAll(fs.GetFileNamesWithExtensions("map/MapStudio", ".msb.dbx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                    .SkipNulls());
            }
            var mapRegex = MapIdRegex();
            return mapSet.Where((s) => mapRegex.IsMatch(s)).Order().ToList();
        }

        [GeneratedRegex(@"^m\d{2}_\d{2}_\d{2}_\d{2}$")]
        private static partial Regex MapIdRegex();
        
        /// <summary>
        /// Given a map ID, this function gets the map ID its assets are stored under.
        /// This is necessary because in some cases, maps store their assets under a different map ID.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetAssetMapId(string mapid, Game game)
        {
            return game switch
            {
                Game.ER or Game.AC6 or Game.DES => mapid,
                //DSR m99 maps contain their own assets
                Game.DS1R when mapid.StartsWith("m99") => mapid,
                //BB chalice dungeons
                Game.BB when mapid.StartsWith("m29") => "m29_00_00_00",
                _ => mapid[..6] + "_00_00"
            };
        }

        public static List<AssetLocation> GetMapBtabs(string mapid, Game game, VirtualFileSystem fs)
            => fs.GetFileNamesWithExtensions($"map/{mapid}", ".btab.dcx")
                .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                .SkipNulls()
                .Select(s => new AssetLocation($"map/{mapid}/{s}.btab.dcx", s))
                .ToList();
    }
    
}