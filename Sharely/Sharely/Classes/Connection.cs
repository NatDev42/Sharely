using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharely.Classes
{
    public class Connection
    {
        public string groupId;
        public string userEmail;
        public bool isOwner;
        public const string CONNECTION_COLLECTION_NAME = "connections";
        FirebaseFirestore db = AppDataHelper.GetFirestore();
        public Context context;

        public Connection(Context context, string groupId, string userId, bool isOwner)
        {
            this.context = context;
            this.groupId = groupId;
            this.userEmail = userId;
            this.isOwner = isOwner;
        }
        public Connection() { }
        public async Task<bool> AddConnection()
        {
            try
            {
                if (await BaseConnectionExists())
                    throw new Exception("You are already a member of this group");
                HashMap connectionMap = new HashMap();
                connectionMap.Put("groupId", groupId);
                connectionMap.Put("userEmail", userEmail);
                connectionMap.Put("isOwner", isOwner);
                DocumentReference connectionReference = db.Collection(CONNECTION_COLLECTION_NAME).Document();
                await connectionReference.Set(connectionMap);
                return true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
            }
            return false;
        }
        public async Task<bool> Disconnect()
        {
            try
            {
                QuerySnapshot query = (QuerySnapshot)await db.Collection(CONNECTION_COLLECTION_NAME).WhereEqualTo("groupId", groupId).Get();
                foreach (DocumentSnapshot document in query.Documents)
                    if ((document.Get("userEmail").ToString() ?? "") == userEmail)
                    {
                        DocumentReference connectionReference = db.Collection(CONNECTION_COLLECTION_NAME).Document(document.Id);
                        await connectionReference.Delete();
                        return true;
                    }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return false;
        }
        public async Task<bool> BaseConnectionExists()
        {
            try
            {
                QuerySnapshot query = (QuerySnapshot)await db.Collection(CONNECTION_COLLECTION_NAME).WhereEqualTo("groupId", groupId).Get();

                foreach (DocumentSnapshot document in query.Documents)
                    if ((document.Get("userEmail").ToString() ?? "") == userEmail)
                        return true;
                return false;
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
            }
            return false;
        }
        public async Task<bool>OwnerConnectionExists()
        {
            try
            {
                QuerySnapshot query = (QuerySnapshot)await db.Collection(CONNECTION_COLLECTION_NAME).WhereEqualTo("groupId", groupId).Get();

                foreach (DocumentSnapshot document in query.Documents)
                    if ((document.Get("userEmail").ToString() ?? "") == userEmail && (bool)document.Get("isOwner") == true)
                        return true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
            }
            return false;
        }
    }
}