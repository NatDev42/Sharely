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
using Sharely.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharely
{
    class GroupAdapter : BaseAdapter
    {

        Context context;
        public List<Group> groups;

        public GroupAdapter(Context context, List<Group> groups)
        {
            this.context = context;
            this.groups = groups;
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
            layoutInflater = ((MainActivity)context).LayoutInflater;
            View view = layoutInflater.Inflate(Resource.Layout.group_button_layout, parent, false);
            TextView groupbtntxt = view.FindViewById<TextView>(Resource.Id.groupbtntxt);
            ImageButton groupbtnBackground = view.FindViewById<ImageButton>(Resource.Id.groupBtnBackground);
            groupbtntxt.Text = groups[position].name;
            groupbtntxt.Tag = groups[position].Id;
            groupbtntxt.Click += EnterGroup;
            // Inflate the drawable resource
            Drawable drawable = ContextCompat.GetDrawable(context, Resource.Drawable.roundcorner);

            // Check if the drawable is a GradientDrawable
            if (drawable is GradientDrawable gradientDrawable)
            {
                // Change the color of the gradient drawable
                Color newColor = Color.ParseColor(groups[position].color); // Replace with your desired color
                gradientDrawable.SetColor(newColor);

                // Set the modified drawable as the background of the ImageButton
                groupbtnBackground.Background = gradientDrawable;
            }
            else
            {
                // Handle other drawable types if necessary
                Toast.MakeText(context, "Drawable is not a GradientDrawable", ToastLength.Short).Show();
            }
            return view;
        }

        private void EnterGroup(object sender, EventArgs e)
        {
            TextView groupbtntxt = (TextView)sender;
            Intent intent = new Intent(context, typeof(GroupActivity));
            intent.PutExtra("groupId", groupbtntxt.Tag.ToString());
            context.StartActivity(intent);
        }
        public override int Count
        {
            get
            {
                return groups.Count();
            }
        }
    }

    class GroupAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }
}