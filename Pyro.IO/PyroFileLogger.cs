using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pyro.IO.Logging
{
    public class PyroFileLogger : IPyroLogger
    {
        public string Id { get; set; }
        public Stream File { get; set; }
        public float UpTime { get => Timer?.ElapsedMilliseconds ?? float.NaN; }
        public Stopwatch Timer { get; }

        public PyroFileLogger()
        {
            Id = "UnityProj";
            Timer = Stopwatch.StartNew();
            File = System.IO.File.Create($"log_{Id}.txt");
            Log($"Time: {DateTime.Now.ToLongTimeString()}, Logging done by IPyroLogger created.");
            Log($"Logging started for assembly with ID: '{Id}'!");

            // #if DEBUG
            // AppDomain.CurrentDomain.FirstChanceException += async (sender, args) =>
            // {
            //     await LogAsync(args.Exception);
            // };
            // #endif
        }

        public void Log(string message)
        {
            var data = StringToBytes(message + '\n');
            File.Write(data, 0, data.Length);
            File.Flush();
        }

        public void Log(Exception exception)
        {
            Log('\n' + exception.ToString() + '\n');
        }

        public async Task LogAsync(string message)
        {
            var data = StringToBytes(message + '\n');
            await File.WriteAsync(data, 0, data.Length);
            await File.FlushAsync();
        }

        public async Task LogAsync(Exception exception)
        {
            await LogAsync('\n' + exception.ToString() + '\n');
        }

        public void Log(object obj)
        {
            Log(obj.ToString());
        }

        public async Task LogAsync(object obj)
        {
            await LogAsync(obj.ToString());
        }

        public byte[] StringToBytes(string s) => Encoding.Default.GetBytes(s);
    }
}
