using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pyro.IO.Logging{
  public class PyroConsoleLogger : IPyroLogger{
    public PyroConsoleLogger(string id, Stream consoleStream){
      Id = id;
      File = consoleStream;
      Timer = Stopwatch.StartNew();
    }
    ~PyroConsoleLogger(){
      File.Dispose();
    }

    public string Id {get;set;}

    public void Log(string message){
      var bytes = StringToBytes(message);
      File.Write(bytes, 0, bytes.Length);
    }
    public async Task LogAsync(string message){
      var bytes = StringToBytes(message);
      File.WriteAsync(bytes, 0, bytes.Length);
    }

    public void Log(object obj){
      var bytes = StringToBytes(obj.ToString());
      File.Write(bytes, 0, bytes.Length);
    }
    public async Task LogAsync(object obj){
      var bytes = StringToBytes(obj.ToString());
      File.WriteAsync(bytes, 0, bytes.Length);
    }

    public byte[] StringToBytes(string s){
      var bytes = Encoding.Default.GetBytes(s);
      return bytes;
    }
    public string Id {get;set;}
    public Stream File {get;set;}
    public float UpTime {get => Timer.ElapsedMiliseconds;}
    public Stopwatch Timer {get;}
  }
}
