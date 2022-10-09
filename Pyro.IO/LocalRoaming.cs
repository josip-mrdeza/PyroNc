using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pyro.IO
{
    public class LocalRoaming
    {
        public Dictionary<string, FileInfo> Files { get; private set; }
        public Dictionary<string, DirectoryInfo> Folders { get; private set; }
        public string Site { get; set; }
        private static readonly Dictionary<string, LocalRoaming> Roamings = new Dictionary<string, LocalRoaming>();

        private static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        private byte[] _emptyBytes = new byte[0];

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
                Directory.CreateDirectory(Site);
            }

            var files = Directory.EnumerateFiles(Site);
            var folders = Directory.EnumerateDirectories(Site);
            Files = files.ToDictionary(k => k.Remove(0, k.LastIndexOf('\\') + 1), fi => new FileInfo(fi));
            Folders = folders.ToDictionary(k => k.Remove(0, k.LastIndexOf('\\') + 1), di => new DirectoryInfo(di));
        }

        public IEnumerable<T> ListAll<T>()
        {
            return Files.Values.Select(x => JsonSerializer.Deserialize<T>(File.ReadAllText(x.FullName)));
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
            var fn = Files[variableId].FullName;
            File.WriteAllBytes(fn, content);
        }

        public void ModifyFile(string variableId, string content)
        {
            if (!Files.ContainsKey(variableId))
            {
                AddFile(variableId);
            }
            var fn = Files[variableId].FullName;
            File.WriteAllText(fn, content);
        }
        
        public void ModifyFile<T>(string variableId, T content)
        {
            ModifyFile(variableId, JsonSerializer.Serialize(content));
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
            var fn = AddFile(variableId, JsonSerializer.Serialize(content));
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