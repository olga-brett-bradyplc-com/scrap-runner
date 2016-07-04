namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System;
    using Android.App;
    using Android.Media;
    using Android.OS;
    using Android.Support.V4.App;
    using Domain.Models;
    using Interfaces;

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
                    title = "New Trip";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been dispatched.";
                    notificationId = NewTripNotificationId;
                    break;
                case TripNotificationContext.Modified:
                    title = "Trip Modified";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been modified by dispatch.";
                    notificationId = ModifiedTripNotificationId;
                    break;
                case TripNotificationContext.Canceled:
                    title = "Trip Canceled";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been canceled by dispatch.";
                    notificationId = CanceledTripNotificationId;
                    break;
                case TripNotificationContext.OnHold:
                    title = "Trip Placed On Hold";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been placed on hold by dispatch.";
                    notificationId = OnHoldTripNotificationId;
                    break;
                case TripNotificationContext.Future:
                    title = "Future Trip";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been changed to a future date.";
                    notificationId = FutureTripNotificationId;
                    break;
                case TripNotificationContext.Reassigned:
                    title = "Trip Reassigned";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been reassigned to a different driver.";
                    notificationId = ReassignedTripNotificationId;
                    break;
                case TripNotificationContext.Unassigned:
                    title = "Trip Unassigned";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been unassigned.";
                    notificationId = UnassignedTripNotificationId;
                    break;
                case TripNotificationContext.MarkedDone:
                    title = "Trip Marked Done";
                    text = $"{trip.TripTypeDesc} trip {trip.TripCustName} ({trip.TripNumber}) has been marked done by dispatch.";
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
            var title = "Trips Resequenced";
            var text = "Trips have been resequenced.";
            var builder = BuildNotification(title, text)
                .SetGroup(TripResequenceNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_swap_vert_black_36dp);
            var notification = builder.Build();
            NotifyUser(notification, ResequencedTripNotficationId);
        }

        public void Message(Messages message)
        {
            var title = $"Message from {message.SenderName}";
            var text = message.MsgText;
            var builder = BuildNotification(title, text)
                .SetGroup(MessageNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_email_black_36dp);
            var notification = builder.Build();
            NotifyUser(notification, NewMessageNotificationId);
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