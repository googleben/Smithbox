using DotNext.IO.MemoryMappedFiles;
using SoulsFormats;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace Andre.Formats
{
    public class BinderArchive
    {
        private BHD5 bhd;
        public FileStream Bdt { get; }

        public BinderArchive(BHD5 bhd, FileStream bdt)
        {
            this.bhd = bhd;
            this.Bdt = bdt;
        }

        public BinderArchive(string bhdPath, string bdtPath, BHD5.Game game)
        {
            using var file = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            try
            {
                bhd = BHD5.Read(accessor.Memory, game);
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Invalid BHD file. Did you make sure the BHD was decrypted?");
                throw;
            }
            Bdt = File.OpenRead(bdtPath);
        }

        public BHD5.FileHeader? TryGetFileFromHash(ulong hash)
        {
            return bhd.Buckets.SelectMany(b => b.Where(f => f.FileNameHash == hash)).FirstOrDefault();
        }

        public byte[] ReadFile(BHD5.FileHeader file) => file.ReadFile(Bdt);

        public byte[]? TryReadFileFromHash(ulong hash) => TryGetFileFromHash(hash)?.ReadFile(Bdt);

        public List<BHD5.Bucket> Buckets => bhd.Buckets;
        
        public IEnumerable<BHD5.FileHeader> EnumerateFiles() =>
            Buckets.Select(b => b.AsEnumerable()).Aggregate(Enumerable.Concat);

    }
}