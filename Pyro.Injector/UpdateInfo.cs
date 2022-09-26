using System;
using System.Linq;
using System.Text.Json.Serialization;
using Pyro.IO;

namespace Pyro.Injector
{
    public class UpdateInfo : SoftwareInfo
    {
        public string Name { get; set; }
        public long Length { get; set; }
        
        [JsonConstructor]
        public UpdateInfo(string name, long length, FolderInfo[] folders, string path, bool isUnity)
        {
            Name = name;
            Folders = folders;
            Length = length;
            Path = path;
            IsUnity = isUnity;
        }
        
        public UpdateInfo(string name, long length, FolderInfo[] directories)
        {
            Name = name;
            Folders = directories;
            Length = length;
        }

        private void UpdateFiles(DocumentInfo[] updateFiles, DocumentInfo[] softwareFiles)
        {
            var defaultColor = Console.ForegroundColor;
            for (var i = 0; i < updateFiles.Length; i++)
            {
                var updateDocument = updateFiles[i];
                if (softwareFiles == null || softwareFiles.Length < i + 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"-->Updating document data: {updateDocument.Path}");
                    updateDocument.Update();
                    Console.ForegroundColor = defaultColor;
                    Console.WriteLine("<------------");
                    continue;
                }

                var softwareDocument = softwareFiles[i];
                var bad = !softwareDocument.IsSameAs(updateDocument);
                if (bad)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"-->Document failed test: {updateDocument.Path}");
                    Console.WriteLine($"Updating document data...");
                    Console.ForegroundColor = defaultColor;
                    updateDocument.Update();
                    Console.WriteLine("<------------");
                }
            }
        }
        
        public void Apply(string app)
        {
            var softwareInfo = GetFromCache(app);
            if (softwareInfo.Folders.Length == 0 || Folders.Length == 0)
            {
                return;
            }
            UpdateFiles(Folders[0].Documents, softwareInfo.Folders[0].Documents);
            for (int i = 0; i < Folders.Length; i++)
            {
                var updateDirectory = Folders[i];
                if (softwareInfo.Folders.Length < i + 1)
                {
                    UpdateFiles(updateDirectory.Documents, null);
                    continue;
                }
                UpdateFiles(updateDirectory.Documents, softwareInfo.Folders[i].Documents);
            }
        }

        public void CreateAsDev()
        {
            var defaultColor = Console.ForegroundColor;
            void RefreshData(FolderInfo[] infos)
            {
                foreach (var folderInfo in infos)
                {
                    Console.WriteLine("\n------------>");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Entered new directory: {folderInfo.Path}");
                    Console.ForegroundColor = defaultColor;
                    Console.WriteLine("<------------\n");
                    foreach (var document in folderInfo.Documents)
                    {
                        Console.WriteLine($"-->Getting document data: {document.Path}");
                        document.RefreshDataAlt();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Read document data: {document.Data.Length} bytes");
                        Console.ForegroundColor = defaultColor;
                        Console.WriteLine("<------------");
                    }
                    RefreshData(folderInfo.Directories);
                }
            }
            
            RefreshData(Folders);

            var roaming = LocalRoaming.OpenOrCreate("PyroNc");
            roaming.AddFile(Name, this);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\n\nTotal data written: {(Folders.Sum(x => x.Size) / 1_000_000f):F} MB!\n");
            Console.ForegroundColor = defaultColor;
        }
    }
}