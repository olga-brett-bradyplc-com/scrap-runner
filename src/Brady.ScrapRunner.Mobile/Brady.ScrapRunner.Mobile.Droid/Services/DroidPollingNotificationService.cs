namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System;
    using Android.App;
    using Android.Media;
    using Android.OS;
    using Android.Support.V4.App;
    using BWF.DataServices.Domain.Models;
    using Domain.Models;
    using Interfaces;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Views;
    using MvvmCross.Platform;
    using Resources;

    public class DroidPollingNotificationService : IPollingNotificationService
    {
        protected const int IdTripsNew = 1;
        protected const int IdTripsModified = 2;
        protected const int IdTripsCanceled = 3;
        protected const int IdTripsUnassigned = 4;
        protected const int IdTripsMarkedDone = 5;
        protected const int IdTripsResequenced = 6;
        protected const int IdNewMessage = 7;

        public void NotifyTripsNew(QueryResult<Trip> tripQueryResult)
        {
            var notificationBuilder = BuildNotification(AppResources.TripsNew, string.Empty);
            ShowNotification(notificationBuilder.Build(), IdTripsNew);
        }

        public void NotifyTripsModified(QueryResult<Trip> tripQueryResult)
        {
            var notificationBuilder = BuildNotification(AppResources.TripsModified, string.Empty);
            ShowNotification(notificationBuilder.Build(), IdTripsNew);
        }

        public void NotifyTripsCanceled(QueryResult<Trip> tripQueryResult)
        {
            var notificationBuilder = BuildNotification(AppResources.TripsCanceled, string.Empty);
            ShowNotification(notificationBuilder.Build(), IdTripsCanceled);
        }

        public void NotifyTripsUnassigned(QueryResult<Trip> tripQueryResult)
        {
            var notificationBuilder = BuildNotification(AppResources.TripsUnassigned, string.Empty);
            ShowNotification(notificationBuilder.Build(), IdTripsUnassigned);
        }

        public void NotifyTripsMarkedDone(QueryResult<Trip> tripQueryResult)
        {
            var notificationBuilder = BuildNotification(AppResources.TripsMarkedDone, string.Empty);
            ShowNotification(notificationBuilder.Build(), IdTripsMarkedDone);
        }

        public void NotifyTripsResequenced(QueryResult<Trip> tripQueryResult)
        {
            var notificationBuilder = BuildNotification(AppResources.TripsResequenced, AppResources.TripsResequencedBody);
            ShowNotification(notificationBuilder.Build(), IdTripsResequenced);
        }

        public void NotifyMessages(QueryResult<Messages> messagesQueryResult)
        {
            var messageCount = messagesQueryResult.Records.Count;
            var notificationBuilder = BuildNotification(AppResources.Message, string.Empty);
            notificationBuilder.SetNumber(messageCount);
            notificationBuilder.SetCategory(NotificationCompat.CategoryEmail); // Overrides CategoryStatus from BuildNotification()
            ShowNotification(notificationBuilder.Build(), IdNewMessage);
        }

        /// <summary>
        /// Creates an Android PendingIntent used for navigating to an MvvmCross ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel">The MvxViewModel to navigate to.</typeparam>
        /// <returns>Returns an Android PendingIntent used for navigating to an MvvmCross ViewModel.</returns>
        private PendingIntent GetIntentForViewModel<TViewModel>() where TViewModel : IMvxViewModel
        {
            var viewModelRequest = MvxViewModelRequest<TViewModel>.GetDefaultRequest();
            var viewModelRequestTranslator = Mvx.Resolve<IMvxAndroidViewModelRequestTranslator>();
            var intent = viewModelRequestTranslator.GetIntentFor(viewModelRequest);
            return PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
        }

        private NotificationCompat.Builder BuildNotification(string title, string text)
        {
            var notificationBuilder = new NotificationCompat.Builder(Application.Context);
            notificationBuilder.SetDefaults(NotificationCompat.DefaultAll);
            notificationBuilder.SetAutoCancel(true);
            notificationBuilder.SetColor(Resource.Color.colorPrimary);
            notificationBuilder.SetCategory(NotificationCompat.CategoryStatus);
            notificationBuilder.SetPriority(NotificationCompat.PriorityHigh);
            notificationBuilder.SetVibrate(new [] {1000L, 1000L, 1000L, 1000L, 1000L});
            notificationBuilder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));
            if (!string.IsNullOrEmpty(title)) notificationBuilder.SetContentTitle(title);
            if (!string.IsNullOrEmpty(text)) notificationBuilder.SetContentText(text);
            // Other values to set:
            // SetNumber(42);
            // SetSmallIcon(Resource.Mipmap.ic_launcher);
            // SetContentIntent(GetIntentForViewModel<TViewModel>());
            return notificationBuilder;
        }

        private void ShowNotification(Notification notification, int notificationId)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var supportNotificationManager = NotificationManagerCompat.From(Application.Context);
            new Handler(Looper.MainLooper).Post(() =>
            {
                supportNotificationManager.Notify(notificationId, notification);
            });
        }
    }
}