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

        Task<ChangeResultWithItem<DriverEnrouteProcess>> SetDriverEnroute(DriverEnrouteProcess driverEnrouteProcess);

        Task<ChangeResultWithItem<DriverArriveProcess>> SetDriverArrived(DriverArriveProcess driverArriveProcess);

        Task<DriverStatusModel> GetCurrentDriverStatus();
    }
}
