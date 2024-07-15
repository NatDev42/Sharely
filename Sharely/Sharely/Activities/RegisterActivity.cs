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
    [Activity(Label = "RegisterActivity")]
    public class RegisterActivity : Activity
    {
        UserEventListener userEventListener = new UserEventListener();
        List<User> users;
        ProgressDialog progressDialog;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.register_layout);
            Button registerBtn = FindViewById<Button>(Resource.Id.register_button);
            registerBtn.Click += RegisterBtn_Click;
            userEventListener.OnUserRetrieved += UserEventListener_OnUserRetrieved;
        }

        private void UserEventListener_OnUserRetrieved(object sender, UserEventListener.UserEventArgs e)
        {
            users = e.userDB;
        }

        private async void RegisterBtn_Click(object sender, EventArgs e)
        {
            EditText fullname = FindViewById<EditText>(Resource.Id.fullname_et);
            EditText email = FindViewById<EditText>(Resource.Id.email_et);
            EditText password = FindViewById<EditText>(Resource.Id.password_et);
            EditText confirmPassword = FindViewById<EditText>(Resource.Id.confirm_password_et);
            if (password.Text != confirmPassword.Text)
                Toast.MakeText(this, "Passwords do not match", ToastLength.Short).Show();
            else if (string.IsNullOrEmpty(fullname.Text))
                Toast.MakeText(this, "Full name is required", ToastLength.Short).Show();
            else if (string.IsNullOrEmpty(email.Text))
                Toast.MakeText(this, "Email is required", ToastLength.Short).Show();
            else if (string.IsNullOrEmpty(password.Text))
                Toast.MakeText(this, "Password is required", ToastLength.Short).Show();
            else if (!IsEmailValid(email.Text))
                Toast.MakeText(this, "Email is not valid", ToastLength.Short).Show();
            else if (password.Length() < 6)
                Toast.MakeText(this, "Password must be at least 6 characters", ToastLength.Short).Show();
            else
            {
                User user = new User(this, fullname.Text, email.Text, password.Text);
                if (users.Where(u => u.email == user.email).FirstOrDefault() != null)
                    Toast.MakeText(this, "Email already in use", ToastLength.Short).Show();
                else
                {
                    ShowProgressDialog("Registering...");
                    if (await user.Register())
                    {
                        HideProgressDialog();
                        Toast.MakeText(this, "Registered successfully", ToastLength.Short).Show();
                        Intent intent = new Intent(this, typeof(MainActivity));
                        StartActivity(intent);
                    }
                    else
                    {
                        HideProgressDialog();
                        Toast.MakeText(this, "Registration failed", ToastLength.Short).Show();
                    }
                }
            }
        }
        private bool IsEmailValid(string email)
        {
            return Android.Util.Patterns.EmailAddress.Matcher(email).Matches();
        }
        private void SetViewsEnabled(bool isEnabled)
        {
            FindViewById<EditText>(Resource.Id.fullname_et).Enabled = isEnabled;
            FindViewById<EditText>(Resource.Id.email_et).Enabled = isEnabled;
            FindViewById<EditText>(Resource.Id.password_et).Enabled = isEnabled;
            FindViewById<EditText>(Resource.Id.confirm_password_et).Enabled = isEnabled;
            FindViewById<Button>(Resource.Id.register_button).Enabled = isEnabled;
        }
        private void ShowProgressDialog(string message)
        {
            SetViewsEnabled(false);
            progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage(message);
            progressDialog.SetCancelable(false);
            progressDialog.Show();
        }
        private void HideProgressDialog()
        {
            SetViewsEnabled(true);
            progressDialog = new ProgressDialog(this);
            if (progressDialog != null && progressDialog.IsShowing)
            {
                progressDialog.Dismiss();
                progressDialog = null;
            }
        }
    }
}