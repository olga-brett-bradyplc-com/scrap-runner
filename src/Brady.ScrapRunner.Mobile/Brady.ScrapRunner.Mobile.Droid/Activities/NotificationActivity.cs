namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using ViewModels;

    [Activity(
        Label = "Notification",
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.NotificationActivity")]
    public class NotificationActivity : MvxAppCompatActivity<NotificationViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // @TODO: Implement an actual UI here;
            SetContentView(Resource.Layout.fragment_delay);
        }
    }
}