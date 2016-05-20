using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    /// <summary>
    /// Service for the various driver tables/processes
    /// </summary>
    public interface IDriverService
    {
        Task UpdateDriverStatus(DriverStatus driverStatus);

        Task<ChangeResultWithItem<DriverEnrouteProcess>> SetDriverEnrouteRemoteAsync(DriverEnrouteProcess driverEnrouteProcess);

        Task<ChangeResultWithItem<DriverArriveProcess>> SetDriverArrivedRemoteAsync(DriverArriveProcess driverArriveProcess);

        Task<ChangeResultWithItem<DriverFuelEntryProcess>> SetFuelEntryRemoteAsync(DriverFuelEntryProcess driverFuelEntryProcess);

        Task<ChangeResultWithItem<DriverMessageProcess>> SendMessageRemoveAsync(DriverMessageProcess driverMessageEntryProcess);

        Task<DriverStatusModel> GetCurrentDriverStatusAsync();
    }
}
