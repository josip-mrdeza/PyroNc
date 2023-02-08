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
        public event EventHandler OnBeginConnectingEvent;
        
        private readonly byte[] _matchSequence;
        private readonly Task _eventTaskRefresher;
        private static HttpClient _httpClient;
        
        public NetworkEvent(string eventId, string matchSequence)
        {
            _httpClient ??= new HttpClient(); 
            Subscribers = new Lazy<List<NetworkEventSubscriber>>();
            IsActive = true;
            var url = "https://pyronetserver.azurewebsites.net/events";
            Url = $"{url}?id={eventId}";
            _matchSequence = matchSequence.Select(x => (byte)x).ToArray();
            _eventTaskRefresher = Task.Run(async () => await RefreshStream());
        }

        private async Task RefreshStream()
        {
            while (IsActive)
            {
                try
                {
                    byte[] buffer = new byte[_matchSequence.Length];
                    byte[] intBuffer = new byte[4];
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
                            bool ok = false;
                            for (int i = 0; i < read; i++)
                            {
                                ok = buffer[i] == _matchSequence[i];
                                if (!ok)
                                {
                                    break;
                                }
                            }

                            if (!ok)
                            {
                                continue;
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
                        EventStream = null;
                        OnBeginConnectingEvent?.Invoke(this, EventArgs.Empty);
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
                    OnBeginConnectingEvent?.Invoke(this, EventArgs.Empty);
                    while (!await TryConnect())
                    {
                        // skip
                        Console.WriteLine($"Failed to connect to event server, trying again in 100ms...[{Url}]");
                        await Task.Delay(100);
                    }
                    //OnConnectedEvent?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    await Task.Delay(10);
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