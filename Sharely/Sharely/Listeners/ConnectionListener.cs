using Android.App;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using Sharely.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sharely
{
    public class ConnectionListener : Java.Lang.Object, IEventListener
    {
        public event EventHandler<ConnectionEventArgs> OnConnectionRetrieved; // contains the event details
        List<Connection> connections;

        public class ConnectionEventArgs : EventArgs
        {
            internal List<Connection> connectionDB { get; set; }
        }

        // set it to listen to the group collection
        public ConnectionListener()
        {
            AppDataHelper.GetFirestore().Collection(Connection.CONNECTION_COLLECTION_NAME).AddSnapshotListener(this);
        }

        public void OnEvent(Java.Lang.Object value, FirebaseFirestoreException error)
        {
            QuerySnapshot snapshot = (QuerySnapshot)value;
            connections = new List<Connection>();
            foreach(DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                if (documentSnapshot != null)
                {
                    Connection connection = new Connection();
                        connection.userEmail = documentSnapshot.GetString("userEmail") ?? "";
                        connection.groupId = documentSnapshot.GetString("groupId") ?? "";
                    if (documentSnapshot.Get("isOwner") != null)
                        connection.isOwner = (bool)documentSnapshot.GetBoolean("isOwner");
                    else
                        connection.isOwner = false;
                    connections.Add(connection);
                }
            }
            if(OnConnectionRetrieved != null)
            {
                ConnectionEventArgs e = new ConnectionEventArgs();
                e.connectionDB = connections;
                OnConnectionRetrieved.Invoke(this, e);
            }
        }
    }
}