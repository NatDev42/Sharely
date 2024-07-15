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

namespace Sharely
{
    internal class DateRange
    {
        public string itemId;
        public string eventName;
        public DateTime startDate;
        public DateTime endDate;
        public const string DATERANGE_COLLECTION_NAME = "dateRanges";
        FirebaseFirestore db = AppDataHelper.GetFirestore();
        Context context;
        public DateRange() { }
        public DateRange(string itemId, string eventName, DateTime startDate, DateTime endDate, Context context)
        {
            this.itemId = itemId;
            this.eventName = eventName;
            this.startDate = startDate;
            this.endDate = endDate;
            this.db = db;
            this.context = context;
        }

        public async Task<bool> AddDateRange(List<DateRange> ranges)
        {
            try
            {
                if (startDate == null)
                {
                    Toast.MakeText(context, "Start date is required", ToastLength.Short).Show();
                    return false;
                }
                if (endDate == null)
                {
                    Toast.MakeText(context, "End date is required", ToastLength.Short).Show();
                    return false;
                }
                if (string.IsNullOrEmpty(eventName))
                {
                    Toast.MakeText(context, "Event name is required", ToastLength.Short).Show();
                    return false;
                }
                if (startDate > endDate)
                {
                    Toast.MakeText(context, "Valid start and end dates required", ToastLength.Short).Show();
                    return false;
                }
                foreach (DateRange dr in ranges)
                {
                    if (startDate < dr.endDate && endDate > dr.startDate)
                        throw new Exception("Date range is overlapping with another date range");
                }
                HashMap dateRangeMap = new HashMap();
                dateRangeMap.Put("itemId", this.itemId);
                dateRangeMap.Put("eventName", this.eventName);
                dateRangeMap.Put("startDate", this.startDate.ToString());
                dateRangeMap.Put("endDate", this.endDate.ToString());
                DocumentReference dateRangeReference = db.Collection(DATERANGE_COLLECTION_NAME).Document();
                await dateRangeReference.Set(dateRangeMap);
            }
            catch (Exception ex)
            {
                string s = ex.Message;

                Toast.MakeText(context, s, ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        public async Task<bool> RemoveDateRange()
        {
            try
            {
                string formattedEndDate = endDate.ToString("M/d/yyyy h:mm:ss tt");

                QuerySnapshot query = (QuerySnapshot)await db.Collection(DATERANGE_COLLECTION_NAME)
                    .WhereEqualTo("itemId", itemId)
                    .WhereEqualTo("eventName", eventName)
                    .WhereEqualTo("endDate", formattedEndDate)
                    .Get();

                if (query.Documents.Count == 0)
                    return false;

                await query.Documents.FirstOrDefault().Reference.Delete();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return false;
            }
            return true;
        }
    }
}
