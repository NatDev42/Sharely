using System.Threading.Tasks;
using System;
using Firebase.Auth;
using Firebase.Firestore;
using Sharely;
using Android.Gms.Extensions;
using Android.Content;
using Android.App;
using Firebase;
using Java.Util;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;

namespace Sharely
{
    internal class Group
    {
        public string name;
        public string color;
        public const string GROUP_COLLECTION_NAME = "groups";
        FirebaseFirestore db = AppDataHelper.GetFirestore();
        public Context context;
        public string Id;
        public Group() { }
        public Group(Context context, string name, string color)
        {
            this.name = name;
            this.context = context;
            this.color = color;
        }
        public async Task<bool> AddGroup()
        {
            try
            {
                HashMap groupMap = new HashMap();
                groupMap.Put("name", name);
                groupMap.Put("color", color);

                DocumentReference groupReference = db.Collection(GROUP_COLLECTION_NAME).Document();
                groupMap.Put("Id", groupReference.Id);

                this.Id = groupReference.Id;
                await groupReference.Set(groupMap);
            }
            catch (Exception ex)
            {
                string s = ex.Message;

                Toast.MakeText(context, s, ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        public async Task<bool> RemoveGroup()
        {
            try
            {
                QuerySnapshot query = (QuerySnapshot)await db.Collection(GROUP_COLLECTION_NAME).WhereEqualTo("Id", Id).Get();
                foreach (DocumentSnapshot document in query.Documents)
                    await document.Reference.Delete();
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        public async Task<bool> UpdateGroup()
        {
            try
            {
                HashMap groupMap = new HashMap();
                groupMap.Put("name", name);
                groupMap.Put("Id", Id);
                groupMap.Put("color", color);

                DocumentReference groupReference = db.Collection(GROUP_COLLECTION_NAME).Document(Id);
                await groupReference.Set(groupMap);
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        public async static Task<bool> Exists(string id)
        {
            FirebaseFirestore db = AppDataHelper.GetFirestore();
            try
            {
                DocumentReference docRef = db.Collection(GROUP_COLLECTION_NAME).Document(id);
                DocumentSnapshot snapshot = (DocumentSnapshot)await docRef.Get();

                return snapshot.Exists();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }
    }
}