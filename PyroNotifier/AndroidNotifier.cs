using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;

namespace PyroNotifier
{
    public class AndroidNotifier
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        bool channelInitialized = false;
        int messageId = 0;
        int pendingIntentId = 0;

        public static NotificationManager manager;
        
        public AndroidNotifier() => Initialize();

        public void Initialize()
        {
            for (int i = 0; i < 3; i++)
            {
                CreateNotificationChannel(i);
            }
        }

        public Notification SendNotification(int channel, string title, string message, int? id = null, bool shouldVibrate = true,
            bool shouldSetProgress = false, int currentPercent = 0, int maxPercent = 100, bool isIndeterminate = false)
        {
            CreateNotificationChannel(channel);

            return Show(channel, title, message, id, shouldVibrate, shouldSetProgress, currentPercent, maxPercent, isIndeterminate);
        }

        public Notification Show(int channel, string title, string message, int? id = null, bool shouldVibrate = true,
            bool shouldSetProgress = false, int currentPercent = 0, int maxPercent = 100, bool isIndeterminate = false)
        {
            Intent intent = new Intent(Application.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(Application.Context, pendingIntentId++, intent,
                                          PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(Application.Context, channelId)
                                                 .SetContentIntent(pendingIntent)
                                                 .SetContentTitle(title)
                                                 .SetContentText(message)
                                                 .SetLargeIcon(
                                                     BitmapFactory.DecodeResource(
                                                         Application.Context.Resources,
                                                         Resource.Drawable.abc_list_pressed_holo_light))
                                                 .SetSmallIcon(Resource.Drawable.abc_list_selector_holo_light)
                                                 .SetColor(0x4cfcb0).SetChannelId($"Channel {channel}");
            if (shouldVibrate)
            {
                //builder.SetDefaults((int)NotificationDefaults.Vibrate);
            }
            else
            {
                builder.SetDefaults((int) NotificationDefaults.Lights)
                       .SetVibrate(new long[]
                       {
                           -1L
                       });
                manager.NotificationChannels[0].EnableVibration(false);
            }

            if (shouldSetProgress)
            {
                builder.SetProgress(maxPercent, currentPercent, isIndeterminate);
            }
            Notification notification = builder.Build();
            builder.SetTimeoutAfter(10000);
            manager.Notify(id ?? messageId++, notification);
            return notification;
        }

        void CreateNotificationChannel(int ch)
        {
            manager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel("Channel " + ch, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }
    }
}