namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.V7.Widget;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using ViewModels;

    [Activity(
        Label = "Notifications",
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.TripNotificationActivity")]
    public class TripNotificationActivity : MvxAppCompatActivity<TripNotificationViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_trip_notification);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = ViewModel.Title;
        }
    }
}