using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TinyClient;

namespace PyroNetServerIntermediateLibrary
{
    public static class FileRequestHelper
    {
        private static HttpClient Client = new HttpClient();
        private static string Address = "http://pi:5000/";
        private static string LocalAddress = "https://192.168.1.200:7028/";
        private static string GlobalAddress = "https://pyronetserver.azurewebsites.net/";
        private static string SetAddress;
        private static string Username;
        private static string Password;      
        
        public static async Task Init(string userName, string password)
        {
            Username = userName;
            Password = password;
            var ping = new Ping();
            if (ping.Send(Address).Status == IPStatus.Success)
            {
                SetAddress = Address;
            }
            else if (ping.Send(LocalAddress).Status == IPStatus.Success)
            {
                SetAddress = LocalAddress;
            }
            else if (ping.Send(GlobalAddress).Status == IPStatus.Success)
            {
                SetAddress = GlobalAddress;
            }
            else
            {
                throw new Exception("Server Status: Offline");
            }
            await Client.PostAsync($"{SetAddress}files/register?userName={userName}&password={password}", new ByteArrayContent(Array.Empty<byte>()));
        } 
        
        public static async Task PostFile(string fileName, string text)
        {
            await Client.PostAsync($"{SetAddress}/files/{fileName}?userName={Username}&password={Password}", new StringContent(text, Encoding.Default, "text/json"));
        }

        public static async Task<string> GetFile(string fileName)
        {
            var result = Client.GetAsync($"{SetAddress}/files/{fileName}?userName={Username}&password={Password}");
            var text = await result.Result.Content.ReadAsStringAsync();
            return text;
        }

        public static async Task<string[]> GetFiles()
        {
            var result = Client.GetAsync($"{SetAddress}/files/all?userName={Username}&password={Password}");
            var arrayJson = await result.Result.Content.ReadAsStringAsync();
            var array = arrayJson.DeserializeUtf8JsonInto<string[]>();

            return array;
        }

        public static async Task DeleteFile(string fileName)
        {
            await Client.DeleteAsync($"{SetAddress}/files/delete/{fileName}?userName={Username}&password={Password}");
        }
    }
}