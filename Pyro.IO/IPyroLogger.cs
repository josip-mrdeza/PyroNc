using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Pyro.IO.Logging
{
    public interface IPyroLogger
    {
        public string Id { get; set; }
        public Stream File { get; set; }
        public float UpTime { get; }
        public Stopwatch Timer { get; }

        public void Log(string message);
        public Task LogAsync(string message);
        public void Log(object obj);
        public Task LogAsync(object obj);

        public byte[] StringToBytes(string s);
    }
}
