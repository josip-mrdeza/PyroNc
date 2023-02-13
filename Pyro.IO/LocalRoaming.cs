using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pyro.IO
{
    public class LocalRoaming
    {
        public Dictionary<string, FileInfo> Files { get; private set; }
        public Dictionary<string, DirectoryInfo> Folders { get; private set; }
        public string Site { get; private set; }
        public DirectoryInfo Info { get; private set; }
        
        private static readonly Dictionary<string, LocalRoaming> Roamings = new Dictionary<string, LocalRoaming>();
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        private readonly byte[] _emptyBytes = Array.Empty<byte>();

        private LocalRoaming(string id)
        {
            Init(id);
            Roamings.Add(id, this);
        }

        public static LocalRoaming OpenOrCreate(string id)
        {
            LocalRoaming roaming;
            if (Roamings.ContainsKey(id))
            {
                roaming = Roamings[id];
                return roaming;
            }

            roaming = new LocalRoaming(id);

            return roaming;
        }
        
        private void Init(string name)
        {
            Site = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{name}\\";
            if (!Directory.Exists(Site))
            {
                Info = Directory.CreateDirectory(Site);
            }
            else
            {
                Info = new DirectoryInfo(Site);
            }

            var files = Directory.EnumerateFiles(Site);
            var folders = Directory.EnumerateDirectories(Site);
            Files = files.ToDictionary(k => k.Remove(0, k.LastIndexOf('\\') + 1), fi => new FileInfo(fi));
            Folders = folders.ToDictionary(k => k.Remove(0, k.LastIndexOf('\\') + 1), di => new DirectoryInfo(di));
        }

        public IEnumerable<DirectoryInfo> ListAllDirectories()
        {
            return Info.EnumerateDirectories();
        }

        public IEnumerable<T> ListAll<T>()
        {
            return Files.Values.Select(x => JsonSerializer.Deserialize<T>(File.ReadAllText(x.FullName), Options));
        }

        public IEnumerable<string> ListAll()
        {
            return Files.Values.Select(x => x.Name);
        }

        public IEnumerable<string> ListContents()
        {
            return Files.Values.Select(x => File.ReadAllText(x.FullName));
        }
        
        public byte[] ReadFile(string variableId)
        {
            if (!Files.ContainsKey(variableId))
            {
                return _emptyBytes;
            }
            var fn = Files[variableId].FullName;

            return File.ReadAllBytes(fn);
        }
        public async Task<byte[]> ReadFileAsync(string variableId)
        {
            FileInfo fi;
            if (!Files.ContainsKey(variableId))
            {
                return _emptyBytes;
            }
            fi = Files[variableId];
            using var fs = fi.OpenRead();
            byte[] arr = new byte[fi.Length];
            var length = arr.Length;
            int read = 0;
            while (read < length)
            {
                read += await fs.ReadAsync(arr, 0, arr.Length);
            }

            return arr;
        }
        
        public string ReadFileAsText(string variableId)
        {
            if (!Files.ContainsKey(variableId))
            {
                return string.Empty;
            }
            var fn = Files[variableId].FullName;

            return File.ReadAllText(fn);
        }
        
        public T ReadFileAs<T>(string variableId)
        {
            T value = default;
            try
            {
                value = JsonSerializer.Deserialize<T>(ReadFileAsText(variableId), Options);
            }
            catch
            {
                //ignore
            }

            return value;
        }

        public void ModifyFile(string variableId, byte[] content)
        {
            FileInfo fi;
            if (!Files.ContainsKey(variableId))
            {
                fi = AddFile(variableId);
            }
            else
            {
                fi = Files[variableId];
            }
            using var fs = fi.OpenWrite();
            fs.Write(content, 0, content.Length);
        }

        public async Task ModifyFileAsync(string variableId, byte[] content)
        {
            FileInfo fi;
            if (!Files.ContainsKey(variableId))
            {
                fi = AddFile(variableId);
            }
            else
            {
                fi = Files[variableId];
            }
            using var fs = fi.OpenWrite();
            await fs.WriteAsync(content, 0, content.Length);
        }

        public void ModifyFile(string variableId, string content)
        {
            FileInfo fi;
            if (!Files.ContainsKey(variableId))
            {
                fi = AddFile(variableId);
            }
            else
            {
                fi = Files[variableId];
            }
            var fn = fi.FullName;
            File.WriteAllText(fn, content);
        }
        
        public async Task ModifyFileAsync(string variableId, string content)
        {
            FileInfo fi;
            if (!Files.ContainsKey(variableId))
            {
                fi = AddFile(variableId);
            }
            else
            {
                fi = Files[variableId];
            }
            using var fs = fi.OpenWrite();
            var bytes = Encoding.Default.GetBytes(content);
            await fs.WriteAsync(bytes, 0, bytes.Length);
        }
        
        public void ModifyFile<T>(string variableId, T content)
        {
            ModifyFile(variableId, JsonSerializer.Serialize(content, Options));
        }
        
        public async Task ModifyFileAsync<T>(string variableId, T content)
        {
            await ModifyFileAsync(variableId, JsonSerializer.Serialize(content, Options));
        }
        
        public FileInfo AddFile(string variableId)
        {
            if (Files.ContainsKey(variableId))
            {
                return Files[variableId];
            }
            var fn = new FileInfo(Site + variableId);
            Files.Add(variableId, fn);
            File.Create(fn.FullName).Dispose();
            return fn;
        }

        public FileStream AddFileNoLock(string variableId, FileAccess access)
        {
            FileInfo info;
            FileStream stream;
            if (Files.ContainsKey(variableId))
            {
                info = Files[variableId];
                stream = new FileStream(info.FullName, FileMode.OpenOrCreate, access, FileShare.ReadWrite);
                return stream;
            }
            info = new FileInfo(Site + variableId);
            stream = new FileStream(info.FullName, FileMode.OpenOrCreate, access, FileShare.ReadWrite);
            Files.Add(variableId, info);
            return stream;
        }
        
        public FileInfo AddFile(string variableId, byte[] data)
        {
            if (Files.ContainsKey(variableId))
            {
                return Files[variableId];
            }
            var fn = new FileInfo(Site + variableId);
            Files.Add(variableId, fn);
            File.WriteAllBytes(fn.FullName, data);
            return fn;
        }
        
        public FileInfo AddFile(string variableId, string content)
        {
            if (Files.ContainsKey(variableId))
            {
                return Files[variableId];
            }
            var fn = new FileInfo($"{Site}\\{variableId}");
            Files.Add(variableId, fn);
            File.Create(fn.FullName).Dispose();
            File.WriteAllText(fn.FullName, content);
            return fn;
        }
        
        public FileInfo AddFile<T>(string variableId, T content)
        {
            var fn = AddFile(variableId, JsonSerializer.Serialize(content, Options));
            return fn;
        }

        public bool Exists(string variableId)
        {
            return Files.ContainsKey(variableId);
        }

        public void Delete(string variableId)
        {
            File.Delete(Site + variableId);
            Files.Remove(variableId);
        }
    }
}