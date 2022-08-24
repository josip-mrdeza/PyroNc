using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pyro.Net
{
    public static class ServerSide
    {
        public static async Task<byte[]> ReadArrayFromStream(this Stream stream, long length)
        {
            var arr = new byte[length];
            await stream.ReadAsync(arr, 0, arr.Length);

            return arr;
        }
        
        public static async Task<DefaultContent> ReadJsonContentFromStream(this Stream stream)
        {
            var defaultContent = await JsonSerializer.DeserializeAsync(stream, typeof(DefaultContent));

            return (DefaultContent) defaultContent;
        }
    }
}