using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TinyClient;

namespace Pyro.Net
{
    public static class NetHelpers
    {
        private static HttpClient Client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
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

        public static async Task<byte[]> GetData(string addr)
        {
            var resp = await Client.GetAsync(addr);
            if (!resp.IsSuccessStatusCode)
            {
                return default;
            }
            var content = await resp.Content.ReadAsByteArrayAsync();

            return content;
        }

        public static async Task<bool> PostJson<T>(this T obj, string addr)
        {
            return (await Client.PostAsync(addr, new StringContent(obj.SerializeToUtf8Json(),
                                                                   Encoding.Default, "text/json"))).IsSuccessStatusCode;
        }

        public static async Task<bool> Post(string addr)
        {
            return (await Client.PostAsync(addr, new StringContent(""))).IsSuccessStatusCode;
        }
        
        public static async Task<HttpResponseMessage> PostWithDetails(string addr)
        {
            return await Client.PostAsync(addr, new StringContent(""));
        }
        
        public static async Task<bool> Post(string addr, string content)
        {
            return (await Client.PostAsync(addr, new StringContent(content))).IsSuccessStatusCode;
        }
        
        public static async Task<bool> Put(string addr)
        {
            return (await Client.PutAsync(addr, new StringContent(""))).IsSuccessStatusCode;
        }
        
    }
}