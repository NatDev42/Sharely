using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using FFImageLoading;
using Org.Apache.Commons.Logging;
using Org.Apache.Http.Conn;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using static Android.Icu.Text.Transliterator;

namespace Sharely
{
    [Activity(Label = "ItemActivity")]
    public class ItemActivity : Activity
    {
        Item item;
        List<DateRange> ranges;
        Dialog d;
        TextView startDate, endDate, itemName, itemDescription, itemCategory;
        EditText eventName;
        ListView dateRangeListView;
        ImageView itemImg;
        Button createEventBtn;
        const string dateFormat = "dd-MM-yyyy:hh";
        DateTime dateTime;
        DateTime startDateSelected, endDateSelected;
        DateRangeAdapter dateRangeAdapter;
        string userFullName;
        ISharedPreferences sp;
        ItemEventListener itemEventListener;
        Color color;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.item_info_layout);
            sp = GetSharedPreferences(User.CURRENT_USER_FILE, FileCreationMode.Private);
            itemEventListener = new ItemEventListener();
            DateRangeListener dateRangeListener = new DateRangeListener();
            UserEventListener userEventListener = new UserEventListener();
            GroupEventListener groupEventListener = new GroupEventListener();
            itemEventListener.OnItemRetrieved += ItemEventListener_OnItemRetrieved;
            dateRangeListener.OnDateRangeRetrieved += DateRangeListener_OnDateRangeRetrieved;
            userEventListener.OnUserRetrieved += UserEventListener_OnUserRetrieved;
            groupEventListener.OnGroupRetrieved += GroupEventListener_OnGroupRetrieved;
            Button addDateRange = FindViewById<Button>(Resource.Id.addDateRangeBtn);
            itemName = FindViewById<TextView>(Resource.Id.itemName);
            itemDescription = FindViewById<TextView>(Resource.Id.itemDescription);
            itemCategory = FindViewById<TextView>(Resource.Id.itemCategory);
            itemImg = FindViewById<ImageView>(Resource.Id.itemImg);
            ImageButton optionsBtn = FindViewById<ImageButton>(Resource.Id.options_btn);
            if (Intent.GetBooleanExtra("isOwner", false))
            {
                optionsBtn.Click += OptionsBtn_Click;
                addDateRange.Text = "Apply";
            }
            else
                optionsBtn.Visibility = ViewStates.Gone;
            addDateRange.Click += AddDateRange_Click;
            dateRangeListView = FindViewById<ListView>(Resource.Id.dateRangeLV);
        }

        private void GroupEventListener_OnGroupRetrieved(object sender, GroupEventListener.GroupEventArgs e)
        {
            Color newColor = Color.ParseColor(e.groupDB.Where(g => g.Id == item.groupId).FirstOrDefault().color);
            Drawable drawable = ContextCompat.GetDrawable(this, Resource.Drawable.roundcorner);

            // Check if the drawable is a GradientDrawable
            if (drawable is GradientDrawable gradientDrawable)
            {
                // Change the color of the gradient drawable
                gradientDrawable.SetColor(newColor);

                // Set the modified drawable as the background of the ImageButton
                itemCategory.Background = gradientDrawable;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            itemEventListener.RetrieveItems();
        }
        private void OptionsBtn_Click(object sender, EventArgs e)
        {
            PopupMenu popup = new PopupMenu(this, (ImageButton)sender);
            popup.MenuInflater.Inflate(Resource.Menu.owner_menu, popup.Menu);

            popup.MenuItemClick += (sender, args) =>
            {
                // Get the ID of the clicked menu item
                int menuItemId = args.Item.ItemId;

                // Perform action based on the ID of the clicked menu item
                switch (menuItemId)
                {
                    case Resource.Id.delete:
                        DeleteItemAction();
                        break;
                    case Resource.Id.editItem:
                        UpdateItemAction();
                        break;
                }
            };

            popup.Show();
        }
        private void UpdateItemAction()
        {
            Intent intent = new Intent(this, typeof(UpdateItemActivity));
            intent.PutExtra("itemId", item.Id);
            intent.PutExtra("groupId", Intent.GetStringExtra("groupId"));
            StartActivity(intent);
        }
        private async void DeleteItemAction()
        {
            if (await item.DeleteItem())
                Toast.MakeText(this, "Item deleted", ToastLength.Short).Show();
            Finish();
        }
        private void UserEventListener_OnUserRetrieved(object sender, UserEventListener.UserEventArgs e)
        {
            userFullName = e.userDB.Where(user => user.email == sp.GetString("email", "")).FirstOrDefault().fullname;
        }

        private void DateRangeListener_OnDateRangeRetrieved(object sender, DateRangeListener.DateRangeArgs e)
        {
            ranges = e.dateRangeDB.Where(range => range.itemId == Intent.GetStringExtra("itemId")).ToList();
            dateRangeAdapter = new DateRangeAdapter(this, ranges, Intent.GetBooleanExtra("isOwner", false));
            dateRangeListView.Adapter = dateRangeAdapter;
        }

        private void ItemEventListener_OnItemRetrieved(object sender, ItemEventListener.ItemEventArgs e)
        {
            if (e.itemDB == null)
                return;
            item = e.itemDB.Where(item => item.Id == Intent.GetStringExtra("itemId")).FirstOrDefault();
            if (item == null)
                return;
            itemName.Text = item.name;
            itemDescription.Text = item.description;
            itemCategory.Text = item.category;
            ImageService.Instance.LoadUrl(item.imageUrl).Retry(3, 200).DownSample(400, 400).Into(itemImg);
        }


        private void AddDateRange_Click(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.add_dateRange_layout);
            d.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
            d.Show();
            eventName = d.FindViewById<EditText>(Resource.Id.editTextEventName);
            startDate = d.FindViewById<TextView>(Resource.Id.textViewSelectedStartDate);
            endDate = d.FindViewById<TextView>(Resource.Id.textViewSelectedEndDate);
            createEventBtn = d.FindViewById<Button>(Resource.Id.buttonCreateEvent);
            if (!Intent.GetBooleanExtra("isOwner", false))
            {
                eventName.Text = userFullName;
                eventName.Enabled = false;
            }
            startDate.Click += StartDate_Click;
            endDate.Click += EndDate_Click;
            createEventBtn.Click += CreateEventBtn_Click;
        }

        private async void CreateEventBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(eventName.Text))
            {
                Toast.MakeText(d.Context, "Please enter an event name", ToastLength.Short).Show();
                return;
            }
            if (startDateSelected == null || endDateSelected == null)
            {
                Toast.MakeText(d.Context, "Please select a start and end date", ToastLength.Short).Show();
                return;
            }
            //make sure date doesn't overlap another range
            foreach (DateRange range in ranges)
            {
                if (startDateSelected > range.startDate && startDateSelected < range.endDate)
                {
                    Toast.MakeText(d.Context, "Event overlaps with another range", ToastLength.Short).Show();
                    return;
                }
                if (endDateSelected > range.startDate && endDateSelected < range.endDate)
                {
                    Toast.MakeText(d.Context, "Event overlaps with another range", ToastLength.Short).Show();
                    return;
                }
            }
            DateRange dateRange = new DateRange(Intent.GetStringExtra("itemId"), eventName.Text, startDateSelected, endDateSelected, this);
            if (await dateRange.AddDateRange(ranges))
            {
                Toast.MakeText(d.Context, "Event added", ToastLength.Short).Show();
                d.Dismiss();
                Finish();
            }
        }

        private void StartDate_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;

            DatePickerDialog datePickerDialog = new DatePickerDialog(this, (s, args) =>
            {

                // Ensure start date is not earlier than today
                if (args.Date.CompareTo(today.Date) < 0)
                    Toast.MakeText(this, "Start date cannot be earlier than today", ToastLength.Short).Show();
                else
                {
                    startDateSelected = args.Date;
                    startDate.Text = startDateSelected.ToString(dateFormat);
                }

            }, today.Year, today.Month - 1, today.Day);

            datePickerDialog.Show();
            if (endDateSelected < startDateSelected)
            {
                endDateSelected = startDateSelected;
                endDate.Text = endDateSelected.ToString(dateFormat);
            }
        }
        private void EndDate_Click(object sender, EventArgs e)
        {
            if (startDateSelected == null)
            {
                Toast.MakeText(this, "Please select a start date first", ToastLength.Short).Show();
                return;
            }
            DateTime today = DateTime.Now;

            DatePickerDialog datePickerDialog = new DatePickerDialog(this, (s, args) =>
            {
                // Ensure end date is not earlier than today or the start date
                if (args.Date == null)
                    Toast.MakeText(this, "End date cannot be empty", ToastLength.Short).Show();
                if (args.Date < startDateSelected)
                    Toast.MakeText(this, "End date cannot be earlier than start date", ToastLength.Short).Show();
                else
                {
                    endDateSelected = args.Date;
                    endDate.Text = endDateSelected.ToString(dateFormat);
                }
            }, today.Year, today.Month - 1, today.Day);

            datePickerDialog.Show();
        }

    }
}