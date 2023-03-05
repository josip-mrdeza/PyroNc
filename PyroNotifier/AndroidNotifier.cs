using System;
using System.ComponentModel;
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

        NotificationManager manager;
        
        public AndroidNotifier() => Initialize();

        public void Initialize()
        {
            CreateNotificationChannel();
        }

        public void SendNotification(string title, string message)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            Show(title, message);
        }

        public void Show(string title, string message)
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
                                                         Application.Context.Resources, Resource.Drawable.notification_bg))
                                                 .SetSmallIcon(Resource.Drawable.notification_bg)
                                                 .SetDefaults((int)NotificationDefaults.Sound |
                                                              (int)NotificationDefaults.Vibrate);

            Notification notification = builder.Build();
            builder.SetTimeoutAfter(10000);
            manager.Notify(messageId++, notification);
        }

        void CreateNotificationChannel()
        {
            manager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }
    }
}