using Android.App;
using Android.Content;
using Android.Net.Wifi.P2p;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Sharely.Classes;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using Sharely.Activities;
using Javax.Sql;
using Android.Graphics;
using Android.Graphics.Drawables;
using static Android.Graphics.ColorSpace;

namespace Sharely
{
    [Activity(Label = "GroupActivity")]
    public class GroupActivity : Activity
    {
        ItemEventListener itemEventListener;
        GroupEventListener groupEventListener;
        List<string> categories;
        List<Item> allItems;
        List<Item> leftItems, rightItems;
        ListView LvLeft, LvRight;
        ItemAdapter itemAdapterLeft, itemAdapterRight;
        TextView tvGroupId;
        bool isOwner;
        ISharedPreferences sp;
        Dialog d;
        View[] squares;
        readonly Color selectedBorderColor = Color.Blue;
        readonly Color defaultBorderColor = Color.Transparent;
        Color groupColor;
        TextView groupNametxt;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.group_layout);
            tvGroupId = FindViewById<TextView>(Resource.Id.groupIdTxt);
            tvGroupId.Text = "Id: " + Intent.GetStringExtra("groupId");
            tvGroupId.Click += TvGroupId_Click;
            groupNametxt = FindViewById<TextView>(Resource.Id.groupName);
            sp = GetSharedPreferences(User.CURRENT_USER_FILE, FileCreationMode.Private);
            Connection connection = new Connection(this, Intent.GetStringExtra("groupId"), sp.GetString("email", ""), true);
            isOwner = await connection.OwnerConnectionExists();
            itemEventListener = new ItemEventListener();
            groupEventListener = new GroupEventListener();
            //   itemEventListener.OnItemRetrieved += ItemEventListener_OnItemRetrieved;
            itemEventListener.OnItemRetrieved += ItemEventListener_OnItemRetrieved;
            groupEventListener.OnGroupRetrieved += GroupEventListener_OnGroupRetrieved;
            Button addItemBtn = FindViewById<Button>(Resource.Id.addItemBtn);
            ImageButton optionsBtn = FindViewById<ImageButton>(Resource.Id.options_btn);
            if (isOwner)
            {
                addItemBtn.Click += AddItemBtn_Click;
                optionsBtn.Click += OpenOwnerMenu;
            }
            else
            {
                addItemBtn.Visibility = ViewStates.Gone;
                optionsBtn.Click += OpenPeasantMenu;
            }
        }

        private void GroupEventListener_OnGroupRetrieved(object sender, GroupEventListener.GroupEventArgs e)
        {
            Group thisgroup = e.groupDB.Where(group => group.Id == Intent.GetStringExtra("groupId")).FirstOrDefault();
            groupColor = Color.ParseColor(thisgroup.color);
            groupNametxt.Text = e.groupDB.Where(group => group.Id == Intent.GetStringExtra("groupId")).FirstOrDefault().name;
            groupNametxt.SetTextColor(groupColor);
        }

        private void OpenOwnerMenu(object sender, EventArgs e)
        {
            PopupMenu popup = new PopupMenu(this, (ImageButton)sender);
            popup.MenuInflater.Inflate(Resource.Menu.group_menu_admin, popup.Menu);

            popup.MenuItemClick += (sender, args) =>
            {
                // Get the ID of the clicked menu item
                int menuItemId = args.Item.ItemId;

                // Perform action based on the ID of the clicked menu item
                switch (menuItemId)
                {
                    case Resource.Id.rename:
                        RenameGroupAction();
                        break;
                    case Resource.Id.deleteGroup:
                        DeleteGroupAction();
                        break;
                    case Resource.Id.manageMembers:
                        ManageMembersAction(true);
                        break;
                }
            };

            popup.Show();
        }
        private void OpenPeasantMenu(object sender, EventArgs e)
        {
            PopupMenu popup = new PopupMenu(this, (ImageButton)sender);
            popup.MenuInflater.Inflate(Resource.Menu.group_menu_user, popup.Menu);

            popup.MenuItemClick += (sender, args) =>
            {
                // Get the ID of the clicked menu item
                int menuItemId = args.Item.ItemId;

                // Perform action based on the ID of the clicked menu item
                switch (menuItemId)
                {
                    case Resource.Id.leaveGroup:
                        LeaveGroupAction();
                        break;
                    case
                        Resource.Id.viewMembers:
                        ManageMembersAction(false);
                        break;
                }
            };

            popup.Show();
        }
        private async void LeaveGroupAction()
        {
            Connection connection = new Connection(this, Intent.GetStringExtra("groupId"), sp.GetString("email", ""), false);
            if(await connection.Disconnect())
                Toast.MakeText(this, "You left the group", ToastLength.Short).Show();
            Finish();
        }
        private void RenameGroupAction()
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.create_group_layout);
            EditText groupName = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            Button editGroupBtn = d.FindViewById<Button>(Resource.Id.addGroupBtn);
            groupName.Text = groupNametxt.Text;

            groupName.Hint = "Enter new group name";
            editGroupBtn.Click += EditGroupBtn_Click;

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

            int[] colors = new int[]
            {
        Color.ParseColor("#FF5071").ToArgb(),
        Color.ParseColor("#F78C00").ToArgb(),
        Color.ParseColor("#FED401").ToArgb(),
        Color.ParseColor("#4AD351").ToArgb(),
        Color.ParseColor("#05ADD2").ToArgb(),
        Color.ParseColor("#1D73BC").ToArgb(),
        Color.ParseColor("#AF4BDF").ToArgb()
            };

            for (int i = 0; i < squares.Length; i++)
            {
                squares[i].Tag = colors[i];
                squares[i].SetBackgroundDrawable(GetDefaultDrawable(new Color(colors[i])));
                squares[i].Click += Square_Click;
            }

            int groupColorInARGB = groupColor.ToArgb();
            var selectedSquare = squares.FirstOrDefault(square => (int)square.Tag == groupColorInARGB);
            selectedSquare?.PerformClick();

            d.Show();
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

        private string GetSelectedColor()
        {
            // Find the clicked square and retrieve its color tag
            foreach (var square in squares)
            {
                if (square.Selected)
                {
                    int argb = (int)square.Tag;
                    Color color = new Color(argb);
                    return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                }
            }
            return ""; // Return default color if none selected
        }

        private void Square_Click(object sender, EventArgs e)
        {
            var clickedSquare = sender as View;

            foreach (var square in squares)
            {
                if (square == clickedSquare)
                {
                    // Set border to blue for selected square
                    square.SetBackgroundDrawable(GetSelectedDrawable(new Color((int)square.Tag)));
                    square.Selected = true;
                }
                else
                {
                    // Reset border for other squares
                    square.SetBackgroundDrawable(GetDefaultDrawable(new Color((int)square.Tag)));
                    square.Selected = false;
                }
            }
        }

        private async void EditGroupBtn_Click(object sender, EventArgs e)
        {
            EditText groupName = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            if (string.IsNullOrEmpty(groupName.Text))
            {
                Toast.MakeText(d.Context, "Please enter a name", ToastLength.Short).Show();
                return;
            }
            if (string.IsNullOrEmpty(GetSelectedColor()))
            {
                Toast.MakeText(d.Context, "Please select a color", ToastLength.Short).Show();
                return;
            }
            Group group = new Group(d.Context, groupName.Text, GetSelectedColor());
            group.Id = Intent.GetStringExtra("groupId");
            if (await group.UpdateGroup())
                Toast.MakeText(d.Context, "Group updated", ToastLength.Short).Show();
            d.Dismiss();
            Finish();
        }

        private void DeleteGroupAction()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete group");
            alert.SetMessage("Are you sure you want to delete this group?");
            alert.SetPositiveButton("Yes", async (senderAlert, args) =>
            {
                Group group = new Group(this, Intent.GetStringExtra("groupId"), "");
                if (await group.RemoveGroup())
                {
                    ItemEventListener itemDeleterListener = new ItemEventListener();
                    itemDeleterListener.OnItemRetrieved += ItemDeleterListener_OnItemRetrieved;
                    ConnectionListener connectionDeleterListener = new ConnectionListener();
                    connectionDeleterListener.OnConnectionRetrieved += ConnectionDeleterListener_OnConnectionRetrieved;
                    Toast.MakeText(this, "Group deleted", ToastLength.Short).Show();
                }
                Finish();
            });
            alert.SetNegativeButton("No", (senderAlert, args) => {});

            Dialog dialog = alert.Create();
            dialog.Show();
        }
        private void ManageMembersAction(bool isOwner)
        {
            Intent intent = new Intent(this, typeof(MembersActivity));
            intent.PutExtra("groupId", Intent.GetStringExtra("groupId"));
            intent.PutExtra("isOwner", isOwner);
            StartActivity(intent);
        }
        private async void ConnectionDeleterListener_OnConnectionRetrieved(object sender, ConnectionListener.ConnectionEventArgs e)
        {
            List<Connection> connections2Delete = e.connectionDB.Where(connection => connection.groupId == Intent.GetStringExtra("groupId")).ToList();
            foreach (Connection connection in connections2Delete)
                await connection.Disconnect();
        }

        private async void ItemDeleterListener_OnItemRetrieved(object sender, ItemEventListener.ItemEventArgs e)
        {
            List<Item> items2Delete = e.itemDB.Where(item => item.groupId == Intent.GetStringExtra("groupId")).ToList();
            foreach (Item item in items2Delete)
                await item.DeleteItem();
        }

        private void ItemEventListener_OnItemRetrieved(object sender, ItemEventListener.ItemEventArgs e)
        {
            //extract the group id from the intent
            allItems = e.itemDB.Where(item => item.groupId == Intent.GetStringExtra("groupId")).ToList();
            GridLayout gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(gridLayout.LayoutParameters);

            layoutParams.Height = (allItems.Count + 1) / 2 * 750;
            gridLayout.LayoutParameters = layoutParams;
            leftItems = new List<Item>();
            rightItems = new List<Item>();
            //place half of the items in the left listview and the other half in the right listview
            for (int i = 0; i < allItems.Count; i++)
                if (i % 2 == 0)
                    leftItems.Add(allItems[i]);
                else
                    rightItems.Add(allItems[i]);
            LvLeft = FindViewById<ListView>(Resource.Id.LvLeft);
            itemAdapterLeft = new ItemAdapter(this, leftItems, isOwner, Intent.GetStringExtra("groupId"));
            LvLeft.Adapter = itemAdapterLeft;
            LvRight = FindViewById<ListView>(Resource.Id.LvRight);
            itemAdapterRight = new ItemAdapter(this, rightItems, isOwner, Intent.GetStringExtra("groupId"));
            LvRight.Adapter = itemAdapterRight;
            Spinner categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            //Add to the spinner the category 'All' and all distinct categories from itemDB
            categories = allItems.Where(item => item.category != null).Select(item => item.category).Distinct().ToList();
            categories.Insert(0, "All");
            ArrayAdapter dataAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, categories);
            categorySpinner.Adapter = dataAdapter;
            categorySpinner.ItemSelected += CategorySpinner_ItemSelected;
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (itemAdapterLeft != null)
                itemAdapterLeft.NotifyDataSetChanged();
            if (itemAdapterRight != null)
                itemAdapterRight.NotifyDataSetChanged();
        }


        private void AddItemBtn_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(AddItemActivity));
            intent.PutExtra("groupId", Intent.GetStringExtra("groupId"));
            StartActivity(intent);
        }

        private void TvGroupId_Click(object sender, EventArgs e)
        {
            Clipboard.SetTextAsync(Intent.GetStringExtra("groupId"));
            Toast.MakeText(this, "Group id copied to clipboard", ToastLength.Short).Show();
        }

        private void CategorySpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            List<Item> SortedItemList = allItems;
            //when a category is selected, filter the items to show only the items of the selected category
            if (e.Position != 0)
                SortedItemList = allItems.Where(item => item.category == categories[e.Position]).ToList();
            leftItems = new List<Item>();
            rightItems = new List<Item>();
            for (int i = 0; i < SortedItemList.Count; i++)
                if (i % 2 == 0)
                    leftItems.Add(SortedItemList[i]);
                else
                    rightItems.Add(SortedItemList[i]);
            LvLeft = FindViewById<ListView>(Resource.Id.LvLeft);
            itemAdapterLeft = new ItemAdapter(this, leftItems, isOwner, Intent.GetStringExtra("groupId"));
            LvLeft.Adapter = itemAdapterLeft;

            LvRight = FindViewById<ListView>(Resource.Id.LvRight);
            itemAdapterRight = new ItemAdapter(this, rightItems, isOwner, Intent.GetStringExtra("groupId"));
            LvRight.Adapter = itemAdapterRight;

        }

    }
}