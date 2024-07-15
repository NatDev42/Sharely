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
using System.Text;

namespace Sharely
{
    internal class UserEventListener : Java.Lang.Object, IEventListener
    {
        public event EventHandler<UserEventArgs> OnUserRetrieved; // contains the event details
        List<User> users;

        public class UserEventArgs : EventArgs
        {
            internal List<User> userDB { get; set; }
        }

        // set it to listen to the group collection
        public UserEventListener()
        {
            AppDataHelper.GetFirestore().Collection(User.COLLECTION_NAME).AddSnapshotListener(this);
        }

        public void OnEvent(Java.Lang.Object value, FirebaseFirestoreException error)
        {
            QuerySnapshot snapshot = (QuerySnapshot)value;
            users = new List<User>();
            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                User user = new User();
                if (documentSnapshot != null)
                {
                    user.fullname = documentSnapshot.GetString("fullname") ?? "";
                    user.email = documentSnapshot.GetString("email") ?? "";
                    user.password = documentSnapshot.GetString("password") ?? "";
                    users.Add(user);
                }
            }
            if (OnUserRetrieved != null)
            {
                UserEventArgs e = new UserEventArgs();
                e.userDB = users;
                OnUserRetrieved.Invoke(this, e);
            }
        }
    }
}