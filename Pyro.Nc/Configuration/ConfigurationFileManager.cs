using System.IO;
using Pyro.Nc.Parsing;

namespace Pyro.Nc.Configuration
{
    public static class ConfigurationFileManager
    {
        static ConfigurationFileManager()
        {
            var files = Directory.GetFiles("./Configuration");
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var fileName = fileInfo.Name;
                var path = $"{CommandHelper._storage.StorageDirectory.FullName}\\Configuration\\{fileName}";
                var remoteFileInfo = new FileInfo(path);
                if (!remoteFileInfo.Exists)
                {
                    fileInfo.CopyTo(path);
                }
            }
        }
    }
}