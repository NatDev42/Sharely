using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharely.Activities
{
    [Activity(Label = "ContactActivity")]
    public class ContactActivity : Activity
    {
        Button dialBtn, smsBtn, emailBtn;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.contact_layout);

            dialBtn = FindViewById<Button>(Resource.Id.dialBtn);
            smsBtn = FindViewById<Button>(Resource.Id.smsBtn);
            emailBtn = FindViewById<Button>(Resource.Id.emailBtn);

            dialBtn.Click += DialBtn_Click;
            smsBtn.Click += SmsBtn_Click;
            emailBtn.Click += EmailBtn_Click;

        }

        private void EmailBtn_Click(object sender, EventArgs e)
        {
            string[] emails = { "shahar9070@gmail.com" };

            Intent intent = new Intent(Intent.ActionSend);
            intent.SetType("text/plain");
            intent.PutExtra(Intent.ExtraEmail, emails);
            intent.PutExtra(Intent.ExtraSubject, "Greetings");
            intent.PutExtra(Intent.ExtraText, "sup bro");

            StartActivity(Intent.CreateChooser(intent, "Send Email"));
        }

        private void SmsBtn_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("sms:0545353712"));
            intent.PutExtra("sms_body", "whats up bro");
            StartActivity(intent);
        }

        private void DialBtn_Click(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("tel:0545353712");
            var intent = new Intent(Intent.ActionDial, uri);
            StartActivity(intent);
        }
    }
}