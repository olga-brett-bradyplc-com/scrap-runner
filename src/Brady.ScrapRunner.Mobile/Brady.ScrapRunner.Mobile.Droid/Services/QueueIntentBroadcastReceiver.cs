namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using Android.Content;
    using Android.Support.V4.Content;

    [BroadcastReceiver]
    public class QueueIntentBroadcastReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var queueServiceIntent = new Intent(context, typeof(QueueIntentService));
            StartWakefulService(context, queueServiceIntent);
        }
    }
}