using System;
using System.Text;

namespace Pyro.Net
{
    public class NetworkEventArgs : EventArgs
    {
        public byte[] Data { get; }
        
        public Lazy<string> StringData { get; }

        public NetworkEventArgs(byte[] data)
        {
            Data = data;
            StringData = new Lazy<string>(() => Encoding.UTF8.GetString(Data));
        }
    }
}