using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharely
{
    internal class ItemAdapter : BaseAdapter
    {
        public List<Item> items;
        Context context;
        string groupId;
        bool isOwner;
        public ItemAdapter(Context context, List<Item> items, bool isOwner, string groupId)
        {
            this.context = context;
            this.items = items;
            this.isOwner = isOwner;
            this.groupId = groupId;
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
            LayoutInflater layoutInflater = ((GroupActivity)context).LayoutInflater;
            TextView itemName;
            View view = layoutInflater.Inflate(Resource.Layout.item_button_layout, parent, false);
            ImageView itemImg = view.FindViewById<ImageView>(Resource.Id.itemImg);
            ImageService.Instance.LoadUrl(items[position].imageUrl).Retry(3, 200).DownSample(400, 400).Into(itemImg);
            ImageButton enterItemBtn = view.FindViewById<ImageButton>(Resource.Id.itemBtn);
            enterItemBtn.Tag = items[position].Id;
            enterItemBtn.Click += EnterItemClick;
            itemName = view.FindViewById<TextView>(Resource.Id.itemTxt);
            itemName.Text = items[position].name;

            return view;
        }

        private void EnterItemClick(object sender, EventArgs e)
        {
            ImageButton enterItemBtn = (ImageButton)sender;
            Intent intent = new Intent(context, typeof(ItemActivity));
            intent.PutExtra("itemId", enterItemBtn.Tag.ToString());
            intent.PutExtra("groupId", groupId);
            intent.PutExtra("isOwner", isOwner);
            context.StartActivity(intent);
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

    }

    internal class itemAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }
}