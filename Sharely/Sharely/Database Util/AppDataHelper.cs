using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sharely
{
    internal class AppDataHelper
    {
        static ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        static ISharedPreferencesEditor editor;
        static FirebaseFirestore database;
        static FirebaseStorage Fstorage;
        public static FirebaseFirestore GetFirestore()
        {
            if (database != null)
            {
                return database;
            }
            var app = FirebaseApp.InitializeApp(Application.Context);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetProjectId("sharely-2b0e1")
                    .SetApplicationId("sharely-2b0e1")
                    .SetApiKey("AIzaSyCX5yrcDkJhi8vUQXyq5Grj1WUAMMWYXGs")
                    .SetDatabaseUrl("https://Sharely.firebaseio.com")
                    .SetStorageBucket("sharely-2b0e1.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options, "FrostyScoops");
                //FirebaseApp.InitializeApp(context, options, "MarketList");
                database = FirebaseFirestore.GetInstance(app);
            }
            else
            {
                database = FirebaseFirestore.GetInstance(app);
            }
            return database;
        }
        public static FirebaseAuth GetFirebaseAuthentication()
        {
            FirebaseAuth firebaseAuthentication;
            var app = FirebaseApp.InitializeApp(Application.Context);
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetProjectId("sharely-2b0e1")
                    .SetApplicationId("sharely-2b0e1")
                    .SetApiKey("AIzaSyCX5yrcDkJhi8vUQXyq5Grj1WUAMMWYXGs")
                    .SetDatabaseUrl("https://Sharely.firebaseio.com")
                    .SetStorageBucket("sharely-2b0e1.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                firebaseAuthentication = FirebaseAuth.Instance;
            }
            else
            {
                firebaseAuthentication = FirebaseAuth.Instance;
            }
            return firebaseAuthentication;
        }
        public static FirebaseStorage GetFirebaseStorage()
        {
            if (Fstorage != null)
            {
                return Fstorage;
            }

            var app = FirebaseApp.InitializeApp(Application.Context);
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetProjectId("sharely-2b0e1")
                    .SetApplicationId("sharely-2b0e1")
                    .SetApiKey("AIzaSyCX5yrcDkJhi8vUQXyq5Grj1WUAMMWYXGs-ep8z4Sg")
                    .SetDatabaseUrl("https://FrostyScoops.firebaseio.com")
                    .SetStorageBucket("sharely-2b0e1.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                Fstorage = FirebaseStorage.GetInstance(app);
            }
            else
            { return Fstorage = FirebaseStorage.GetInstance(app); }
            return Fstorage;
        }
    }
}