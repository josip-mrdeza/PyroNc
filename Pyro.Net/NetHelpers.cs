using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TinyClient;
using UnityEngine;

namespace Pyro.Net
{
    public static class NetHelpers
    {
        private static HttpClient Client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        public static async Task<bool> Ping(this string addr)
        {
            try
            {
                await Client.GetAsync(addr);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<T> GetJson<T>(string addr)
        {
            var resp = await Client.GetAsync(addr);
            if (!resp.IsSuccessStatusCode)
            {
                return default;
            }

            var content = await resp.Content.ReadAsStringAsync();
            return content.DeserializeUtf8JsonInto<T>();
        }

        public static async Task PostJson<T>(this T obj, string addr)
        {
            await Client.PostAsync(addr, new StringContent(obj.SerializeToUtf8Json(), Encoding.Default, "text/json"));
        }

        public static async Task Post(string addr)
        {
            await Client.PostAsync(addr, new StringContent(""));
        }
        
        public static async Task Post(string addr, string content)
        {
            await Client.PostAsync(addr, new StringContent(content));
        }
        
        public static async Task Put(string addr)
        {
            await Client.PutAsync(addr, new StringContent(""));
        }

        public static NetworkEvent ListenToEvent(string id, string password)
        {
            var ne = new NetworkEvent(id, password);

            return ne;
        }
    }
}