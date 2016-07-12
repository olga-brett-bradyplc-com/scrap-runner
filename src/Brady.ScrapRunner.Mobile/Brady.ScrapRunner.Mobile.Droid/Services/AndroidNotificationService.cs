namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System;
    using Android.App;
    using Android.Media;
    using Android.OS;
    using Android.Support.V4.App;
    using Domain.Models;
    using Interfaces;
    using Resources;

    public class AndroidNotificationService : INotificationService
    {
        public const int NewTripNotificationId = 1;
        public const int ModifiedTripNotificationId = 2;
        public const int CanceledTripNotificationId = 3;
        public const int OnHoldTripNotificationId = 4;
        public const int FutureTripNotificationId = 5;
        public const int ReassignedTripNotificationId = 6;
        public const int UnassignedTripNotificationId = 7;
        public const int MarkedDoneTripNotificationId = 8;
        public const int NewMessageNotificationId = 9;
        public const int ResequencedTripNotficationId = 10;
        public const int ForcedLogoffNotificationId = 11;

        public const string TripNotificationGroup = "TripNotificationGroup";
        public const string TripResequenceNotificationGroup = "TripResequenceNotificationGroup";
        public const string MessageNotificationGroup = "MessageNotificationGroup";

        public void Trip(Trip trip, TripNotificationContext context)
        {
            var title = string.Empty;
            var text = string.Empty;
            var notificationId = 0;
            switch (context)
            {
                case TripNotificationContext.New:
                    title = AppResources.NotificationNewTripTitle;
                    text = string.Format(AppResources.NotificationNewTripText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = NewTripNotificationId;
                    break;
                case TripNotificationContext.Modified:
                    title = AppResources.NotificationTripModifiedTitle;
                    text = string.Format(AppResources.NotificationTripModifiedText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = ModifiedTripNotificationId;
                    break;
                case TripNotificationContext.Canceled:
                    title = AppResources.NotificationTripCanceledTitle;
                    text = string.Format(AppResources.NotificationTripCanceledText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = CanceledTripNotificationId;
                    break;
                case TripNotificationContext.OnHold:
                    title = AppResources.NotificationTripOnHoldTitle;
                    text = string.Format(AppResources.NotificationTripOnHoldText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = OnHoldTripNotificationId;
                    break;
                case TripNotificationContext.Future:
                    title = AppResources.NotificationTripFutureTitle;
                    text = string.Format(AppResources.NotificationTripFutureText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = FutureTripNotificationId;
                    break;
                case TripNotificationContext.Reassigned:
                    title = AppResources.NotificationTripReassignedTitle;
                    text = string.Format(AppResources.NotificationTripReassignedText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = ReassignedTripNotificationId;
                    break;
                case TripNotificationContext.Unassigned:
                    title = AppResources.NotificationTripUnassignedTitle;
                    text = string.Format(AppResources.NotificationTripUnassignedText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = UnassignedTripNotificationId;
                    break;
                case TripNotificationContext.MarkedDone:
                    title = AppResources.NotificationTripMarkedDoneTitle;
                    text = string.Format(AppResources.NotificationTripMarkedDoneText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = MarkedDoneTripNotificationId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
            var builder = BuildNotification(title, text)
                .SetGroup(TripNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_assignment_late_white_24dp);
            var notification = builder.Build();
            NotifyUser(notification, notificationId);
        }

        public void TripsResequenced()
        {
            var builder = BuildNotification(AppResources.NotificationTripResequenceTitle, 
                    AppResources.NotificationTripResequenceText)
                .SetGroup(TripResequenceNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_swap_vert_black_36dp);
            var notification = builder.Build();
            NotifyUser(notification, ResequencedTripNotficationId);
        }

        public void Message(Messages message)
        {
            var builder = BuildNotification(string.Format(AppResources.NotificationMessageTitle, message.SenderName),
                    string.Format(AppResources.NotificationMessageText, message.MsgText))
                .SetGroup(MessageNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_email_black_36dp);
            var notification = builder.Build();
            NotifyUser(notification, NewMessageNotificationId);
        }

        public void ForcedLogoff()
        {
            var builder = BuildNotification(AppResources.ForcedLogoff,
                    AppResources.ForcedLogoffMessage)
                .SetGroup(MessageNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_email_black_36dp);
            var notification = builder.Build();
            NotifyUser(notification, ForcedLogoffNotificationId);
        }

        private NotificationManagerCompat NotificationManager => NotificationManagerCompat.From(Application.Context);

        private NotificationCompat.Builder BuildNotification(string title, string text)
        {
            return new NotificationCompat.Builder(Application.Context)
                .SetDefaults(NotificationCompat.DefaultAll)
                .SetAutoCancel(true)
                .SetCategory(NotificationCompat.CategoryStatus)
                .SetColor(Resource.Color.colorPrimary)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVibrate(new[] {1000L, 1000L, 1000L, 1000L, 1000L})
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                .SetContentTitle(title).SetContentText(text);
        }

        private void NotifyUser(Notification notification, int notificationId)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            new Handler(Looper.MainLooper).Post(() =>
            {
                NotificationManager.Notify(notificationId, notification);
            });
        }
    }
}