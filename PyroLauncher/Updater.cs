using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using Pyro.Injector;
using Pyro.IO;
using Pyro.Math;

namespace PyroLauncher.Api
{
    public static class Updater
    {
        public static void CreateDllUpdate()
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("Pyro Launcher");
            var apps = roaming.ReadFileAs<AppConfiguration[]>("Apps.json");
            var pync = apps.FirstOrDefault();
            var path = pync.FullPath;
            var dirName = new FileInfo(path).Directory.FullName;
            var fullPath = $"{dirName}\\PyNc_Data\\Managed";
            UpdateInfo info = new UpdateInfo("update.json", 0, new SoftwareInfo.FolderInfo[]
            {
                new SoftwareInfo.FolderInfo(fullPath)
            });
            info.CreateAsDev();
        }

        public static void UnpackDllUpdate(string pathToFile)
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("Pyro Launcher");
            var apps = roaming.ReadFileAs<AppConfiguration[]>("Apps.json");
            var pync = apps.FirstOrDefault();
            var path = pync.FullPath;
            var dirName = new FileInfo(path).Directory.FullName;
            var fullPath = $"{dirName}\\PyNc_Data\\Managed";
            UpdateInfo info = JsonSerializer.Deserialize<UpdateInfo>(File.ReadAllText(pathToFile));
            info.Apply("PyNc", fullPath);
        }
        
        public static async Task UnpackDllUpdate(Stream fileStream, TextBlock block)
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("Pyro Launcher");
            var apps = roaming.ReadFileAs<AppConfiguration[]>("Apps.json");
            var pync = apps.FirstOrDefault();
            var path = pync.FullPath;
            var dirName = new FileInfo(path).Directory.FullName;
            var fullPath = $"{dirName}\\PyNc_Data\\Managed";
            UpdateInfo info = JsonSerializer.Deserialize<UpdateInfo>(fileStream);
            block.Text = $"Read update info: {(info.Length / 1_000_000f).Round().ToString(CultureInfo.InvariantCulture)}MB!";
            await Task.Run(() =>
            {
                info.Apply("PyNc", fullPath);
            });
            block.Text = $"Wrote all to disk!";
        }
    }
}