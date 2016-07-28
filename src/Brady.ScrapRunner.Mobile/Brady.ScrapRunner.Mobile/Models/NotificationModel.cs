namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("Notifications")]
    public class NotificationModel
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }
        public NotificationType NotificationType { get; set; }
        public string Summary { get; set; }
        [Ignore]
        public DateTimeOffset NotificationDateTimeOffset { get; set; }

        public string NotificationDateTimeOffsetWorkaround
        {
            get { return NotificationDateTimeOffset.ToString("O"); }
            set { NotificationDateTimeOffset = DateTimeOffset.Parse(value); }
        }
    }

    public enum NotificationType
    {
        NewTrip,
        TripModified,
        TripCanceled,
        TripOnHold,
        TripFuture,
        TripReassigned,
        TripUnassigned,
        TripMarkedDone,
        TripsResequenced,
        NewMessage
    }
}