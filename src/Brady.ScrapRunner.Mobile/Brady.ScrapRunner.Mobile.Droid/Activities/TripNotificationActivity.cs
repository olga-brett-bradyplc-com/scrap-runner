namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using Android.App;
    using ViewModels;

    [Activity(
        Label = "Trip",
        Name = "brady.scraprunner.mobile.droid.activities.TripNotificationActivity")]
    public class TripNotificationActivity : BaseActivity<TripNotificationViewModel>
    {
        protected override int ActivityId => Resource.Layout.activity_trip_notification;

        public override void OnBackPressed()
        {
            // Swallow
        }
    }
}