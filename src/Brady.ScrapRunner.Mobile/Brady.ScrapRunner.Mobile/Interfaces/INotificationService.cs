namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using Domain.Models;

    public interface INotificationService
    {
        void Trip(Trip trip, TripNotificationContext context);
        void TripsResequenced();
        void Message(Messages message);
        void ForcedLogoff();
    }

    public enum TripNotificationContext
    {
        /// <summary>
        /// A new trip was received.
        /// </summary>
        New,
        /// <summary>
        /// An existing trip was modified.
        /// </summary>
        Modified,
        /// <summary>
        /// An existing trip was canceled by dispatch.
        /// </summary>
        Canceled,
        /// <summary>
        /// An existing trip was put on hold by dispatch.
        /// </summary>
        OnHold,
        /// <summary>
        /// The ready date/time of an existing trip was changed to a point in the future.
        /// </summary>
        Future,
        /// <summary>
        /// An existing trip was reassigned to a different driver.
        /// </summary>
        Reassigned,
        /// <summary>
        /// An existing trip was unassigned by dispatch.
        /// </summary>
        Unassigned,
        /// <summary>
        /// An existing trip has been marked done by dispatch.
        /// </summary>
        MarkedDone
    }
}