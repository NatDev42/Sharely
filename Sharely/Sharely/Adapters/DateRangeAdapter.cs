using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharely
{
    internal class DateRangeAdapter : BaseAdapter
    {

        Context context;
        List<DateRange> ranges;
        bool userIsOwner;

        public DateRangeAdapter(Context context, List<DateRange> ranges, bool userIsOwner)
        {
            this.context = context;
            this.ranges = ranges;
            this.userIsOwner = userIsOwner;
        }


        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater layoutInflater;
            layoutInflater = ((ItemActivity)context).LayoutInflater;
            View view = layoutInflater.Inflate(Resource.Layout.dateRange_layout, parent, false);
            TextView dateRangeTxt = view.FindViewById<TextView>(Resource.Id.dateRangeTV);
            dateRangeTxt.Text = ranges[position].startDate.Date.ToString("dd/MM/yyyy") + " - " + ranges[position].endDate.Date.ToString("dd/MM/yyyy") + ": " + ranges[position].eventName;
            ImageButton deleteBtn = view.FindViewById<ImageButton>(Resource.Id.deleteBtn);
            if(!userIsOwner)
                deleteBtn.Visibility = ViewStates.Gone;
            deleteBtn.Tag = position;
            deleteBtn.Click += DeleteBtn_Click;
            return view;
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle("Delete date range");
            builder.SetMessage("Are you sure you want to delete this date range?");
            builder.SetPositiveButton("Yes", (senderAlert, args) => { DeleteDateRange(sender); });
            builder.SetNegativeButton("No", (senderAlert, args) => {  });
            Dialog dialog = builder.Create();
            dialog.Show();
        }
        private async void DeleteDateRange(object sender)
        {
            ImageButton deleteBtn = (ImageButton)sender;
            int position = (int)deleteBtn.Tag;

            DateRange dateRange = ranges[position];
            if (await dateRange.RemoveDateRange())
            {
                ranges.RemoveAt(position);
                NotifyDataSetChanged();
            }
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return ranges.Count;
            }
        }

    }

    internal class DateRangeAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }
}