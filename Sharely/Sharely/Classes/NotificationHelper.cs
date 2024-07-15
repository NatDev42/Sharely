using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Sharely.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.Core.App;
using Sharely;

namespace Sharely.Classes
{
    public static class NotificationHelper
    {
        public const string CHANNEL_ID = "BackgroundMusicServiceChannel";

        public static void CreateNotificationChannel(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelName = "Background Music Service Channel";
                var channelDescription = "Channel for background music service";
                var channel = new NotificationChannel(CHANNEL_ID, channelName, NotificationImportance.Low)
                {
                    Description = channelDescription
                };
                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public static Notification CreateNotification(Context context)
        {
            var notificationIntent = new Intent(context, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(context, 0, notificationIntent, PendingIntentFlags.Immutable);

            var notificationBuilder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .SetContentTitle("Background Music Service")
                .SetContentText("Playing background music")
                .SetSmallIcon(Resource.Drawable.SharelyCompact)
                .SetContentIntent(pendingIntent)
                .SetPriority((int)NotificationPriority.Low)
                .SetOngoing(true);

            return notificationBuilder.Build();
        }
    }
}