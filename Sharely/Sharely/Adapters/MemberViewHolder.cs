using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Sharely.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharely.Adapters
{
    public class MemberViewHolder : RecyclerView.ViewHolder
    {
        public TextView FullNameTextView { get; }
        public ImageButton DeleteButton { get; }
        Context context;
        public MemberViewHolder(View itemView) : base(itemView)
        {
            FullNameTextView = itemView.FindViewById<TextView>(Resource.Id.textViewFullName);
            DeleteButton = itemView.FindViewById<ImageButton>(Resource.Id.deleteBtn);
        }
    }
}