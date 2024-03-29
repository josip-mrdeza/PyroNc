using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Pyro.IO;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

namespace Pyro.Injector
{
    public class UpdateInfo : SoftwareInfo
    {
        public string Name { get; set; }
        public long Length { get; set; }
        
        [JsonConstructor]
        public UpdateInfo(string name, long length, FolderInfo[] folders, string path)
        {
            Name = name;
            Folders = folders;
            Length = length;
            Path = path;
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
        
        private void UpdateFiles(DocumentInfo[] updateFiles, DocumentInfo[] softwareFiles, string path)
        {
            var defaultColor = Console.ForegroundColor;
            for (var i = 0; i < updateFiles.Length; i++)
            {
                var updateDocument = updateFiles[i];
                if (softwareFiles == null || softwareFiles.Length < i + 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"-->Updating document data: {updateDocument.Path}");
                    updateDocument.Update(path);
                    Console.ForegroundColor = defaultColor;
                    Console.WriteLine("<------------");
                    continue;
                }

                var softwareDocument = softwareFiles[i];
                var bad = !softwareDocument.IsSameAs(updateDocument, path);
                if (bad)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"-->Document failed test: {updateDocument.Path}");
                    Console.WriteLine($"Updating document data...");
                    Console.ForegroundColor = defaultColor;
                    updateDocument.Update(path);
                    Console.WriteLine("<------------");
                }
            }
        }
        
        public void Apply(string app, string path)
        {
            var softwareInfo = GetFromCache_Init(app, path);
            if (softwareInfo.Folders.Length == 0 || Folders.Length == 0)
            {
                return;
            }
            //UpdateFiles(Folders[0].Documents, softwareInfo.Folders[0].Documents);
            void Recurse(FolderInfo folderInfoUpdate, FolderInfo folderInfoSoftware)
            {
                UpdateFiles(folderInfoUpdate.Documents, folderInfoSoftware is null ? null : folderInfoSoftware.Documents, path);
                if (folderInfoUpdate.Directories.Length > 0)
                {
                    for (var i = 0; i < folderInfoUpdate.Directories.Length; i++)
                    {
                        var directory = folderInfoUpdate.Directories[i];
                        if (folderInfoSoftware == null)
                        {
                            Recurse(directory, null);

                            continue;
                        }
                        var sDir = folderInfoSoftware.Directories.FirstOrDefault(x => x.Path == directory.Path);
                        if (sDir != null)
                        {
                            Recurse(directory, sDir);

                            continue;
                        }
                        Recurse(directory, null);
                    }
                }
            }
            Recurse(Folders.FirstOrDefault(), softwareInfo.Folders.FirstOrDefault());
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
                    folderInfo.Documents = folderInfo.Documents.Where(x => !x.Path.EndsWith(".pdb")).ToArray();
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
            roaming.ModifyFile(Name, this);
            using var archive = ZipArchive.Create();
            var fileData = roaming.ReadFile(Name);
            var rarPath = Name.Replace(".json", ".zip");
            archive.AddEntry(Name, new MemoryStream(fileData), true, fileData.Length);
            archive.SaveTo(roaming.Site + rarPath, new WriterOptions(CompressionType.Deflate)
            {
                LeaveStreamOpen = false
            });
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\n\nTotal data written: {(Folders.Sum(x => x.Size) / 1_000_000f):F} MB!\n");
            Console.ForegroundColor = defaultColor;
        }
    }
}