using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using static Android.Content.ClipData;

namespace Sharely
{
    [Activity(Label = "AddItemActivity")]
    public class AddItemActivity : Activity
    {
        Dialog d;
        ItemEventListener ItemEventListener = new ItemEventListener();
        ImageView itemImage;
        EditText itemName, itemDescription;
        Spinner categorySpinner;
        List<Item> allItems;
        byte[] imagefileBytes;
        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.item_form_layout);
            itemImage = FindViewById<ImageView>(Resource.Id.itemImage);
            EditText itemName = FindViewById<EditText>(Resource.Id.itemName);
            EditText itemDescription = FindViewById<EditText>(Resource.Id.itemDescription);
            Spinner categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            TextView addItemBtn = FindViewById<TextView>(Resource.Id.addItemBtn);
            TextView addCategoryBtn = FindViewById<TextView>(Resource.Id.addCategoryBtn);
            itemImage.Click += SelectPhotoClick;
            addCategoryBtn.Click += AddCategoryClick;
            addItemBtn.Click += AddItemBtn_Click;
            ItemEventListener.OnItemRetrieved += ItemEventListener_OnItemRetrieved;
            RequestPermissions(permissionGroup, 0);
        }

        private void ItemEventListener_OnItemRetrieved(object sender, ItemEventListener.ItemEventArgs e)
        {
            allItems = e.itemDB.Where(item => item.groupId == Intent.GetStringExtra("groupId")).ToList();
            categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            //Add to the spinner the category 'All' and all distinct categories from itemDB
            List<string> categories = allItems.Where(item => item.category != null).Select(item => item.category).Distinct().ToList();
            ArrayAdapter dataAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, categories);
            dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            categorySpinner.Adapter = dataAdapter;
        }

        private async void SelectPhotoClick(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Photo not supported", ToastLength.Short).Show();
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 30
            }) ;
            if(file == null)
                return;
            imagefileBytes = System.IO.File.ReadAllBytes(file.Path);
            Item.fileBytes = imagefileBytes;
            Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeByteArray(imagefileBytes, 0, imagefileBytes.Length);
            itemImage.SetImageBitmap(bitmap);
        }

        private void AddCategoryClick(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.add_group_layout);
            d.SetTitle("Add Category");
            d.SetCancelable(true);
            d.Show();
            Button addCategoryConfirm = d.FindViewById<Button>(Resource.Id.addGroupBtn);
            EditText addCategoryTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            addCategoryTxt.Hint = "Enter category name";
            addCategoryConfirm.Click += AddCategoryConfirm;
        }

        private void AddCategoryConfirm(object sender, EventArgs e)
        {
            EditText addCategoryTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            Spinner categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            ArrayAdapter dataAdapter = (ArrayAdapter)categorySpinner.Adapter;
            if(addCategoryTxt.Text != "All")
                dataAdapter.Insert(addCategoryTxt.Text,0);
            categorySpinner.Adapter = dataAdapter;
            d.Dismiss();
        }

        private async void AddItemBtn_Click(object sender, EventArgs e)
        {
            itemName = FindViewById<EditText>(Resource.Id.itemName);
            itemDescription = FindViewById<EditText>(Resource.Id.itemDescription);
            categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            if(categorySpinner.Count == 0 || categorySpinner.SelectedItem == null)
            {
                Toast.MakeText(this, "Category not selected",ToastLength.Short).Show();
                return;
            }    
            string name = itemName.Text;
            string description = itemDescription.Text;
            string category = categorySpinner.SelectedItem.ToString();
            string groupId = Intent.GetStringExtra("groupId");
            if(name == "" || description == "" || category == "")
            {
                Toast.MakeText(this, "Please fill all fields", ToastLength.Short).Show();
                return;
            }
            Item item = new Item(this, name, description, category, groupId, imagefileBytes);
            ShowProgressBar("Adding item...");
            if (await item.AddItem())
            {
                HideProgressBar();
                Toast.MakeText(this, "Item added successfully", ToastLength.Short).Show();
                Finish();
            }
            HideProgressBar();
        }
        private void SetViewsEnabled(bool isEnabled)
        {
            FindViewById<EditText>(Resource.Id.itemName).Enabled = isEnabled;
            FindViewById<EditText>(Resource.Id.itemDescription).Enabled = isEnabled;
            FindViewById<Spinner>(Resource.Id.categorySpinner).Enabled = isEnabled;
            FindViewById<TextView>(Resource.Id.addCategoryBtn).Enabled = isEnabled;
            FindViewById<TextView>(Resource.Id.addItemBtn).Enabled = isEnabled;
        }
        private void ShowProgressBar(string message)
        {
            SetViewsEnabled(false);
            ProgressDialog progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage(message);
            progressDialog.SetCancelable(false);
            progressDialog.Show();
        }
        private void HideProgressBar()
        {
            SetViewsEnabled(true);
            ProgressDialog progressDialog = new ProgressDialog(this);
            if (progressDialog != null && progressDialog.IsShowing)
            {
                progressDialog.Dismiss();
            }
        }
    }
}