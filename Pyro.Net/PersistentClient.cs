using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Pyro.IO;
namespace Pyro.Net
{
    public static class PersistentClient
    {
        private static readonly HttpClient _client = new HttpClient();
        private static LocalRoaming Roaming = LocalRoaming.OpenOrCreate("Pyro.Net");
        public static string ID = null;
        
        public static async Task<DefaultContent> Get(this string id, string url, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(id))
            {
                url += $"/{id}";
            }
            return JsonSerializer.Deserialize<DefaultContent>(await (await _client.GetAsync(url, token)).Content.ReadAsStringAsync(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            });
        }
        
        public static async Task<T> Get<T>(this string id, string url, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(id))
            {
                url += $"/{id}";
            }
            return JsonSerializer.Deserialize<T>(await (await _client.GetAsync(url, token)).Content.ReadAsStringAsync(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            });
        }

        public static async Task<DefaultContent> Get(this string id, CancellationToken token)
        {
            return await id.Get("http://localhost:5000/playerregistry", token);
        }
        
        public static async Task<T> Get<T>(this string id, CancellationToken token)
        {
            return await id.Get<T>("http://localhost:5000/playerregistry", token);
        }
        
        public static async Task<HttpResponseMessage> Send(this DefaultContent obj, string url, CancellationToken token)
        {
            if (ID == null)
            {
                Roaming.AddFile("PlayerInfo.txt", new Random().Next(-69420, 69420).ToString());
                ID = Roaming.ReadFileAsText("PlayerInfo.txt");
                url = url.Replace("id=", $"id={ID}");
            }
            obj.SenderId = ID;
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            }, token);
            stream.Capacity = (int) stream.Length;
            var content = new StringContent(Encoding.ASCII.GetString(stream.GetBuffer()));
            return await _client.PostAsync(url, content, token);
        }

        public static async Task<HttpResponseMessage> Send(this DefaultContent obj, CancellationToken token = default)
        {
            return await obj.Send($"http://localhost:5000/playerregistry?id={obj.SenderId}", token);
        }
    }
}