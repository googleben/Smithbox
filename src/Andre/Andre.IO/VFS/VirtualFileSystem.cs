using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Andre.IO.VFS;

/// <summary>
/// Virtual file system used to abstract file system operations on a variety of sources, such as raw filesystem, a zip
/// file, or binders
/// </summary>
public abstract class VirtualFileSystem
{
    /// <summary>
    /// Is the file system readonly
    /// </summary>
    public abstract bool IsReadOnly { get; }
    
    /// <summary>
    /// The directory representing the root of this filesystem
    /// </summary>
    public abstract VirtualDirectory FsRoot { get; }

    /// <summary>
    /// Returns true if a given file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool FileExists(string path) => FileExists(new VFSPath(path));
    
    /// <summary>
    /// Returns true if a given file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public abstract bool FileExists(VFSPath path);

    /// <summary>
    /// Gets a file from a given path, if that file can be found
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public bool TryGetFile(string path, [MaybeNullWhen(false)] out VirtualFile file) 
        => TryGetFile(new VFSPath(path), out file);

    public Memory<byte>? ReadFile(string path)
        => GetFile(path)?.GetData();
    
    public VirtualFile? GetFile(string path)
    {
        TryGetFile(path, out var f);
        return f;
    }
    
    /// <summary>
    /// Gets a file from a given path, if that file can be found
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public abstract bool TryGetFile(VFSPath path, [MaybeNullWhen(false)] out VirtualFile file);

    /// <summary>
    /// Returns true if a given directory exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public bool DirectoryExists(string path) => DirectoryExists(new VFSPath(path));
    
    /// <summary>
    /// Returns true if a given directory exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public abstract bool DirectoryExists(VFSPath path);

    public VirtualDirectory? GetDirectory(string path)
    {
        return GetDirectory(new VFSPath(path));
    }

    public VirtualDirectory? GetDirectory(VFSPath path)
    {
        var currDir = FsRoot;
        foreach (var dir in path.directories)
        {
            if (currDir.TryGetDirectory(dir, out var d))
            {
                currDir = d;
            }
            else return null;
        }

        return currDir;
    }

    public bool TryGetDirectory(string path, [MaybeNullWhen(false)] out VirtualDirectory directory)
    {
        directory = GetDirectory(path);
        return directory != null;
    }

    /// <summary>
    /// Returns an enumerator over all the files in this filesystem
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<VirtualFile> EnumerateFiles();

    /// <summary>
    /// Equivalent to Directory.GetFileSystemEntries (but only returns files, not directories)
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public IEnumerable<string> GetFileNamesMatching(string directoryPath, [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
        => GetDirectory(directoryPath)?.GetFileNamesMatching(regex).Select(p => Path.Combine(directoryPath, p)) ?? Array.Empty<string>();

    public IEnumerable<string> GetFileNamesMatchingRecursive(string directoryPath,
        [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
    {
        var dir = GetDirectory(directoryPath);
        var ans = dir.GetFileNamesMatching(regex).Select(p => Path.Combine(directoryPath, p)).ToList();
        
        List<(string, VirtualDirectory)> pathStack = [(directoryPath, dir)];
        while (pathStack.Count != 0)
        {
            var (curr, currDir) = pathStack[^1];
            pathStack.RemoveAt(pathStack.Count-1);
            ans.AddRange(currDir.GetFileNamesMatching(regex).Select(p => Path.Combine(curr, p)));
            foreach (var (n, d) in currDir.EnumerateDirectories())
            {
                pathStack.Add((Path.Combine(curr, n), d));
            }
        }

        return ans;
    }

    #region WriteOperations

    /// <summary>
    /// Attempts to write all the supplied data to the specified file.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public void WriteFile(string path, byte[] data)
        => WriteFile(new VFSPath(path), data);

    /// <summary>
    /// Attempts to write all the supplied data to the specified file.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public void WriteFile(VFSPath path, byte[] data)
        => GetOrCreateFile(path).WriteData(data);

    public VirtualFile GetOrCreateFile(string path)
        => GetOrCreateFile(new VFSPath(path));
    
    public VirtualFile GetOrCreateFile(VFSPath path)
        => GetOrCreateDirectory(path).GetOrCreateFile(path.fileName!);
    
    /// <summary>
    /// Creates the specified directory, and any parent directories, if any of them do not exist.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    public void GetOrCreateDirectory(string path)
        => GetOrCreateDirectory(new VFSPath(path));

    /// <summary>
    /// Creates the specified directory, and any parent directories, if any of them do not exist.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    public VirtualDirectory GetOrCreateDirectory(VFSPath path)
        => path.directories.Aggregate(FsRoot, (current, dir) => current.GetOrCreateDirectory(dir));

    /// <summary>
    /// Attempt to delete a given file.
    /// Does nothing if the file cannot be found.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    public virtual void Delete(string path)
        => GetFile(path)?.Delete();

    public virtual void Copy(string from, string to)
        => WriteFile(to, GetFile(from).GetData().ToArray());

    public virtual void Move(string from, string to)
    {
        WriteFile(to, GetFile(from).GetData().ToArray());
        Delete(from);
    }

    #endregion
    
    public struct VFSPath(string[] directories, string? fileName)
    {
        public string[] directories = directories;
        public string? fileName = fileName;

        public VFSPath(params string[] pathComponents) : 
            this(
                pathComponents[^1].Contains('.') ? pathComponents[0..^1] : pathComponents, 
                pathComponents[^1].Contains('.') ? pathComponents[^1] : null) {}

        public VFSPath(string path) : this(path.Replace('\\', '/').Trim().Trim('/').Split('/')) {}

        public override string ToString()
        {
            return ("/" + string.Join('/', directories) + "/" + (fileName ?? "")).Replace("//", "/");
        }
    }

    internal static NotSupportedException ThrowWriteNotSupported() 
        => new("Attempted to write to a read-only file or filesystem.");
}

public abstract class VirtualDirectory
{
    
    public abstract bool IsReadOnly { get; }
    
    /// <summary>
    /// Returns true if a given file exists within this directory.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public abstract bool FileExists(string fileName);

    /// <summary>
    /// Gets the binary data of a file in this directory, if that file can be found
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileData"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public abstract bool TryGetFile(string fileName, [MaybeNullWhen(false)] out VirtualFile file);

    public VirtualFile? GetFile(string fileName)
    {
        return TryGetFile(fileName, out var d) ? d : null;
    }
    
    /// <summary>
    /// Returns true if a given directory exists within this directory.
    /// Only one level of directories may be queried, i.e. the path may not contain '/'.
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public abstract bool DirectoryExists(string directoryName);

    public abstract bool TryGetDirectory(string directoryName, [MaybeNullWhen(false)] out VirtualDirectory directory);

    public VirtualDirectory? GetDirectory(string directoryName)
    {
        return TryGetDirectory(directoryName, out var d) ? d : null;
    }

    /// <summary>
    /// Returns an enumerator over the directories contained by this directory.
    /// The enumerator returns tuples of the directory name followed by the directory object.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<(string, VirtualDirectory)> EnumerateDirectories();

    /// <summary>
    /// Returns an enumerator over the names of the directories contained by this directory.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<string> EnumerateDirectoryNames();

    /// <summary>
    /// Returns an enumerator over the names of the files contained by this directory.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<string> EnumerateFileNames();
    
    /// <summary>
    /// Returns an enumerator over the files contained by this directory.
    /// The enumerator returns tuples of the file name followed by the file object.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<(string, VirtualFile)> EnumerateFiles();

    public IEnumerable<string> GetFileNamesMatching([StringSyntax(StringSyntaxAttribute.Regex)] string regex) => 
        EnumerateFileNames().Where(s => Regex.IsMatch(s, regex));

    #region WritingOperations
    
    /// <summary>
    /// Attempts to get a directory with a specified name.
    /// If the directory does not already exist, an attempt is made to create that directory.
    /// May throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public abstract VirtualDirectory GetOrCreateDirectory(string directoryName);

    /// <summary>
    /// Gets a file with a given name, or if the file does not exist, attempts to create it.
    /// May throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public abstract VirtualFile GetOrCreateFile(string fileName);

    #endregion

}

public abstract class VirtualFile
{
    public abstract bool IsReadOnly { get; }
    /// <summary>
    /// Attempt to get the binary data of this file.
    /// </summary>
    /// <returns></returns>
    public abstract Memory<byte> GetData();

    /// <summary>
    /// Attempt to write binary data to a file.
    /// Guaranteed to throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    /// <param name="data"></param>
    public virtual void WriteData(byte[] data)
    {
        throw VirtualFileSystem.ThrowWriteNotSupported();
    }

    /// <summary>
    /// Attempt to delete this file.
    /// Guaranteed to throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    public virtual void Delete()
    {
        throw VirtualFileSystem.ThrowWriteNotSupported();
    }
}