using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Pyro.IO
{
    public class LocalVariables
    {
        public Dictionary<string, FileInfo> Files { get; private set; }
        public Dictionary<string, DirectoryInfo> Folders { get; private set; }
        public string Site { get; set; }

        public void Init(string name)
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
        
        public byte[] GetVariableDataAsBytes(string variableId)
        {
            var fn = Files[variableId].FullName;

            return File.ReadAllBytes(fn);
        }
        
        public string GetVariableDataAsString(string variableId)
        {
            var fn = Files[variableId].FullName;

            return File.ReadAllText(fn);
        }
        
        public T GetVariableDataAsT<T>(string variableId)
        {
            var fn = Files[variableId].FullName;

            return JsonSerializer.Deserialize<T>(File.ReadAllText(fn));
        }

        public void ChangeVariable(string variableId, byte[] content)
        {
            var fn = Files[variableId].FullName;
            File.WriteAllBytes(fn, content);
        }

        public void ChangeVariable(string variableId, string content)
        {
            if (!Files.ContainsKey(variableId))
            {
                AddVariable(variableId);
            }
            var fn = Files[variableId].FullName;
            File.WriteAllText(fn, content);
        }
        
        public FileInfo AddVariable(string variableId)
        {
            if (Files.ContainsKey(variableId))
            {
                return Files[variableId];
            }
            var fn = new FileInfo(Site + variableId);
            Files.Add(variableId, fn);
            File.Create(fn.FullName);
            return fn;
        }
        
        public FileInfo AddVariable(string variableId, string content)
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
        
        public FileInfo AddVariable<T>(string variableId, T content)
        {
            if (Files.ContainsKey(variableId))
            {
                File.WriteAllText(Files[variableId].FullName, JsonSerializer.Serialize(content));
                return Files[variableId];
            }
            var fn = new FileInfo($"{Site}\\{variableId}");
            Files.Add(variableId, fn);
            File.Create(fn.FullName).Dispose();
            File.WriteAllText(fn.FullName, JsonSerializer.Serialize(content));
            return fn;
        }

        public bool Exists(string variableId)
        {
            return Files.ContainsKey(variableId);
        }
    }
}