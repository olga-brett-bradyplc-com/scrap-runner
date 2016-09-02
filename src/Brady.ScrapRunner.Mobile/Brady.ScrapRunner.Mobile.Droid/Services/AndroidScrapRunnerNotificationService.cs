namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Support.V4.App;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Core.Views;
    using MvvmCross.Droid.Views;
    using MvvmCross.Platform;
    using Resources;
    using ViewModels;

    public class AndroidScrapRunnerNotificationService : IScrapRunnerNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly IRepository<NotificationModel> _notificationModelRepository;

        public AndroidScrapRunnerNotificationService(INotificationService notificationService, 
            IRepository<NotificationModel> notificationModelRepository)
        {
            _notificationService = notificationService;
            _notificationModelRepository = notificationModelRepository;
        }

        public async Task TripAsync(TripModel trip, TripNotificationContext context)
        {
            var drawable = Resource.Drawable.ic_assignment_late_white_24dp;
            INotification notification = null;
            NotificationType notificationType;
            switch (context)
            {
                case TripNotificationContext.New:
                    notificationType = NotificationType.NewTrip;
                    notification = _notificationService.Create(
                        id: null, 
                        icon: drawable,
                        title: AppResources.NotificationNewTripTitle,
                        text: string.Format(AppResources.NotificationNewTripText, trip.TripNumber, trip.TripCustName));
                    break;
                case TripNotificationContext.Modified:
                    notificationType = NotificationType.TripModified;;
                    notification = _notificationService.Create(
                        id: null,
                        icon: drawable,
                        title: AppResources.NotificationTripModifiedTitle,
                        text: string.Format(AppResources.NotificationTripModifiedText, trip.TripNumber, trip.TripCustName));
                    break;
                case TripNotificationContext.Canceled:
                    notificationType = NotificationType.TripCanceled;
                    notification = _notificationService.Create(
                        id: null,
                        icon: drawable,
                        title: AppResources.NotificationTripCanceledTitle,
                        text: string.Format(AppResources.NotificationTripCanceledText, trip.TripCustName));
                    break;
                case TripNotificationContext.OnHold:
                    notificationType = NotificationType.TripOnHold;
                    notification = _notificationService.Create(
                        id: null,
                        icon: drawable,
                        title: AppResources.NotificationTripOnHoldTitle,
                        text: string.Format(AppResources.NotificationTripOnHoldText, trip.TripCustName));
                    break;
                case TripNotificationContext.Future:
                case TripNotificationContext.Reassigned:
                case TripNotificationContext.Unassigned:
                    notificationType = NotificationType.TripCanceled;
                    notification = _notificationService.Create(
                        id: null,
                        icon: drawable,
                        title: AppResources.NotificationTripCanceledTitle,
                        text: string.Format(AppResources.NotificationTripCanceledText, trip.TripCustName));
                    break;
                case TripNotificationContext.MarkedDone:
                    notificationType = NotificationType.TripMarkedDone;
                    notification = _notificationService.Create(
                        id: null,
                        icon: drawable,
                        title: AppResources.NotificationTripMarkedDoneTitle,
                        text: string.Format(AppResources.NotificationTripMarkedDoneText, trip.TripCustName));
                    break;
                case TripNotificationContext.Resequenced:
                    notificationType = NotificationType.TripsResequenced;
                    notification = _notificationService.Create(
                        id: null,
                        icon: drawable,
                        title: AppResources.NotificationTripResequenceTitle,
                        text: AppResources.NotificationTripResequenceText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
            await UpdateNotificationContentIntentAsync(notificationType, (AndroidNotification)notification);
            ShowTripNotificationActivity(trip.TripNumber, context, notification.Id);
            _notificationService.Notify(notification);
        }

        public async Task TripsResequencedAsync()
        {
            var notificationType = NotificationType.TripsResequenced;
            var notification = _notificationService.Create(
                id: null,
                icon: Resource.Drawable.ic_swap_vert_black_36dp,
                title: AppResources.NotificationTripResequenceTitle,
                text: AppResources.NotificationTripResequenceText);
            await UpdateNotificationContentIntentAsync(notificationType, (AndroidNotification)notification);
            ShowTripNotificationActivity(string.Empty, TripNotificationContext.Resequenced, notification.Id);
            _notificationService.Notify(notification);
        }

        public async Task MessageAsync(MessagesModel message)
        {
            var notificationType = NotificationType.NewMessage;
            var notification = _notificationService.Create(
                id: null,
                icon: Resource.Drawable.ic_email_black_36dp,
                title: string.Format(AppResources.NotificationMessageTitle, message.SenderName),
                text: string.Format(AppResources.NotificationMessageText, message.SenderName, message.MsgText));
            await UpdateNotificationContentIntentAsync(notificationType, (AndroidNotification)notification);
            ShowMessageNotificationActivity(message.MsgId.Value, notification.Id);
            _notificationService.Notify(notification);
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

        private async Task UpdateNotificationContentIntentAsync(NotificationType notificationType, AndroidNotification notification)
        {
            var notificationModelId = await InsertNotificationModelAsync(notificationType, notification.Text);
            if (notificationModelId.HasValue)
            {
                var builder = (NotificationCompat.Builder)notification.Context;
                builder.SetContentIntent(GetViewModelPendingIntent<NotificationViewModel>(null));
            }
        }

        private PendingIntent GetViewModelPendingIntent<TViewModel>(IDictionary<string, string> navigationParameters) where TViewModel : IMvxViewModel
        {
            var request = MvxViewModelRequest<TViewModel>.GetDefaultRequest();
            if (navigationParameters != null) request.ParameterValues = navigationParameters;
            var translator = Mvx.Resolve<IMvxAndroidViewModelRequestTranslator>();
            var intent = translator.GetIntentFor(request);
            return PendingIntent.GetActivity(Application.Context, 0, intent,
                PendingIntentFlags.UpdateCurrent);
        }

        private void ShowViewModel<TViewModel>(IDictionary<string, string> parameterValues) where TViewModel : BaseViewModel
        {
            var request = MvxViewModelRequest<TViewModel>.GetDefaultRequest();
            request.ParameterValues = parameterValues;
            var viewDispatcher = Mvx.Resolve<IMvxViewDispatcher>();
            viewDispatcher.ShowViewModel(request);
        }

        private void ShowTripNotificationActivity(string tripNumber, TripNotificationContext context, object notificationId)
        {
            ShowViewModel<TripNotificationViewModel>(new Dictionary<string, string>
            {
               { "tripNumber", tripNumber },
               { "notificationContext", context.ToString() },
               { "notificationId", Convert.ToInt32(notificationId).ToString() }
            });
        }

        private void ShowMessageNotificationActivity(int messageId, object notificationId)
        {
            ShowViewModel<MessageNotificationViewModel>(new Dictionary<string, string>
            {
                { "messageId", messageId.ToString()},
                { "notificationId", Convert.ToInt32(notificationId).ToString() }
            });
        }
    }
}