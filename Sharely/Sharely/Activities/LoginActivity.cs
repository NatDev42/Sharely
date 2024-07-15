using Android.App;
using Android.Content;
using Android.Icu.Text;
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
    [Activity(Label = "LoginActivity", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        UserEventListener userEventListener = new UserEventListener();
        ISharedPreferences sp;
        ISharedPreferencesEditor editor;
        List<User> users;
        CheckBox saveDetails;
        ProgressDialog progressDialog;
        Button loginBtn;
        TextView registerBtn;
        EditText email;
        EditText password;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.login_layout);
            sp = GetSharedPreferences(User.CURRENT_USER_FILE, FileCreationMode.Private);
            editor = sp.Edit();

            // Create your application here
            userEventListener.OnUserRetrieved += UserEventListener_OnUserRetrieved;
            loginBtn = FindViewById<Button>(Resource.Id.login_button);
            loginBtn.Click += LoginBtn_Click;
            registerBtn = FindViewById<TextView>(Resource.Id.create_account_txt);
            registerBtn.Click += RegisterBtn_Click;
            saveDetails = FindViewById<CheckBox>(Resource.Id.save_login_details_checkbox);
            if (sp.GetBoolean("saveDetails", false))
            {
                email = FindViewById<EditText>(Resource.Id.email_et);
                password = FindViewById<EditText>(Resource.Id.password_et);
                email.Text = sp.GetString("email", "");
                password.Text = sp.GetString("password", "");
                saveDetails.Checked = true;
            }
        }

        private void UserEventListener_OnUserRetrieved(object sender, UserEventListener.UserEventArgs e)
        {
            users = e.userDB;
        }

        private void RegisterBtn_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(RegisterActivity));
            StartActivity(intent);
        }

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            email = FindViewById<EditText>(Resource.Id.email_et);
            password = FindViewById<EditText>(Resource.Id.password_et);
            User user = new User(this, email.Text, password.Text);
            ShowProgressDialog("Logging in...");
            if (await user.Login())
            {
                HideProgressDialog();
                Intent intent = new Intent(this, typeof(MainActivity));
                editor.PutBoolean("saveDetails", saveDetails.Checked);
                editor.Apply();
                StartActivity(intent);
            }
            else
            {
                HideProgressDialog();
                Toast.MakeText(this, "Wrong email or password", ToastLength.Short).Show();
            }
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
            if (progressDialog != null && progressDialog.IsShowing)
            {
                progressDialog.Dismiss();
                progressDialog = null;
            }
        }
        void SetViewsEnabled(bool isEnabled)
        {
            email.Enabled = isEnabled;
            password.Enabled = isEnabled;
            loginBtn.Enabled = isEnabled;
            registerBtn.Enabled = isEnabled;
        }

    }
}