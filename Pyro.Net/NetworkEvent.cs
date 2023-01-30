using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Pyro.Net
{
    public class NetworkEvent : IDisposable
    {
        public Lazy<List<NetworkEventSubscriber>> Subscribers { get; }
        public Stream EventStream { get; private set;}
        public bool IsActive { get; private set; }
        public string Url { get; private set; }
        public event EventHandler<NetworkEventArgs> OnEvent;
        public event EventHandler OnConnectedEvent;
        
        private readonly byte[] _matchSequence;
        private readonly Task _eventTaskRefresher;
        private static HttpClient _httpClient;
        
        public NetworkEvent(string matchSequence, string url = null)
        {
            _httpClient ??= new HttpClient(); 
            Subscribers = new Lazy<List<NetworkEventSubscriber>>();
            IsActive = true;
            Url = url ?? $"https://pyronetserver.azurewebsites.net/events?id={Assembly.GetCallingAssembly().GetName().Name}";
            _matchSequence = matchSequence.Select(x => (byte)x).ToArray();
            _eventTaskRefresher = Task.Run(async () => await RefreshStream());
        }

        private async Task RefreshStream()
        {
            byte[] buffer = new byte[_matchSequence.Length];
            byte[] intBuffer = new byte[4];
            while (IsActive)
            {
                try
                {
                    if (EventStream != null)
                    {
                        var isBasic = EventStream.ReadByte() == 0;
                        byte[] data = null;
                        if (!isBasic)
                        {
                            await EventStream.ReadAsync(intBuffer, 0, 4);
                            data = new byte[BitConverter.ToInt32(intBuffer, 0)];
                            await EventStream.ReadAsync(data, 0, data.Length);
                        } 
                        
                        var read = await EventStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == _matchSequence.Length)
                        {
                            for (int i = 0; i < read; i++)
                            {
                                bool ok = buffer[i] == _matchSequence[i];
                                if (!ok)
                                {
                                    break;
                                }
                            }

                            for (int i = 0; i < buffer.Length; i++)
                            {
                                buffer[i] = 0;
                            }

                            if (data != null)
                            {
                                OnEvent?.Invoke(this,new NetworkEventArgs(data));
                            }
                            else
                            {
                                OnEvent?.Invoke(this, new NetworkEventArgs(Array.Empty<byte>()));
                            }
                            foreach (var subscriber in Subscribers.Value)
                            {
                                if (subscriber.IsAsync)
                                {
                                    await subscriber.OnEventAsync();
                                }
                                else
                                {
                                    subscriber.OnEvent();
                                }
                            }
                        }
                    }
                    else
                    {
                        EventStream?.Dispose();
                        EventStream = null;
                        while (!await TryConnect())
                        {
                            // skip
                            Console.WriteLine($"Failed to connect to event server, trying again in 100ms...[{Url}]");
                            await Task.Delay(100);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    EventStream?.Dispose();
                    EventStream = null;
                    while (!await TryConnect())
                    {
                        // skip
                        Console.WriteLine($"Failed to connect to event server, trying again in 100ms...[{Url}]");
                        await Task.Delay(100);
                    }
                }
                finally
                {
                    //await Task.Delay(1);
                }
            }
        }

        private async Task<bool> TryConnect(bool closeAfter = false)
        {
            try
            {
                EventStream = await _httpClient.GetStreamAsync(Url);
            }
            catch
            {
                return false;
            }
            OnConnectedEvent?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void Dispose()
        {
            _eventTaskRefresher?.Dispose();
            EventStream?.Dispose();
            IsActive = false;
        }
    }
}