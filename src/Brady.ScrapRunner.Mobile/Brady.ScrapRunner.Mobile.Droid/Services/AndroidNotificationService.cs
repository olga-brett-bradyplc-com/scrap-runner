namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System.Threading;
    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.Support.V4.App;
    using Interfaces;
    using MvvmCross.Platform;

    public class AndroidNotificationService : INotificationService
    {
        private int _notificationId;

        public INotification Create(object id, object icon, string title, string text)
        {
            var newNotification = Mvx.Resolve<INotification>();
            newNotification.Id = id ?? GetNextNotificationId();
            newNotification.Icon = icon;
            newNotification.Title = title;
            newNotification.Text = text;
            return newNotification;
        }

        public void Notify(INotification notification)
        {
            GetNotificationManager()
                .Notify((int)notification.Id, BuildNativeNotification((AndroidNotification)notification));
        }

        public void Cancel(object notificationId)
        {
            GetNotificationManager()
                .Cancel((int)notificationId);
        }

        public void CancelAll()
        {
            GetNotificationManager()
                .CancelAll();
        }

        private NotificationManagerCompat GetNotificationManager()
        {
            return NotificationManagerCompat.From(Application.Context);
        }

        private int GetNextNotificationId()
        {
            return Interlocked.Increment(ref _notificationId);
        }

        private Notification BuildNativeNotification(AndroidNotification notification)
        {
            var builder = (NotificationCompat.Builder) notification.Context;
            var builtNotification = builder.Build();
            // You can also specify other Android specific options here such as NoClear, Ongoing, etc.
            return builtNotification;
        }
    }

    public class AndroidNotification : INotification
    {
        private readonly NotificationCompat.Builder _builder;

        public AndroidNotification(Context context)
        {
            _builder = new NotificationCompat.Builder(context)
                .SetDefaults(NotificationCompat.DefaultAll)
                .SetColor(Resource.Color.colorPrimary)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVibrate(new[] {1000L, 1000L, 1000L, 1000L, 1000L})
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));
            Context = _builder;
        }

        public object Id { get; set; }

        private int? _drawableId;
        public object Icon
        {
            get
            {
                return _drawableId;
            }
            set
            {
                _drawableId = (int?)value;
                if (_drawableId.HasValue)
                {
                    _builder.SetSmallIcon(_drawableId.Value);
                }
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                _builder.SetContentTitle(_title);
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _builder.SetContentText(_text);
            }
        }

        public object Context { get; set; }
    }
}