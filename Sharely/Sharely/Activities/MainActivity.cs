using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views.InputMethods;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Sharely.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System;
using Sharely.Classes;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Sharely
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        private static readonly int ColorTagKey = 123456789; // Unique key for storing color in tags
        ISharedPreferences sp;
        List<string> usersGroupsIds;
        string email;
        Dialog d;
        List<Group> leftGroups, rightGroups;
        View[] squares;
        Color selectedBorderColor = Color.Blue;
        Color defaultBorderColor = Color.Transparent;
        ConnectionListener connectionListener;
        GroupEventListener groupEventListener;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main_layout);
            sp = GetSharedPreferences(User.CURRENT_USER_FILE, FileCreationMode.Private);
            email = sp.GetString("email", "");
            TextView userName = FindViewById<TextView>(Resource.Id.userNametxt);
            string fullname = await User.GetUserNameByEmail(email);
            userName.Text = fullname.Split(' ')[0];
            connectionListener = new ConnectionListener();
            groupEventListener = new GroupEventListener();
            connectionListener.OnConnectionRetrieved += ConnectionListener_OnConnectionRetrieved;
            groupEventListener.OnGroupRetrieved += GroupEventListener_OnGroupRetrieved;
            ImageButton options = FindViewById<ImageButton>(Resource.Id.options_btn);
            options.Click += Options_Click;
            Button createOrJoinBtn = FindViewById<Button>(Resource.Id.createOrJoinBtn);
            createOrJoinBtn.Click += CreateOrJoinGroup;

            BackgroundMusicService musicService = new BackgroundMusicService();
            Intent musicServiceIntent = new Intent(this, typeof(BackgroundMusicService));
            StartForegroundService(musicServiceIntent);
        }


        protected override void OnResume()
        {
            base.OnResume();
            if(groupEventListener != null)
                groupEventListener.RetrieveGroups();
        }

        private void ConnectionListener_OnConnectionRetrieved(object sender, ConnectionListener.ConnectionEventArgs e)
        {
            usersGroupsIds = e.connectionDB.Where(c => c.userEmail == email).Select(c => c.groupId).ToList();
        }

        private void GroupEventListener_OnGroupRetrieved(object sender, GroupEventListener.GroupEventArgs e)
        {
            if (usersGroupsIds == null)
                return;
            List<Group> usersGroups = e.groupDB.Where(g => usersGroupsIds.Contains(g.Id)).ToList();
            GridLayout gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(gridLayout.LayoutParameters);
            layoutParams.Height = (e.groupDB.Count + 1) / 2 * 460;
            gridLayout.LayoutParameters = layoutParams;
            //here needs to be a check for if user is in group
            leftGroups = new List<Group>();
            rightGroups = new List<Group>();
            //place half of the groups in the left listview and the other half in the right listview
            for (int i = 0; i < usersGroups.Count; i++)
                if (i % 2 == 0)
                    leftGroups.Add(usersGroups[i]);
                else
                    rightGroups.Add(usersGroups[i]);
            ListView LvLeft = FindViewById<ListView>(Resource.Id.LvLeft);
            GroupAdapter groupAdapterLeft = new GroupAdapter(this, leftGroups);
            LvLeft.Adapter = groupAdapterLeft;
            ListView LvRight = FindViewById<ListView>(Resource.Id.LvRight);
            GroupAdapter groupAdapterRight = new GroupAdapter(this, rightGroups);
            LvRight.Adapter = groupAdapterRight;
        }

        private void Square_Click(object sender, EventArgs e)
        {
            var clickedSquare = sender as View;

            foreach (var square in squares)
            {
                if (square == clickedSquare)
                {
                    // Set border to blue for selected square
                    square.SetBackgroundDrawable(GetSelectedDrawable(new Color((int)square.GetTag(ColorTagKey))));
                    square.Selected = true;
                }
                else
                {
                    // Reset border for other squares
                    square.SetBackgroundDrawable(GetDefaultDrawable(new Color((int)square.GetTag(ColorTagKey))));
                    square.Selected = false;
                }
            }
        }

        private Drawable GetSelectedDrawable(Color color)
        {
            GradientDrawable drawable = new GradientDrawable();
            drawable.SetColor(color);
            drawable.SetStroke(5, selectedBorderColor); // Border width and color
            return drawable;
        }

        private Drawable GetDefaultDrawable(Color color)
        {
            GradientDrawable drawable = new GradientDrawable();
            drawable.SetColor(color);
            drawable.SetStroke(5, defaultBorderColor); // Border width and color
            return drawable;
        }
        private void Options_Click(object sender, System.EventArgs e)
        {
            PopupMenu popup = new PopupMenu(this, (ImageButton)sender);
            popup.MenuInflater.Inflate(Resource.Menu.user_menu, popup.Menu);

            popup.MenuItemClick += (sender, args) =>
            {
                // Get the ID of the clicked menu item
                int menuItemId = args.Item.ItemId;

                // Perform action based on the ID of the clicked menu item
                switch (menuItemId)
                {
                    case Resource.Id.logout:
                        LogoutAction();
                        break;
                    case Resource.Id.contact:
                        Intent intent = new Intent(this, typeof(ContactActivity));
                        StartActivity(intent);
                        break;
                }
            };

            popup.Show();
        }
        private void LogoutAction()
        {
            ISharedPreferencesEditor editor = sp.Edit();
            editor.Remove("email");
            editor.Remove("password");
            editor.Remove("saveDetails");
            editor.Apply();
            Intent intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
            Finish();
        }
        private void CreateOrJoinGroup(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.create_or_join_layout);
            d.SetTitle("Add Or Join Group");
            d.SetCancelable(true);
            d.Show();
            Button createGroupBtn = d.FindViewById<Button>(Resource.Id.buttonCreateGroup);
            Button joinGroupBtn = d.FindViewById<Button>(Resource.Id.buttonJoinGroup);
            createGroupBtn.Click += CreateGroupBtn_Click;
            joinGroupBtn.Click += JoinGroupBtn_Click;
        }

        private void JoinGroupBtn_Click(object sender, EventArgs e)
        {
            d.Dismiss();
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.add_group_layout);
            d.SetTitle("Join Group");
            d.SetCancelable(true);
            d.Show();
            EditText joinGroupTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            Button joinGroupBtn = d.FindViewById<Button>(Resource.Id.addGroupBtn);
            joinGroupTxt.Hint = "Group Id";
            joinGroupBtn.Click += JoinGroupConfirm;
        }

        private async void JoinGroupConfirm(object sender, EventArgs e)
        {
            EditText groupNameTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            d.Dismiss();
            Connection connection = new Connection(this, groupNameTxt.Text, email, false);
            bool exists = await Group.Exists(groupNameTxt.Text);
            if (!exists)
            {
                Toast.MakeText(this, "Group does not exist", ToastLength.Short).Show();
                return;
            }
            if (await connection.AddConnection())
                Toast.MakeText(this, "Group joined", ToastLength.Short).Show();
            groupEventListener.RetrieveGroups();
        }

        private void CreateGroupBtn_Click(object sender, EventArgs e)
        {
            d.Dismiss();
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.create_group_layout);
            d.SetTitle("Add Group");
            d.SetCancelable(true);
            d.Show();
            EditText addgroupTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            Button addGroupBtn = d.FindViewById<Button>(Resource.Id.addGroupBtn);
            addGroupBtn.Click += AddGroupConfirm;
            addgroupTxt.Hint = "Enter group name";

            //set colors

            squares = new View[]
            {
                d.FindViewById<View>(Resource.Id.square1),
                d.FindViewById<View>(Resource.Id.square2),
                d.FindViewById<View>(Resource.Id.square3),
                d.FindViewById<View>(Resource.Id.square4),
                d.FindViewById<View>(Resource.Id.square5),
                d.FindViewById<View>(Resource.Id.square6),
                d.FindViewById<View>(Resource.Id.square7)
            };

            Color[] colors = new Color[]
            {
                Color.ParseColor("#FF5071"),
                Color.ParseColor("#F78C00"),
                Color.ParseColor("#FED401"),
                Color.ParseColor("#4AD351"),
                Color.ParseColor("#05ADD2"),
                Color.ParseColor("#1D73BC"),
                Color.ParseColor("#AF4BDF")
            };

            for (int i = 0; i < squares.Length; i++)
            {
                squares[i].SetTag(ColorTagKey, colors[i].ToArgb());
                squares[i].SetBackgroundDrawable(GetDefaultDrawable(colors[i]));
                squares[i].Click += Square_Click;
            }
        }


        private async void AddGroupConfirm(object sender, EventArgs e)
        {
            EditText groupNameTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            if (groupNameTxt.Text == "")
            {
                Toast.MakeText(this, "Group name cannot be empty", ToastLength.Short).Show();
                return;
            }
            if(GetSelectedColor() == "")
            {
                Toast.MakeText(this, "Please select a color", ToastLength.Short).Show();
                return;
            }
            Group group = new Group(this, groupNameTxt.Text, GetSelectedColor());
            
            if (await group.AddGroup())
            {
                Connection connection = new Connection(this, group.Id, email, true);
                if (await connection.AddConnection())
                    Toast.MakeText(this, "Group created", ToastLength.Short).Show();
                groupEventListener.RetrieveGroups();
            }
            d.Dismiss();
        }
        private string GetSelectedColor()
        {
            // Find the clicked square and retrieve its color tag
            foreach (var square in squares)
            {
                if (square.Selected)
                {
                    int colorArgb = (int)square.GetTag(ColorTagKey);
                    return "#" + colorArgb.ToString("X8").Substring(2); // Convert color ARGB to hex string
                }
            }
            return ""; // Return default color if none selected
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}