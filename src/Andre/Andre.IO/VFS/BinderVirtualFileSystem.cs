﻿using Andre.Core;
using Andre.Core.Util;
using Andre.Formats;
using SoulsFormats;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Andre.IO.VFS
{
    public class BinderVirtualFileSystem : VirtualFileSystem
    {
        private BinderArchive[] binders;
        private BhdDictionary dictionary;
        private List<BinderVirtualFile> fileList;
        private Dictionary<string, BinderVirtualFile> files;
        private BinderVirtualDirectory root;
        
        public override bool IsReadOnly => true;
        public override VirtualDirectory FsRoot => root;

        public BinderVirtualFileSystem(BinderArchive[] binders, BhdDictionary dictionary)
        {
            this.binders = binders;
            this.dictionary = dictionary;
            files = new();
            int numFiles = binders.Sum(b => b.Buckets.Sum(bucket => bucket.Count));

            fileList = new(numFiles);
            root = new BinderVirtualDirectory("");
            
            //build file caches
            foreach (var b in binders)
            {
                foreach (var h in b.EnumerateFiles())
                {
                    BinderVirtualFile f;
                    if (this.dictionary.GetPath(h.FileNameHash, out string? p))
                    {
                        p = p.ToLower();
                        if (this.files.ContainsKey(p))
                        {
                            Console.WriteLine($"Duplicate file for name \"{p}\"!");
                            continue;
                        }
                        string[] sp = p.Trim('/').Split('/');
                        string fileName = sp[^1];
                        f = new(fileName, h, b.Bdt);
                        files.Add(p!, f);
                        var currDir = root;
                        foreach (string dirName in sp[..^1])
                        {
                            if (currDir.directories.TryGetValue(dirName, out var d))
                            {
                                currDir = d;
                            }
                            else
                            {
                                var tmp = new BinderVirtualDirectory(dirName);
                                currDir.directories.Add(dirName, tmp);
                                currDir = tmp;
                            }
                        }
                        currDir.files.Add(fileName, f);
                    }
                    else
                    {
                        f = new(null, h, b.Bdt);
                        Console.WriteLine($"Couldn't find name for file hash: {h.FileNameHash}");
                    }
                    fileList.Add(f);
                }
            }
        }
        public static BinderVirtualFileSystem FromDS1(string folder)
        {
            var binders = Directory.GetFiles(folder, "*.bdt", SearchOption.AllDirectories)
                .Select(bdt => new BinderArchive(bdt.Replace(".bdt", ".bhd"), bdt, Game.DS1))
                .ToArray();
            var dictionary = new BhdDictionary(File.ReadAllText(@"Resources\DarkSouls1Dictionary.txt"), BHD5.Game.DarkSouls1);
            return new(binders, dictionary);
        }

        public static BinderVirtualFileSystem FromDS2(string folder)
        {
            var binders = Directory.GetFiles(folder, "*.bdt", SearchOption.AllDirectories)
                .Select(bdt => new BinderArchive(bdt.Replace(".bdt", ".bhd"), bdt, Game.DS2S))
                .ToArray();
            var dictionary = new BhdDictionary(File.ReadAllText(@"Resources\DarkSouls2Dictionary.txt"), BHD5.Game.DarkSouls2);
            return new(binders, dictionary);
        }
        
        public static BinderVirtualFileSystem FromEldenRing(string folder)
        {
            var binders = Directory.GetFiles(folder, "*.bdt", SearchOption.AllDirectories)
                .Where(x => !x.Contains("bhd5"))
                .Select(bdt => new BinderArchive(bdt.Replace(".bdt", ".bhd"), bdt, Game.ER))
                .ToArray();
            var dictionary = new BhdDictionary(File.ReadAllText(@"Resources\EldenRingDictionary.txt"), BHD5.Game.EldenRing);
            return new(binders, dictionary);
        }

        public static BinderVirtualFileSystem FromGameFolder(string folder, Game game)
        {
            BhdDictionary dictionary = game switch
            {
                Game.DES => throw new NotImplementedException(),
                Game.DS1 => new(File.ReadAllText(@"Resources\DarkSoulsDictionary.txt"), BHD5.Game.DarkSouls1),
                Game.DS1R => new(File.ReadAllText(@"Resources\DarkSoulsDictionary.txt"), BHD5.Game.DarkSouls1),
                Game.DS2S => new(File.ReadAllText(@"Resources\DarkSouls2Dictionary.txt"), BHD5.Game.DarkSouls2),
                Game.DS3 => new(File.ReadAllText(@"Resources\DarkSouls3Dictionary.txt"), BHD5.Game.DarkSouls3),
                Game.BB => throw new NotImplementedException(),
                Game.SDT => new(File.ReadAllText(@"Resources\SekiroDictionary.txt"), BHD5.Game.DarkSouls3),
                Game.ER => new(File.ReadAllText(@"Resources\EldenRingDictionary.txt"), BHD5.Game.EldenRing),
                Game.AC6 => new(File.ReadAllText(@"Resources\ArmoredCore6Dictionary.txt"), BHD5.Game.EldenRing),
                Game.DS2 => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
            };
            var binders = BinderArchive.FindBHDs(folder, game)
                .Select(s => new BinderArchive(s, s.Replace(".bhd5", ".bdt").Replace(".bhd", ".bdt"), game))
                .ToArray();
            return new(binders, dictionary);
        }
        
        public override bool TryGetFile(VirtualFileSystem.VFSPath path, [MaybeNullWhen(false)] out VirtualFile file)
        {
            if (TryGetFileInner(path.ToString().ToLower(), out var f))
            {
                file = f;
                return true;
            }
            file = null;
            return false;
        }

        /// <summary>
        /// The same as TryGetFile, but may only be used with a canonicalized path.
        /// No guarantees are made for behavior when using a non-canonical path, or in the case of hash collisions.
        /// </summary>
        /// <param name="canonicalPath">The canonical path of the file to find</param>
        /// <param name="file">The requested file, if it can be found. Null otherwise</param>
        /// <returns>true if the file was found, false otherwise</returns>
        private bool TryGetFileInner(string canonicalPath, [MaybeNullWhen(false)] out BinderVirtualFile file)
        {
            if (files.TryGetValue(canonicalPath, out file))
            {
                return true;
            }
            //The file wasn't found in our cache, so maybe something is wrong with our dictionary.
            //As a fallback, we'll do the hash lookup manually.
            ulong hash = dictionary.ComputeHash(canonicalPath);
            var tmp = fileList.Where(f => f.FileHeader.FileNameHash == hash).ToArray();
            switch (tmp.Length)
            {
                case 0:
                    file = null;
                    return false;
                case > 1:
                    Console.WriteLine($"Warning: Found more than one file for path: \"{canonicalPath}\", hash: {hash}");
                    break;
            }
            Console.WriteLine($"Warning: file for path \"{canonicalPath}\" wasn't cached in the file lookup table correctly. Hash: {hash}");
            file = tmp[0];
            return true;
            
        }
        
        public override bool FileExists(VirtualFileSystem.VFSPath path) => TryGetFile(path, out var _);

        public override bool DirectoryExists(VirtualFileSystem.VFSPath path)
        {
            return ((VirtualFileSystem)this).GetDirectory(path) != null;
        }

        public override IEnumerable<VirtualFile> EnumerateFiles()
        {
            return fileList;
        }

        public class BinderVirtualFile(string? name, BHD5.FileHeader fileHeader, FileStream bdt) : VirtualFile
        {
            public string? Name { get; } = name;
            public override bool IsReadOnly => true;
            public BHD5.FileHeader FileHeader { get; } = fileHeader;
            public FileStream Bdt { get; } = bdt;
            
            public override Memory<byte> GetData() => FileHeader.ReadFileThreaded(Bdt);
        }

        public class BinderVirtualDirectory(string name) : VirtualDirectory
        {
            /// <summary>
            /// The name of this directory
            /// </summary>
            public string Name { get; } = name;
            public override bool IsReadOnly => true;
            /// <summary>
            /// All directories contained by this directory
            /// </summary>
            internal readonly Dictionary<string, BinderVirtualDirectory> directories = new();
            /// <summary>
            /// All files contained by this directory
            /// </summary>
            internal readonly Dictionary<string, BinderVirtualFile> files = new();

            public override bool FileExists(string fileName)
            {
                return files.ContainsKey(fileName.ToLower());
            }

            public override bool TryGetFile(string fileName, [MaybeNullWhen(false)] out VirtualFile file)
            {
                if (files.TryGetValue(fileName.ToLower(), out var f))
                {
                    file = f;
                    return true;
                }
                file = null;
                return false;
            }

            public override bool DirectoryExists(string directoryName)
            {
                return directories.ContainsKey(directoryName.ToLower());
            }

            public override bool TryGetDirectory(string directoryName, [MaybeNullWhen(false)] out VirtualDirectory directory)
            {
                if (directories.TryGetValue(directoryName.ToLower(), out var d))
                {
                    directory = d;
                    return true;
                }
                directory = null;
                return false;
            }

            public override IEnumerable<(string, VirtualDirectory)> EnumerateDirectories() 
                => directories.AsEnumerable().Select(p => (p.Key, p.Value as VirtualDirectory));

            public override IEnumerable<string> EnumerateDirectoryNames() => directories.Keys;

            public override IEnumerable<string> EnumerateFileNames() => files.Keys;

            public override IEnumerable<(string, VirtualFile)> EnumerateFiles()
                => files.AsEnumerable().Select(p => (p.Key, p.Value as VirtualFile));

            public override VirtualDirectory GetOrCreateDirectory(string directoryName)
            {
                if (TryGetDirectory(directoryName, out var dir))
                {
                    return dir;
                }
                throw ThrowWriteNotSupported();
            }

            public override VirtualFile GetOrCreateFile(string fileName)
            {
                if (TryGetFile(fileName, out var file))
                {
                    return file;
                }

                throw ThrowWriteNotSupported();
            }
        }
    }
}