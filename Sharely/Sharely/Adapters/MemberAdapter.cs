using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Sharely;
using Sharely.Adapters;
using Sharely.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MemberAdapter : RecyclerView.Adapter
{
    Context context;
    private List<Connection> connections;
    bool viewerIsOwner;

    public MemberAdapter(Context context, List<Connection> connections, bool viewerIsOwner)
    {
        this.context = context;
        this.connections = connections;
        this.viewerIsOwner = viewerIsOwner;
    }
    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.member_view_layout, parent, false);
        MemberViewHolder viewHolder = new MemberViewHolder(itemView);
        return viewHolder;
    }

    public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        MemberViewHolder viewHolder = holder as MemberViewHolder;
        Connection connection = connections[position];

        // Set user's full name
        // Assuming you have a method to get the user's full name based on the userEmail
        string fullName = await User.GetUserNameByEmail(connection.userEmail);
        if (viewerIsOwner)
            viewHolder.DeleteButton.Visibility = ViewStates.Visible;
        if (connection.isOwner)
            fullName += " (Owner)";
        viewHolder.FullNameTextView.Text = fullName;
        if (viewerIsOwner && !connection.isOwner)
            viewHolder.DeleteButton.Visibility = ViewStates.Visible;

        // Set delete button click listener
        if (!connection.isOwner)
            viewHolder.DeleteButton.Click += (sender, e) =>
            {
                // Call a method to delete the connection
                DeleteConnection(connection);
            };
    }

    public override int ItemCount => connections.Count;

    // Method to get user's full name based on email

    // Method to delete connection
    private async void DeleteConnection(Connection connection)
    {
        if (await connection.Disconnect())
        {
            // Connection deleted successfully
            // Notify the adapter that the data set has changed
            NotifyDataSetChanged();
        }
        else
        {
            // Error occurred while deleting connection
            // Show an error message
            Toast.MakeText(context, "Error deleting connection", ToastLength.Short).Show();
        }
    }
}