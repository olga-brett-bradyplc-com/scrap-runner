namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using Android.App;
    using Android.Content;
    using Interfaces;
    using Java.Util;

    public class QueueSchedulerAndroid : IQueueScheduler
    {
        protected const int AlarmRequestCode = 1024;

        public void Schedule(int timeoutMillis)
        {
            var calendar = Calendar.Instance;
            calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            calendar.Add(CalendarField.Millisecond, timeoutMillis);

            var alarmManager = AlarmManager.FromContext(Application.Context);
            alarmManager?.SetInexactRepeating(AlarmType.RtcWakeup, calendar.TimeInMillis, timeoutMillis, GetIntent());
        }

        public void Unschedule()
        {
            var queueServicePendingIntent = GetIntent();
            queueServicePendingIntent.Cancel();
            var alarmManager = AlarmManager.FromContext(Application.Context);
            alarmManager?.Cancel(queueServicePendingIntent);
        }

        private PendingIntent GetIntent()
        {
            var queueServiceIntent = new Intent(Application.Context, typeof(QueueIntentBroadcastReceiver));
            var queueServicePendingIntent = PendingIntent.GetBroadcast(Application.Context, AlarmRequestCode, queueServiceIntent, 0);
            return queueServicePendingIntent;
        }
    }
}