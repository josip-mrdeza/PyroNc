using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Pyro.IO.Versioning;

public class FileVersion
{
    public string Name { get; private set; }
    public string Version => $"v{Major.ToString()}.{Minor.ToString()}.{Patch.ToString()}";
    public IntLimiter Major { get; set; }
    public IntLimiter Minor { get; set; }
    public IntLimiter Patch { get; set; }
    public string BaseFolder { get; private set; }
    public string EndPath => Path.Combine(BaseFolder, Name);
    public long Length => _info.Value.Length;
    private readonly Lazy<FileInfo> _info;
    public FileVersion(string name, int major, int minor, int patch, string baseFolder)
    {
        Name = name;
        Major = major;
        Minor = minor;
        Patch = patch;
        BaseFolder = baseFolder;
        _info = new Lazy<FileInfo>(() => new FileInfo(EndPath));
    }

    public async Task<byte[]> ReadFileAsync()
    {
        var fs = File.OpenRead(EndPath);
        var arr = new byte[Length];
        await fs.ReadAsync(arr, 0, arr.Length);
        fs.Flush();
        fs.Dispose();
        return arr;
    }

    public static FileVersion New(string name, string baseFolder)
    {
        var fv = new FileVersion(name, 0, 0, 1, baseFolder);
        return fv;
    }

    public bool IsEqualTo(FileVersion other)
    {
        if (Name != other.Name)
        {
            return false;
        }
        if (Version != other.Version)
        {
            return false;
        }
        return true;
    }
}