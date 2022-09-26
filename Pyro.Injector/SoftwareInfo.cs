using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Pyro.IO;

namespace Pyro.Injector
{
    public class SoftwareInfo
    {
        public FolderInfo[] Folders { get; set; }
        public string Path { get; set; }
        public bool IsUnity { get; set; }
        internal void Init()
        {
            Path = Directory.GetCurrentDirectory();
            IsUnity = Path.Contains("_Data\\Managed");
            if (IsUnity)
            {
                var _dataIndex = Path.IndexOf("_Data", StringComparison.InvariantCulture);
                Path = Path.Remove(_dataIndex);
                var _mainIndex = Path.LastIndexOf("\\", StringComparison.InvariantCulture);
                Path = Path.Remove(_mainIndex - 1);
            }
            // Folders = Directory.EnumerateDirectories(Path)
            //                    .Prepend(Path)
            //                    .Select(d => new DirectoryInfo(d))                                                                               
            //                    .Select(di => new FolderInfo(di.FullName.Replace(Path + "\\", string.Empty),-1))
            //                    .ToArray();

            Folders = new FolderInfo[]{new FolderInfo(Path)};
        }
        
        public static SoftwareInfo GetFromCache()
        {
            var assemblyId = Assembly.GetCallingAssembly().GetName().Name;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate($"PyroNc/{assemblyId}");
            SoftwareInfo softwareInfo;
            var jsonId = "SoftwareInfo_Cache.json";
            if (roaming.Exists(jsonId))
            {
                softwareInfo = roaming.ReadFileAs<SoftwareInfo>(jsonId);

                return softwareInfo;
            }

            softwareInfo = new SoftwareInfo();
            softwareInfo.Init();
            roaming.AddFile(jsonId, softwareInfo);
            return softwareInfo;
        }
        
        public static SoftwareInfo GetFromCache(string app)
        {
            var assemblyId = app;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate($"PyroNc/{assemblyId}");
            SoftwareInfo softwareInfo;
            var jsonId = "SoftwareInfo_Cache.json";
            if (roaming.Exists(jsonId))
            {
                softwareInfo = roaming.ReadFileAs<SoftwareInfo>(jsonId);

                return softwareInfo;
            }

            softwareInfo = new SoftwareInfo();
            softwareInfo.Init();
            roaming.AddFile(jsonId, softwareInfo);
            return softwareInfo;
        }

        public static UpdateInfo GetFromCache(string app, string updateName)
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate($"PyroNc/{app}");
            SoftwareInfo softwareInfo;
            UpdateInfo updateInfo;
            var jsonId = "SoftwareInfo_Cache.json";
            if (roaming.Exists(jsonId))
            {
                softwareInfo = roaming.ReadFileAs<SoftwareInfo>(jsonId);
                updateInfo = new UpdateInfo(updateName, softwareInfo.Folders.Sum(x => x.Size), softwareInfo.Folders);
                return updateInfo;
            }

            throw new Exception($"No software info was found in path {roaming.Site}!");
        }

        public static UpdateInfo GetUpdateFromCache(string updateId)
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc");
            UpdateInfo updateInfo;
            if (roaming.Exists(updateId))
            {
                updateInfo = roaming.ReadFileAs<UpdateInfo>(updateId);
                return updateInfo;
            }

            throw new Exception($"No update info was found in path {roaming.Site}!");
        }

        public void Refresh()
        {
            var assemblyId = Assembly.GetCallingAssembly().GetName().Name;
            LocalRoaming roaming = LocalRoaming.OpenOrCreate($"PyroNc/{assemblyId}");
            var jsonId = "SoftwareInfo_Cache.json";
            Init();
            roaming.ModifyFile(jsonId, this);
        }
        
        public class BasicInfo
        {
            public BasicInfo(string path, string basePath, long length)
            {
                Path = path;
                BasePath = basePath;
                Size = length;
            }
            public string Path { get; set; }
            public long Size { get; set; }
            public string BasePath { get; set; }
            internal FolderInfo Parent { get; set; }
        }
        public class DocumentInfo : BasicInfo
        {
            [JsonConstructor]
            public DocumentInfo(string path, string basePath, long size, byte[] data) : base(path, basePath, size)
            {
                Data = data;
            }
            public DocumentInfo(string path, string basePath, long size, byte[] data, FolderInfo parent = null) : base(path, basePath, size)
            {
                Data = data;
                Parent = parent;
                if (parent != null)
                {
                    Parent.Size += size;
                    Parent = null;
                }
            }
            public byte[] Data { get; set; }
            public void RefreshData()
            {
                Data = File.ReadAllBytes(Path);
            }
            
            public void RefreshDataAlt()
            {
                Data = File.ReadAllBytes($"{BasePath}/{Path}");
            }


            public void Update()
            {
                File.WriteAllBytes(Path, Data);
            }

            public bool IsSameAs(DocumentInfo other)
            {
                if (Size != other.Size)
                {
                    return false;
                }
                if (Data is null)
                {
                    RefreshData();
                }

                if (other.Data is null)
                {
                    throw new Exception("Update file cannot contain null variable Data.");
                }
                for (int i = 0; i < Size; i++)
                {
                    if (Data[i] != other.Data[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        public class FolderInfo : BasicInfo
        {
            [JsonConstructor]
            public FolderInfo(string path, string basePath, long size, FolderInfo[] directories, DocumentInfo[] documents) : base(path, basePath, size)
            {
                Directories = directories;
                Documents = documents;
            }
            
            public FolderInfo(string path, string basePath, long size, FolderInfo parent = null) : base(path, basePath, size)
            {
                FindDirectories(Path);
                FindFiles(Path);
                Parent = parent;
                if (parent != null)
                {
                    Parent.Size += size;
                    Parent = null;
                }
            }
            
            public FolderInfo(string path) : base(path, path, -1)
            {
                FindFiles(Path);
                FindDirectories(Path);
                UpdateSize();
            }

            public void FindDirectories(string localDirectory)
            {
                Directories = Directory.EnumerateDirectories(localDirectory)
                                       .Select(d => new DirectoryInfo(d))
                                       .Select(di => new FolderInfo(di.FullName.RemoveBasePath(BasePath), BasePath, -1, this))
                                       .ToArray();
            }
            
            public void FindFiles(string localDirectory)
            {
                Documents = Directory.EnumerateFiles(localDirectory)
                                     .Select(f => new FileInfo(f))
                                     .Select(fi => new DocumentInfo(fi.FullName.RemoveBasePath(BasePath), BasePath, fi.Length, null, this))
                                     .ToArray();
            }

            public void UpdateSize()
            {
                foreach (var directory in Directories)
                {
                    directory.UpdateSize();
                    Size += directory.Size;
                }
            }
            public FolderInfo[] Directories { get; set; }
            public DocumentInfo[] Documents { get; set; }
        }
    }
}