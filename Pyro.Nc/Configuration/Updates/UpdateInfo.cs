using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Pyro.Nc.Configuration.Statistics;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Configuration.Updates
{
    public class UpdateInfo
    {
        private UpdateInfo(string name, Guid id, long length)
        {
            Name = name;
            Id = id;
            Length = length;
        }

        private static JsonSerializerOptions _defaultOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        
        public string Name { get; private set; }
        public Guid Id { get; private set; }
        public long Length { get; private set; }
        public byte[] Data { get; private set; }

        public static UpdateInfo CreateFrom(string file)
        {
            var json = File.ReadAllText(file);
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json, _defaultOptions);

            return updateInfo;
        }

        public static async Task<UpdateInfo> GetLatest()
        {
            if (!Globals.IsNetworkPresent)
            {
                return null;
            }

            var updateInfoJson = await Collector.HttpClient.GetStringAsync(Collector.BaseAddress + "/updatesapi");
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(updateInfoJson, _defaultOptions);
            return updateInfo;
        }
    }
}