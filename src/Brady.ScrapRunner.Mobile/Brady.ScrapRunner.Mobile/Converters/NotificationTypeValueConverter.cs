namespace Brady.ScrapRunner.Mobile.Converters
{
    using System;
    using System.Globalization;
    using Models;
    using MvvmCross.Platform.Converters;
    using Resources;

    public class NotificationTypeValueConverter : MvxValueConverter<NotificationType, string>
    {
        protected override string Convert(NotificationType value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case NotificationType.NewTrip:
                    return AppResources.NotificationNewTripTitle;
                case NotificationType.TripModified:
                    return AppResources.NotificationTripModifiedTitle;
                case NotificationType.TripCanceled:
                case NotificationType.TripFuture:
                case NotificationType.TripReassigned:
                case NotificationType.TripUnassigned:
                    return AppResources.NotificationTripCanceledTitle;
                case NotificationType.TripOnHold:
                    return AppResources.NotificationTripOnHoldTitle;
                case NotificationType.TripMarkedDone:
                    return AppResources.NotificationTripMarkedDoneTitle;
                case NotificationType.TripsResequenced:
                    return AppResources.NotificationTripResequenceTitle;
                case NotificationType.NewMessage:
                    return AppResources.NotificationNewMessage;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}