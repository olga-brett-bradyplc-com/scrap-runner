namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using System;
    using System.ComponentModel;
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.V7.Widget;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using MvvmCross.Platform.WeakSubscription;
    using ViewModels;

    [Activity(
        Label = "Notifications",
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.TripNotificationActivity")]
    public class TripNotificationActivity : MvxAppCompatActivity<TripNotificationViewModel>
    {
        private IDisposable _titleToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_trip_notification);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = ViewModel.Title;
            _titleToken = ViewModel.WeakSubscribe(() => ViewModel.Title, OnTitleChanged);
        }

        private void OnTitleChanged(object sender, PropertyChangedEventArgs args)
        {
            SupportActionBar.Title = ViewModel.Title;
        }

        public override void OnBackPressed()
        {
            // Swallow
        }
    }
}