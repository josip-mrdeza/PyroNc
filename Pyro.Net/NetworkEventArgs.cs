using System;

namespace Pyro.Net
{
    public class NetworkEventArgs : EventArgs
    {
        public byte[] Data { get; }

        public NetworkEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}