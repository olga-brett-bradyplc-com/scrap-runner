namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using Android.App;
    using ViewModels;

    [Activity(
        Label = "New Message",
        Name = "brady.scraprunner.mobile.droid.activities.MessageNotificationActivity")]
    public class MessageNotificationActivity : BaseActivity<MessageNotificationViewModel>
    {
        protected override int ActivityId => Resource.Layout.activity_message_notification;

        public override void OnBackPressed()
        {
            // Swallow
        }
    }
}