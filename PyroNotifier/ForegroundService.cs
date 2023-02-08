using Android.App;
using Android.Content;
using Android.OS;
using Pyro.Net;

namespace PyroNotifier{

    [Service]
    public class ForegroundService : Service
    {
        public override IBinder OnBind(Intent intent) => null;
        public NetworkEvent Event;
        public AndroidNotifier Notifier;
        public string EventId = "Exception";
        public string EventPassword = "Pyro";
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Notifier = new AndroidNotifier();
            Event = NetHelpers.ListenToEvent(EventId, EventPassword);
            Event.OnBeginConnectingEvent += (sender, args) =>
            {
                Notifier.SendNotification("Connecting...", "{}");
            };
            Event.OnConnectedEvent += (sender, args) =>
            {
                Notifier.SendNotification("Connected to event!", $"Name: {EventId}\nPassword: {EventPassword}");
            };
            Event.OnEvent += (sender, args) =>
            {
                Notifier.SendNotification("Exception", args.StringData.Value);
            };
            Notifier.SendNotification("Started service", "");
            return StartCommandResult.NotSticky;
        }
    }
}