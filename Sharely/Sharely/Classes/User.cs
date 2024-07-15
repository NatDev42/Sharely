using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Java.Util;
using Sharely;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharely
{
    public class User
    {
        public string fullname;
        public string email;
        public string password;
        Context context;


        FirebaseAuth firebaseAuth; // points to the authentication table
        FirebaseFirestore database; // points to the firestore table

        public const string COLLECTION_NAME = "users";
        public const string CURRENT_USER_FILE = "currentUserFile";

        public User(Context context, string fullname, string email, string password)
        {
            this.context = context;
            this.fullname = fullname;
            this.email = email;
            this.password = password;

            this.firebaseAuth = AppDataHelper.GetFirebaseAuthentication();
            this.database = AppDataHelper.GetFirestore();
        }

        public User(Context context, string email, string password)
        {
            this.context = context;
            this.email = email;
            this.password = password;

            this.firebaseAuth = AppDataHelper.GetFirebaseAuthentication();
            this.database = AppDataHelper.GetFirestore();
        }

        public User()
        {
            this.firebaseAuth = AppDataHelper.GetFirebaseAuthentication();
            this.database = AppDataHelper.GetFirestore();
        }

        public async Task<bool> Register()
        {
            try
            {
                await firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
            }
            catch (Exception ex)
            {
                return false;
            }
            try
            {
                StorageReference storageRef = null;

                if (database != null)
                {
                    email = email.ToLower();
                    HashMap userMap = new HashMap();
                    userMap.Put("email", email);
                    userMap.Put("password", password);
                    userMap.Put("fullname", fullname);

                    // sets all the attributes for this user
                    DocumentReference userRef = database.Collection(COLLECTION_NAME).Document(firebaseAuth.CurrentUser.Uid);
                    userMap.Put("Id", userRef.Id);

                    await userRef.Set(userMap);
                }

            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
                return false;
            }
            return await Login();
        }

        public async Task<bool> Login()
        {
            try
            {
                await firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                var editor = Application.Context.GetSharedPreferences(CURRENT_USER_FILE, FileCreationMode.Private).Edit();
                editor.PutString("email", email);
                editor.PutString("password", password);
                editor.Apply();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async Task<string> GetUserNameByEmail(string email)
        {
            FirebaseFirestore database = AppDataHelper.GetFirestore();
            try
            {
                QuerySnapshot query = (QuerySnapshot)await database.Collection(COLLECTION_NAME).WhereEqualTo("email", email).Get();
                DocumentSnapshot document = query.Documents[0];
                return (document.Get("fullname") ?? "").ToString();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return "";
            }
        }
    }
}