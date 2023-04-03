using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using apartmaniLib;
using Pyro.Net;

namespace PyroNotifier{

    [Service]
    public class ForegroundService : Service
    {
        public override IBinder OnBind(Intent intent) => null;
        public NetworkEvent Event;
        public NetworkEvent Event2;
        public AndroidNotifier Notifier;
        public string EventId = "Exception";
        public string EventPassword = "Pyro";
        public Thread Timer;
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                Notifier ??= new AndroidNotifier();
                Notifier.Initialize();
                var n = Notifier.SendNotification(2, "s", "s2", 69, false);
                StartForeground(69, n);
                if (Event is null)
                {
                    Event = NetworkEvent.ListenToEvent(EventId, EventPassword);
                    AddSubs(Event);
                }

                if (Event2 is null)
                {
                    Event2 = NetworkEvent.ListenToEvent(Ids.NetworkId, Ids.NetworkSequence);
                    AddSubs(Event2);
                }
                Timer = new Thread(async () => {
                    DateTime start = DateTime.Now;
                    while (true)
                    {
                        var elapsed = (DateTime.Now - start);
                        var ts = new TimeSpan(elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
                        Notifier.SendNotification(2, "Waiting for events...", $"{ts.ToString()} elapsed",
                                                  69, false, true, 0, 100, true);
                        await Task.Delay(1000);
                    }
                });
                Timer.Start();
            }
            catch (Exception e)
            {
                Notifier.SendNotification(3, "An error has occured", e.Message, 999);
            }
            return StartCommandResult.Sticky;
        }
        private void AddSubs(NetworkEvent @event)
        {
            var defhash = @event.GetHashCode();
            // @event.OnBeginConnectingEvent += (sender, args) =>
            // {
            //     Notifier.SendNotification("Connecting...", "", defhash, shouldVibrate: false, shouldSetProgress: true,
            //                               isIndeterminate: true, currentPercent: 0, maxPercent: 100);
            // };
            // @event.OnConnectedEvent += (sender, args) =>
            // {
            //     Notifier.SendNotification("Connected to event!", $"Id={@event.Id} & Match={@event.Sequence}", defhash,
            //                               shouldVibrate: false);
            // };
            @event.OnEvent += (sender, args) =>
            {
                //var str = args.StringData.Value.Split(' ');
                Notifier.SendNotification(1, $"[{@event.Id}]: Received event" , args.StringData.Value, defhash + 10);
            };
        }

        public override void OnDestroy()
        {
            try
            {
                Notifier.SendNotification(3, "Redelivering", "", 999);
                Timer.Abort();
                OnStartCommand(null, StartCommandFlags.Redelivery, 0);
            }
            catch (Exception e)
            {
                Notifier.SendNotification(3, "An error has occured", e.Message, 999);
            }
        }
    }
}