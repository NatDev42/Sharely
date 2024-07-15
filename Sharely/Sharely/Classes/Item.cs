using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using Firebase.Storage;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharely
{
    public class Item
    {
        public string Id;
        public string name;
        public string description;
        public string category;
        public string groupId;
        public string imageUrl;
        public static byte[] fileBytes { get; set; }
        FirebaseFirestore db = AppDataHelper.GetFirestore();
        public Context context;
        public const string ITEM_COLLECTION_NAME = "items";
        public Item()
        {
        }
        public Item(Context context, string name, string description, string category, string groupId, byte[] _fileBytes)
        {
            this.context = context;
            this.name = name;
            this.description = description;
            this.category = category;
            this.groupId = groupId;
            this.imageUrl = imageUrl;
            fileBytes = _fileBytes;
        }
        private bool CheckIfFieldIsEmpty(string field, string fieldName)
        {
            if (string.IsNullOrEmpty(field))
            {
                Toast.MakeText(context, $"{fieldName} is required", ToastLength.Short).Show();
                return true;
            }
            return false;
        }
        private bool ValidateFields()
        {
            if (CheckIfFieldIsEmpty(this.name, "Name")) return false;
            if (CheckIfFieldIsEmpty(this.description, "Description")) return false;
            if (CheckIfFieldIsEmpty(this.category, "Category")) return false;
            if (CheckIfFieldIsEmpty(this.groupId, "Group")) return false;
            return true;
        }

        public async Task<bool> AddItem()
        {
            if (!ValidateFields())
                return false;
            if (fileBytes == null)
            {
                Toast.MakeText(context, "Image is required", ToastLength.Short).Show();
                return false;
            }
            try
            {
                HashMap itemMap = new HashMap();
                DocumentReference itemReference = db.Collection(ITEM_COLLECTION_NAME).Document();
                itemMap.Put("Id", itemReference.Id);
                itemMap.Put("name", this.name);
                itemMap.Put("description", this.description);
                itemMap.Put("category", this.category);
                itemMap.Put("groupId", this.groupId);
                StorageReference storageReference = null;
                FirebaseStorage storage = AppDataHelper.GetFirebaseStorage();
                try
                {
                    storageReference = storage.GetReference("itemImages").Child(itemReference.Id + "/" + GenerateRandomString(2));
                    await storageReference.PutBytes(fileBytes);
                    if (storageReference != null)
                    {
                        imageUrl = (await storageReference.DownloadUrl).ToString();
                        itemMap.Put("imageUrl", imageUrl);
                        await itemReference.Set(itemMap);
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    Toast.MakeText(context, s, ToastLength.Short).Show();
                }
            }

            catch (Exception ex)
            {
                string s = ex.Message;

                Toast.MakeText(context, s, ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        public async Task<bool> DeleteItem()
        {
            try
            {
                DocumentReference itemReference = db.Collection(ITEM_COLLECTION_NAME).Document(this.Id);
                await itemReference.Delete();
                FirebaseStorage storage = AppDataHelper.GetFirebaseStorage();
                StorageReference storageReference = storage.GetReferenceFromUrl(this.imageUrl);
                await storageReference.Delete();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                Toast.MakeText(context, s, ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        public async Task<bool> UpdateItem()
        {
            if (!ValidateFields())
                return false;
            try
            {
                DocumentReference itemReference = db.Collection(ITEM_COLLECTION_NAME).Document(this.Id);
                // Convert reference to snapshot
                DocumentSnapshot snapshot = (DocumentSnapshot)await itemReference.Get();
                StorageReference storageReference = null;
                FirebaseStorage storage = AppDataHelper.GetFirebaseStorage();
                if (fileBytes != null)
                {
                    storageReference = storage.GetReference("itemImages").Child(this.Id + "/" + GenerateRandomString(2));
                    await storageReference.PutBytes(fileBytes);
                    if (storageReference != null)
                        imageUrl = (await storageReference.DownloadUrl).ToString();
                }
                else
                {
                    imageUrl = snapshot.GetString("imageUrl");
                }

                // Use Dictionary instead of HashMap
                IDictionary<string, Java.Lang.Object> itemMap = new Dictionary<string, Java.Lang.Object>
        {
            { "name", new Java.Lang.String(this.name) },
            { "description", new Java.Lang.String(this.description) },
            { "category", new Java.Lang.String(this.category) },
            { "groupId", new Java.Lang.String(this.groupId) },
            { "imageUrl", new Java.Lang.String(imageUrl) }
        };

                await itemReference.Update(itemMap);
            }
            catch (Exception ex)
            {
                string s = ex.Message;

                Toast.MakeText(context, s, ToastLength.Short).Show();
                return false;
            }
            return true;
        }

        private string GenerateRandomString(int length)
        {
            System.Random random = new System.Random();
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length)
                             .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public byte[] GetFileBytes()
        {
            return fileBytes;
        }
    }
}