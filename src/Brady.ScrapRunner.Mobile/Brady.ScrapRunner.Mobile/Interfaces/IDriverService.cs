using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System;

    /// <summary>
    /// Service for the various driver tables/processes
    /// </summary>
    public interface IDriverService
    {
        Task UpdateDriverStatus(DriverStatus driverStatus);

        Task UpdateDriverEmployeeRecord(EmployeeMaster employee);

        Task<int> UpdateDriver(DriverStatusModel driver);
        
        Task<DriverStatusModel> GetCurrentDriverStatusAsync();

        Task<EmployeeMaster> FindEmployeeMasterForDriverRemoteAsync(string employeeId);

        Task<ChangeResultWithItem<DriverEnrouteProcess>> ProcessDriverEnrouteAsync(DriverEnrouteProcess driverEnrouteProcess);

        Task<ChangeResultWithItem<DriverArriveProcess>> ProcessDriverArrivedAsync(DriverArriveProcess driverArriveProcess);

        Task<ChangeResultWithItem<DriverDelayProcess>> ProcessDriverDelayAsync(DriverDelayProcess driverDelayProcess);

        Task<ChangeResultWithItem<DriverFuelEntryProcess>> ProcessDriverFuelEntryAsync(DriverFuelEntryProcess driverFuelEntryProcess);

        Task<ChangeResultWithItem<DriverMessageProcess>> ProcessDriverMessageAsync(DriverMessageProcess driverMessageEntryProcess);

        Task<ChangeResultWithItem<DriverOdomUpdateProcess>> ProcessDriverOdomUpdateAsync(DriverOdomUpdateProcess driverOdomUpdateProcess);

        Task<DateTime?> GetTerminalMasterDateTimeAsync();

        Task<int> UpdateTerminalMasterDateTimeAsync(DateTime? terminalMasterDateTime);
    }
}
