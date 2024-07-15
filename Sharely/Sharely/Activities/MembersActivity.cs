using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharely.Classes;

namespace Sharely.Activities
{
    [Activity(Label = "MembersActivity")]
    public class MembersActivity : Activity
    {
        private RecyclerView recyclerView;
        private MemberAdapter adapter;
        private List<Connection> connections;
        private ConnectionListener connectionListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialize RecyclerView
            SetContentView(Resource.Layout.members_layout);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            //Get Connections
            connectionListener = new ConnectionListener();
            connectionListener.OnConnectionRetrieved += ConnectionListener_OnConnectionRetrieved;
        }

        private void ConnectionListener_OnConnectionRetrieved(object sender, ConnectionListener.ConnectionEventArgs e)
        {
            connections = e.connectionDB.Where(connection => connection.groupId == Intent.GetStringExtra("groupId")).ToList();
            // Initialize and set adapter
            adapter = new MemberAdapter(this, connections, Intent.GetBooleanExtra("isOwner", false));
            recyclerView.SetAdapter(adapter);
        }

        // Method to get list of connections

    }
}