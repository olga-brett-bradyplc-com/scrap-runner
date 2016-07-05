namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using Android.App;
    using Android.Content;
    using Interfaces;
    using Java.Util;

    public class AndroidBackgroundScheduler : IBackgroundScheduler
    {
        protected const int AlarmRequestCode = 1024;

        public void Schedule(int timeoutMillis)
        {
            var calendar = Calendar.Instance;
            calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            calendar.Add(CalendarField.Millisecond, timeoutMillis);

            // InexactRepeating alarms will be batched together across the system for power savings.
            // Timeouts < 1 minute will often be rounded up to 1 minute.
            var alarmManager = AlarmManager.FromContext(Application.Context);
            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, AlarmRequestCode, NewIntent(), 0);
            alarmManager?.SetInexactRepeating(AlarmType.RtcWakeup, calendar.TimeInMillis, timeoutMillis, pendingIntent);
        }

        public void Unschedule()
        {
            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, AlarmRequestCode, 
                NewIntent(), PendingIntentFlags.NoCreate);
            if (pendingIntent == null) return;
            pendingIntent.Cancel();
            var alarmManager = AlarmManager.FromContext(Application.Context);
            alarmManager?.Cancel(pendingIntent);
        }

        private static Intent NewIntent()
        {
            return new Intent(Application.Context, typeof(BackgroundIntentBroadcastReceiver));
        }
    }
}