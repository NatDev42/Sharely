using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Sharely;
using Sharely.Activities;
using Sharely.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharely.Classes
{
    [Service(Label = "BackgroundMusicService")]
    public class BackgroundMusicService : Service
    {
        IBinder binder;
        MediaPlayer mediaPlayer;
        public override void OnCreate()
        {
            base.OnCreate();

            // Create the notification channel
            NotificationHelper.CreateNotificationChannel(this);

            // Create a notification
            var notification = NotificationHelper.CreateNotification(this);

            // Start the service in the foreground with the notification
            StartForeground(1, notification);

            // Initialize and start the media player
            mediaPlayer = MediaPlayer.Create(this, Resource.Raw.TheChamps_Tequila);
            mediaPlayer.Looping = true; // Loop the music
        }
        public override IBinder OnBind(Intent intent)
        {
            binder = new BackgroundMusicServiceBinder(this);
            return binder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            mediaPlayer.Start();
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            mediaPlayer.Stop();
            mediaPlayer.Release();
        }
    }


    public class BackgroundMusicServiceBinder : Binder
    {
        readonly BackgroundMusicService service;

        public BackgroundMusicServiceBinder(BackgroundMusicService service)
        {
            this.service = service;
        }

        public BackgroundMusicService GetMusicService()
        {
            return service;
        }
    }
}