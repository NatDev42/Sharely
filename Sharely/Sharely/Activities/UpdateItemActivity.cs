using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using FFImageLoading;
using Plugin.Media;
using Sharely.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using static Android.Content.ClipData;

namespace Sharely
{
    [Activity(Label = "UpdateItemActivity")]
    public class UpdateItemActivity : Activity
    {
        Dialog d;
        ItemEventListener ItemEventListener = new ItemEventListener();
        ImageView itemImage;
        EditText itemName, itemDescription;
        Spinner categorySpinner;
        List<Item> allItems;
        ProgressDialog progressDialog;
        string groupId;
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
            itemName = FindViewById<EditText>(Resource.Id.itemName);
            itemDescription = FindViewById<EditText>(Resource.Id.itemDescription);
            categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            TextView addCategoryBtn = FindViewById<TextView>(Resource.Id.addCategoryBtn);
            TextView addItemBtn = FindViewById<TextView>(Resource.Id.addItemBtn);
            addItemBtn.Text = "Update Item";
            itemImage.Click += SelectPhotoClick;
            addCategoryBtn.Click += AddCategoryClick;
            addItemBtn.Click += AddItemBtn_Click;
            ItemEventListener.OnItemRetrieved += ItemEventListener_OnItemRetrieved;
            RequestPermissions(permissionGroup, 0);
            groupId = Intent.GetStringExtra("groupId");
        }

        private void ItemEventListener_OnItemRetrieved(object sender, ItemEventListener.ItemEventArgs e)
        {
            Item item = e.itemDB.Where(item => item.Id == Intent.GetStringExtra("itemId")).FirstOrDefault();
            allItems = e.itemDB.Where(pos => pos.groupId == item.groupId).ToList();
            categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            //Add to the spinner the category 'All' and all distinct categories from itemDB
            List<string> categories = allItems.Where(item => item.category != null).Select(item => item.category).Distinct().ToList();
            ArrayAdapter dataAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, categories);
            dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            categorySpinner.Adapter = dataAdapter;

            //entering existing item data into the fields
            itemName.Text = item.name;
            itemDescription.Text = item.description;
            categorySpinner.SetSelection(dataAdapter.GetPosition(item.category));
            if (item.imageUrl != null)
                ImageService.Instance.LoadUrl(item.imageUrl).Retry(3, 200).DownSample(400, 400).Into(itemImage);
        }

        private async void SelectPhotoClick(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Photo not supported", ToastLength.Short).Show();
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;
            imagefileBytes = System.IO.File.ReadAllBytes(file.Path);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imagefileBytes, 0, imagefileBytes.Length);
            itemImage.SetImageBitmap(bitmap);
        }

        private void AddCategoryClick(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.add_group_layout);
            d.SetTitle("Add Category");
            d.SetCancelable(true);
            d.Show();
            EditText addCategoryTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            Button addCategoryBtn = d.FindViewById<Button>(Resource.Id.addCategoryBtn);
            addCategoryTxt.Hint = "Enter category name";
            addCategoryBtn.Click += AddCategoryConfirm;
        }

        private void AddCategoryConfirm(object sender, EventArgs e)
        {
            EditText addCategoryTxt = d.FindViewById<EditText>(Resource.Id.groupNameTxt);
            Spinner categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            ArrayAdapter dataAdapter = (ArrayAdapter)categorySpinner.Adapter;
            if (addCategoryTxt.Text != "All")
                dataAdapter.Insert(addCategoryTxt.Text, 0);
            categorySpinner.Adapter = dataAdapter;
            d.Dismiss();
        }

        private async void AddItemBtn_Click(object sender, EventArgs e)
        {
            itemName = FindViewById<EditText>(Resource.Id.itemName);
            itemDescription = FindViewById<EditText>(Resource.Id.itemDescription);
            categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            if (categorySpinner.Count == 0 || categorySpinner.SelectedItem == null)
            {
                Toast.MakeText(this, "Category not selected", ToastLength.Short).Show();
                return;
            }
            string name = itemName.Text;
            string description = itemDescription.Text;
            string category = categorySpinner.SelectedItem.ToString();
            if (name == "" || description == "" || category == "")
            {
                Toast.MakeText(this, "Please fill all fields", ToastLength.Short).Show();
                return;
            }
            ShowProgressBar("Updating item...");
            //find the items' id
            Item item = allItems.Where(item => item.Id == Intent.GetStringExtra("itemId")).FirstOrDefault();
            item.context = this;
            item.name = name;
            item.description = description;
            item.category = category;
            item.groupId = groupId;
            if (await item.UpdateItem())
            {
                HideProgressBar();
                Toast.MakeText(this, "Item updated successfully", ToastLength.Short).Show();
                Finish();
            }
            HideProgressBar();
        }
        private void ShowProgressBar(string message)
        {
            SetViewsEnabled(false);
            progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage(message);
            progressDialog.SetCancelable(false);
            progressDialog.Show();
        }
        private void HideProgressBar()
        {
            SetViewsEnabled(true);
            if (progressDialog != null && progressDialog.IsShowing)
            {
                progressDialog.Dismiss();
                progressDialog = null;
            }
        }
        private void SetViewsEnabled(bool enabled)
        {
            FindViewById<EditText>(Resource.Id.itemName).Enabled = enabled;
            FindViewById<EditText>(Resource.Id.itemDescription).Enabled = enabled;
            FindViewById<Spinner>(Resource.Id.categorySpinner).Enabled = enabled;
            FindViewById<TextView>(Resource.Id.addCategoryBtn).Enabled = enabled;
            FindViewById<TextView>(Resource.Id.addItemBtn).Enabled = enabled;
        }
    }
}