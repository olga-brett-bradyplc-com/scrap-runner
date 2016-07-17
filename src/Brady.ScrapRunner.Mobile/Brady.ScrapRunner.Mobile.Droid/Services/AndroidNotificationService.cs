namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Media;
    using Android.OS;
    using Android.Support.V4.App;
    using Domain.Models;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Views;
    using MvvmCross.Platform;
    using Resources;
    using ViewModels;

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

        private readonly IRepository<NotificationModel> _notificationModelRepository;

        public AndroidNotificationService()
        {
            _notificationModelRepository = Mvx.Resolve<IRepository<NotificationModel>>();
        }

        private PendingIntent GetIntent<TViewModel>(IDictionary<string, string> navigationParameters) where TViewModel : IMvxViewModel
        {
            var request = MvxViewModelRequest<TViewModel>.GetDefaultRequest();
            if (navigationParameters != null) request.ParameterValues = navigationParameters;
            var translator = Mvx.Resolve<IMvxAndroidViewModelRequestTranslator>();
            var intent = translator.GetIntentFor(request);
            return PendingIntent.GetActivity(Application.Context, 0, intent,
                PendingIntentFlags.UpdateCurrent);
        }

        private PendingIntent GetNotficationViewModelIntent(int notificationModelId)
        {
            return GetIntent<NotificationViewModel>(new Dictionary<string, string>
            {
                {"id", notificationModelId.ToString()}
            });
        }

        public async Task TripAsync(Trip trip, TripNotificationContext context)
        {
            var title = string.Empty;
            var text = string.Empty;
            var notificationId = 0;
            var notificationType = NotificationType.NewTrip;
            switch (context)
            {
                case TripNotificationContext.New:
                    title = AppResources.NotificationNewTripTitle;
                    text = string.Format(AppResources.NotificationNewTripText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = NewTripNotificationId;
                    notificationType = NotificationType.NewTrip;
                    break;
                case TripNotificationContext.Modified:
                    title = AppResources.NotificationTripModifiedTitle;
                    text = string.Format(AppResources.NotificationTripModifiedText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = ModifiedTripNotificationId;
                    notificationType = NotificationType.TripModified;
                    break;
                case TripNotificationContext.Canceled:
                    title = AppResources.NotificationTripCanceledTitle;
                    text = string.Format(AppResources.NotificationTripCanceledText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = CanceledTripNotificationId;
                    notificationType = NotificationType.TripCanceled;
                    break;
                case TripNotificationContext.OnHold:
                    title = AppResources.NotificationTripOnHoldTitle;
                    text = string.Format(AppResources.NotificationTripOnHoldText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = OnHoldTripNotificationId;
                    notificationType = NotificationType.TripOnHold;
                    break;
                case TripNotificationContext.Future:
                    title = AppResources.NotificationTripFutureTitle;
                    text = string.Format(AppResources.NotificationTripFutureText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = FutureTripNotificationId;
                    notificationType = NotificationType.TripFuture;
                    break;
                case TripNotificationContext.Reassigned:
                    title = AppResources.NotificationTripReassignedTitle;
                    text = string.Format(AppResources.NotificationTripReassignedText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = ReassignedTripNotificationId;
                    notificationType = NotificationType.TripReassigned;
                    break;
                case TripNotificationContext.Unassigned:
                    title = AppResources.NotificationTripUnassignedTitle;
                    text = string.Format(AppResources.NotificationTripUnassignedText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = UnassignedTripNotificationId;
                    notificationType = NotificationType.TripUnassigned;
                    break;
                case TripNotificationContext.MarkedDone:
                    title = AppResources.NotificationTripMarkedDoneTitle;
                    text = string.Format(AppResources.NotificationTripMarkedDoneText, trip.TripTypeDesc, trip.TripCustName, trip.TripNumber);
                    notificationId = MarkedDoneTripNotificationId;
                    notificationType = NotificationType.TripMarkedDone;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
            var builder = BuildNotification(title, text)
                .SetGroup(TripNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_assignment_late_white_24dp);
            var notificationModelId = await InsertNotificationModelAsync(notificationType, text);
            if (notificationModelId.HasValue)
                builder.SetContentIntent(GetNotficationViewModelIntent(notificationModelId.Value));
            var notification = builder.Build();
            NotifyUser(notification, notificationId);
        }

        public async Task TripsResequencedAsync()
        {
            var builder = BuildNotification(AppResources.NotificationTripResequenceTitle, 
                    AppResources.NotificationTripResequenceText)
                .SetGroup(TripResequenceNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_swap_vert_black_36dp);
            var notificationModelId = await InsertNotificationModelAsync(NotificationType.TripsResequenced, AppResources.NotificationTripResequenceText);
            if (notificationModelId.HasValue)
                builder.SetContentIntent(GetNotficationViewModelIntent(notificationModelId.Value));
            var notification = builder.Build();
            NotifyUser(notification, ResequencedTripNotficationId);
        }

        public async Task MessageAsync(Messages message)
        {
            var text = string.Format(AppResources.NotificationMessageText, message.MsgText);
            var builder = BuildNotification(string.Format(AppResources.NotificationMessageTitle, message.SenderName), text)
                .SetGroup(MessageNotificationGroup)
                .SetSmallIcon(Resource.Drawable.ic_email_black_36dp);
            var notificationModelId = await InsertNotificationModelAsync(NotificationType.NewMessage, text);
            if (notificationModelId.HasValue)
                builder.SetContentIntent(GetNotficationViewModelIntent(notificationModelId.Value));
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
                .SetContentTitle(title)
                .SetContentText(text);
        }

        private void NotifyUser(Notification notification, int notificationId)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            new Handler(Looper.MainLooper).Post(() =>
            {
                NotificationManager.Notify(notificationId, notification);
            });
        }

        private async Task<int?> InsertNotificationModelAsync(NotificationType type, string summary)
        {
            var notificationModel = new NotificationModel
            {
                NotificationDateTimeOffset = DateTimeOffset.Now,
                NotificationType = type,
                Summary = summary
            };
            await _notificationModelRepository.InsertAsync(notificationModel);
            return notificationModel.Id;
        }
    }
}