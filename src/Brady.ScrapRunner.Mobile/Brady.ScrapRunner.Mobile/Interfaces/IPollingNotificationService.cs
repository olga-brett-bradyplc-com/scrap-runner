namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using BWF.DataServices.Domain.Models;
    using Domain.Models;

    public interface IPollingNotificationService
    {
        void NotifyTripsNew(QueryResult<Trip> tripQueryResult);
        void NotifyTripsModified(QueryResult<Trip> tripQueryResult);
        void NotifyTripsCanceled(QueryResult<Trip> tripQueryResult);
        void NotifyTripsUnassigned(QueryResult<Trip> tripQueryResult);
        void NotifyTripsMarkedDone(QueryResult<Trip> tripQueryResult);
        void NotifyTripsResequenced(QueryResult<Trip> tripQueryResult);
        void NotifyMessages(QueryResult<Messages> messagesQueryResult);
    }
}