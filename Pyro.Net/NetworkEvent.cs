using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Pyro.Threading;

namespace Pyro.Net
{
    public class NetworkEvent : IDisposable
    {
        public Lazy<List<NetworkEventSubscriber>> Subscribers { get; }
        public Stream EventStream { get; private set;}
        public bool IsActive { get; private set; }
        public string Url { get; }
        public string Id { get; }
        public string Sequence { get; }
        public string LastServerMessage { get; private set; }
        public event EventHandler<NetworkEventArgs> OnEvent;
        public event EventHandler OnConnectedEvent;
        public event EventHandler OnBeginConnectingEvent;
        public readonly ConcurrentQueue<Action> Queue;
        public readonly ConcurrentQueue<Func<Task>> AsyncQueue;
        public TimeSpan Delay;
        private readonly byte[] _matchSequence;
        private Task _eventTaskRefresher;
        private readonly HttpClient _httpClient;
        private readonly bool _executeOnMainThread;
        
        public NetworkEvent(string eventId, string matchSequence, bool executeEventsOnMainThread = false, bool startImplicitly = true)
        {
            _httpClient = new HttpClient();
            Delay = TimeSpan.FromMilliseconds(25);
            Subscribers = new Lazy<List<NetworkEventSubscriber>>();
            IsActive = true;
            var url = "https://pyronetserver.azurewebsites.net/events";
            Url = $"{url}?id={eventId}";
            Id = eventId;
            Sequence = matchSequence;
            _executeOnMainThread = executeEventsOnMainThread;
            _matchSequence = matchSequence.Select(x => (byte)x).ToArray();
            if (startImplicitly)
            {
                StartListening();
            }
            if (executeEventsOnMainThread)
            {
                Queue = new ConcurrentQueue<Action>();
                AsyncQueue = new ConcurrentQueue<Func<Task>>();
            }
        }

        public void StartListening()
        {
            if (_eventTaskRefresher == null)
            {
                _eventTaskRefresher = Task.Run(async () => await RefreshStream());
            }
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
                            var num = BitConverter.ToInt32(intBuffer, 0);
                            if (num > 10_000)
                            {
                                continue;
                            }
                            data = new byte[num];
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
                                if (_executeOnMainThread)
                                {
                                    Queue.Enqueue(() => OnEvent?.Invoke(this, new NetworkEventArgs(data)));
                                }
                                else
                                {
                                    OnEvent?.Invoke(this,new NetworkEventArgs(data));
                                }
                            }
                            else
                            {
                                if (_executeOnMainThread)
                                {
                                    Queue.Enqueue(() => OnEvent?.Invoke(this, new NetworkEventArgs(Array.Empty<byte>())));
                                }
                                else
                                {
                                    OnEvent?.Invoke(this,new NetworkEventArgs(Array.Empty<byte>()));
                                }
                            }
                            foreach (var subscriber in Subscribers.Value)
                            {
                                if (subscriber.IsAsync)
                                {
                                    if (_executeOnMainThread)
                                    {
                                        AsyncQueue.Enqueue(subscriber.OnEventAsync);
                                    }
                                    else
                                    {
                                        await subscriber.OnEventAsync();
                                    }
                                }
                                else
                                {
                                    if (_executeOnMainThread)
                                    {
                                        Queue.Enqueue(subscriber.OnEvent);
                                    }
                                    else
                                    {
                                        subscriber.OnEvent();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        EventStream = null;
                        if (_executeOnMainThread)
                        {
                            Queue.Enqueue(() => OnBeginConnectingEvent?.Invoke(this, EventArgs.Empty));
                        }
                        else
                        {
                            OnBeginConnectingEvent?.Invoke(this, EventArgs.Empty);
                        }
                        while (!await TryConnect())
                        {
                            // skip
                            Console.WriteLine($"Failed to connect to event server, trying again in 100ms...[{Url}]");
                            await Task.Delay(100);
                        }
                    }
                    

                    await Task.Delay(Delay);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    EventStream?.Dispose();
                    EventStream = null;
                    if (_executeOnMainThread)
                    {
                        Queue.Enqueue(() => OnBeginConnectingEvent?.Invoke(this, EventArgs.Empty));
                    }
                    else
                    {
                        OnBeginConnectingEvent?.Invoke(this, EventArgs.Empty);
                    }                  
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
                    await Task.Delay(Delay);
                }
            }
            Dispose();
            await KillEvent(Id);
        }

        private async Task<bool> TryConnect()
        {
            try
            {
                EventStream = await _httpClient.GetStreamAsync(Url);
            }
            catch
            {
                return false;
            }
            if (_executeOnMainThread)
            {
                Queue.Enqueue(() => OnConnectedEvent?.Invoke(this, EventArgs.Empty));
            }
            else
            {
                OnConnectedEvent?.Invoke(this, EventArgs.Empty);
            }
            return true;
        }

        public void Dispose()
        {
            _eventTaskRefresher?.Dispose();
            EventStream?.Dispose();
            IsActive = false;
        }

        public static NetworkEvent ListenToEvent(string id, string password, bool startImplicitly = true)
        {
            var ne = new NetworkEvent(id, password, startImplicitly:startImplicitly);

            return ne;
        }

        public static async Task<bool> InvokeEvent(string id, string password, string content)
        {
            return await NetHelpers.Post($"https://pyronetserver.azurewebsites.net/events/invoke?id={id}&sequence={password}", content);
        }

        public static async Task<bool> KillEvent(string id)
        {
            return await NetHelpers.Post($"https://pyronetserver.azurewebsites.net/events/close?id={id}");
        }
    }
}