using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sharely
{
    internal class ItemEventListener : Java.Lang.Object, IEventListener
    {
        public event EventHandler<ItemEventArgs> OnItemRetrieved; // contains the event details
        List<Item> items;

        public class ItemEventArgs : EventArgs
        {
            internal List<Item> itemDB { get; set; }
        }

        // set it to listen to the group collection
        public ItemEventListener()
        {
            AppDataHelper.GetFirestore().Collection(Item.ITEM_COLLECTION_NAME).AddSnapshotListener(this);
        }

        public void OnEvent(Java.Lang.Object value, FirebaseFirestoreException error)
        {
            QuerySnapshot snapshot = (QuerySnapshot)value;
            items = new List<Item>();
            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                Item item = new Item();
                if (documentSnapshot != null)
                {
                    item.Id = documentSnapshot.GetString("Id") ?? "";
                    item.name = documentSnapshot.GetString("name") ?? "";
                    item.description = documentSnapshot.GetString("description") ?? "";
                    item.category = documentSnapshot.GetString("category") ?? "";
                    item.groupId = documentSnapshot.GetString("groupId") ?? "";
                    item.imageUrl = documentSnapshot.GetString("imageUrl") ?? "";
                    this.items.Add(item);
                }
            }
            if (OnItemRetrieved != null)
            {
                ItemEventArgs e = new ItemEventArgs();
                e.itemDB = items;
                OnItemRetrieved.Invoke(this, e);
            }
        }
        public void RetrieveItems()
        {
            //trigger the event
            if (OnItemRetrieved != null)
            {
                ItemEventArgs e = new ItemEventArgs();
                e.itemDB = items;
                OnItemRetrieved.Invoke(this, e);
            }
        }
    }
}