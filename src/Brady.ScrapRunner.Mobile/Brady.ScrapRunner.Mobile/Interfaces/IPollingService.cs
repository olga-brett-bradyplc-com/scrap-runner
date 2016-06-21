namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Models;
    using Domain.Models;

    public interface IPollingService
    {
        Task<QueryResult<Trip>> GetTripsAfterLoginAsync(string driverId);
        Task<QueryResult<Trip>> GetTripsCanceledAsync(string driverId);
        Task<QueryResult<Trip>> GetTripsUnassignedAsync(string driverId);
        Task<QueryResult<Trip>> GetTripsMarkedDoneAsync(string driverId);
        Task<QueryResult<Trip>> GetTripsResequencedAsync(string driverId);
        Task<QueryResult<ContainerChange>> GetContainerChangesAsync(string terminalId, string regionId, DateTime modifiedAfter);
        Task<QueryResult<TerminalChange>> GetTerminalChangesAsync(string areaId, string regionId, DateTime modifiedAfter, string defSendOnlyYardsForArea);
        Task<QueryResult<DriverStatus>> GetForceLogoffMessageAsync(string driverId);
        Task<QueryResult<Messages>> GetMessagesAsync(string driverId);
    }
}