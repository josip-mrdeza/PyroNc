using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Pyro.IO.Events;
using Pyro.Math;
using Pyro.Nc.Simulation;
using TinyClient;
using TinyServer;

namespace Pyro.Nc.UI.Net;

public class PyroReceiver : Receiver
{
     public Dictionary<string, Device> Devices = new Dictionary<string, Device>();
     
     public override async Task Resolve(HttpListenerContext context)
     {
          var packet = context.Request.InputStream.DeserializeUtf8BytesInto<Packet>();
          var str = $"[{packet.Sender.IpAddress}] ({packet.Sender.Name}): {packet.Utf8Bytes.DeserializeUtf8Bytes(packet.FullPacketTypeName)}";
          var name = packet.Sender.Name;
          if (Devices.ContainsKey(name))
          {
               var device = Devices[name];
               device.Received += context.Request.ContentLength64;
               device.Requests += 1;
          }
          else
          {
               Devices.Add(name, packet.Sender);
               packet.Sender.Received += context.Request.ContentLength64;
               packet.Sender.Requests += 1;
          }
          Globals.Console.Push(str);      
     }

     public override void Log(Hourglass hourglass)
     {
          string str;
          var ms = hourglass.Stopwatch.Elapsed.TotalMilliseconds;
          if (ms < 1d)
          {
               str = $"Finished request in {(hourglass.Stopwatch.Elapsed.TotalMilliseconds * 1000d).Round().ToString(CultureInfo.InvariantCulture)} ns";
          }
          else
          {
               str = $"Finished request in {hourglass.Stopwatch.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}ms";
          }
          Globals.Console.Push(str);   
     }
}