using Android.App;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sharely
{
    internal class DateRangeListener : Java.Lang.Object, IEventListener
    {
        public event EventHandler<DateRangeArgs> OnDateRangeRetrieved; // contains the event details
        List<DateRange> dateRanges;
        DateTime dateTime;

        public class DateRangeArgs : EventArgs
        {
            internal List<DateRange> dateRangeDB { get; set; }
        }

        // set it to listen to the group collection
        public DateRangeListener()
        {
            AppDataHelper.GetFirestore().Collection(DateRange.DATERANGE_COLLECTION_NAME).AddSnapshotListener(this);
        }

        public void OnEvent(Java.Lang.Object value, FirebaseFirestoreException error)
        {
            QuerySnapshot snapshot = (QuerySnapshot)value;
            dateRanges = new List<DateRange>();
            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                DateRange dateRange = new DateRange();
                if (documentSnapshot != null)
                {
                        dateRange.itemId = documentSnapshot.GetString("itemId") ?? "";
                        dateRange.eventName = documentSnapshot.GetString("eventName") ?? "";
                    if (documentSnapshot.Get("startDate") != null)
                    {
                        string date = documentSnapshot.GetString("startDate");
                        DateTime dateAsDateTime;
                        DateTime.TryParse(date, out dateAsDateTime);
                        dateRange.startDate = dateAsDateTime;
                        if (dateRange.startDate < DateTime.Now)
                            dateRange.startDate = DateTime.Now;
                    }
                    else
                        dateRange.startDate = DateTime.MinValue;
                    if (documentSnapshot.Get("endDate") != null)
                    {
                        string date = documentSnapshot.GetString("endDate");
                        DateTime dateAsDateTime;
                        DateTime.TryParse(date, out dateAsDateTime);
                        dateRange.endDate = dateAsDateTime;
                    }
                    else
                        dateRange.endDate = DateTime.MinValue;
                    if(dateRange.endDate.Date >= DateTime.Now.Date)
                        dateRanges.Add(dateRange);
                }
            }
            if(dateRanges != null)
                dateRanges.Sort((x, y) => x.startDate.CompareTo(y.startDate));
            if (OnDateRangeRetrieved != null)
            {
                DateRangeArgs e = new DateRangeArgs();
                e.dateRangeDB = dateRanges;
                OnDateRangeRetrieved.Invoke(this, e);
            }
        }
    }
}