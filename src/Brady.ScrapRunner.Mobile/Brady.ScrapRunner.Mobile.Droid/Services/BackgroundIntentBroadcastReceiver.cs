namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using Android.Content;
    using Android.Support.V4.Content;

    [BroadcastReceiver]
    public class BackgroundIntentBroadcastReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var serviceIntent = new Intent(context, typeof(BackgroundIntentService));
            StartWakefulService(context, serviceIntent);
        }
    }
}