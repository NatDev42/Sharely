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
    public class GroupEventListener : Java.Lang.Object, IEventListener
    {
        public event EventHandler<GroupEventArgs> OnGroupRetrieved; // contains the event details
        List<Group> groups;

        public class GroupEventArgs : EventArgs
        {
            internal List<Group> groupDB { get; set; }
        }

        // set it to listen to the group collection
        public GroupEventListener()
        {
            AppDataHelper.GetFirestore().Collection(Group.GROUP_COLLECTION_NAME).AddSnapshotListener(this);
        }

        public void OnEvent(Java.Lang.Object value, FirebaseFirestoreException error)
        {
            QuerySnapshot snapshot = (QuerySnapshot)value;
            groups = new List<Group>();
            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                Group group = new Group();
                if (documentSnapshot != null)
                {
                    group.name = documentSnapshot.GetString("name") ?? "";
                    group.color = documentSnapshot.GetString("color") ?? "";
                    group.Id = documentSnapshot.GetString("Id") ?? "";
                    groups.Add(group);
                }
            }
            if (OnGroupRetrieved != null)
            {
                GroupEventArgs e = new GroupEventArgs();
                e.groupDB = groups;
                OnGroupRetrieved.Invoke(this, e);
            }
        }
        public void RetrieveGroups()
        {
            //trigger the event
            if (OnGroupRetrieved != null)
            {
                GroupEventArgs e = new GroupEventArgs();
                e.groupDB = groups;
                OnGroupRetrieved.Invoke(this, e);
            }
        }
    }
}