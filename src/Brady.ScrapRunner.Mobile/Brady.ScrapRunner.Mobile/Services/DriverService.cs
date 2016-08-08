using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class DriverService : IDriverService
    {
        private readonly IConnectionService _connection;
        private readonly IRepository<DriverStatusModel> _driverStatusRepository;
        private readonly IRepository<EmployeeMasterModel> _employeeMasterRepository; 

        public DriverService(IRepository<DriverStatusModel> driverStatusRepository,
            IRepository<EmployeeMasterModel> employeeMasterRepository,
            IConnectionService connection)
        {
            _driverStatusRepository = driverStatusRepository;
            _employeeMasterRepository = employeeMasterRepository;
            _connection = connection;
        }

        /// <summary>
        /// Update the local DriverStatus SQLite table from the provided DriverStatus object
        /// </summary>
        /// <param name="driverStatus"></param>
        /// <returns></returns>
        public Task UpdateDriverStatus(DriverStatus driverStatus)
        {
            var mapped = AutoMapper.Mapper.Map<DriverStatus, DriverStatusModel>(driverStatus);
            return _driverStatusRepository.InsertAsync(mapped);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverStatus"></param>
        /// <returns></returns>
        public async Task<int> CreateDriverStatus(DriverStatusModel driverStatus)
        {
            return await _driverStatusRepository.InsertOrReplaceAsync(driverStatus);
        }

        /// <summary>
        /// Update the local EmployeeMaster SQLite table with current user information
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        public Task UpdateDriverEmployeeRecord(EmployeeMaster employee)
        {
            var mapped = AutoMapper.Mapper.Map<EmployeeMaster, EmployeeMasterModel>(employee);
            return _employeeMasterRepository.InsertAsync(mapped);
        }

        /// <summary>
        /// Find the remote EmployeeMaster record for the current user
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<EmployeeMaster> FindEmployeeMasterForDriverRemoteAsync(string employeeId)
        {
            var driver =
                await
                    _connection.GetConnection(ConnectionType.Online)
                        .QueryAsync(
                            new QueryBuilder<EmployeeMaster>().Filter(
                                e => e.Property(f => f.EmployeeId).EqualTo(employeeId)));
            return driver.Records.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public async Task<int> UpdateDriver(DriverStatusModel driver)
        {
            return await _driverStatusRepository.UpdateAsync(driver);
        }

        /// <summary>
        /// Return current driver status
        /// Right now, making an assumption that there will only ever be one record in the local DriverStatus SQLite table
        /// </summary>
        /// <returns></returns>
        public async Task<DriverStatusModel> GetCurrentDriverStatusAsync()
        {
            var driver = await _driverStatusRepository.AllAsync();
            return driver.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverArriveProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverArriveProcess>> ProcessDriverArrivedAsync(DriverArriveProcess driverArriveProcess)
        {
            var arriveProcess = await _connection.GetConnection().UpdateAsync(driverArriveProcess, requeryUpdated: false);
            return arriveProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverEnrouteProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverEnrouteProcess>> ProcessDriverEnrouteAsync(DriverEnrouteProcess driverEnrouteProcess)
        {
            var enrouteProcess = await _connection.GetConnection().UpdateAsync(driverEnrouteProcess, requeryUpdated: false);
            return enrouteProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverDelayProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverDelayProcess>> ProcessDriverDelayAsync(DriverDelayProcess driverDelayProcess)
        {
            var driverDelay = await _connection.GetConnection().UpdateAsync(driverDelayProcess, requeryUpdated: false);
            return driverDelay;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverFuelEntryProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverFuelEntryProcess>> ProcessDriverFuelEntryAsync(DriverFuelEntryProcess driverFuelEntryProcess)
        {
            var fuelEntry = await _connection.GetConnection().UpdateAsync(driverFuelEntryProcess, requeryUpdated: false);
            return fuelEntry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverMessageEntryProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverMessageProcess>> ProcessDriverMessageAsync(DriverMessageProcess driverMessageEntryProcess)
        {
            var message = await _connection.GetConnection().UpdateAsync(driverMessageEntryProcess, requeryUpdated: false);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverOdomUpdateProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverOdomUpdateProcess>> ProcessDriverOdomUpdateAsync(DriverOdomUpdateProcess driverOdomUpdateProcess)
        {
            var odomupdate =
                await _connection.GetConnection().UpdateAsync(driverOdomUpdateProcess, requeryUpdated: false);
            return odomupdate;
        }

        public async Task<DateTime?> GetTerminalMasterDateTimeAsync()
        {
            var driverStatus = await _driverStatusRepository.AsQueryable().FirstOrDefaultAsync();
            return driverStatus?.TerminalMasterDateTime;
        }

        public async Task<int> UpdateTerminalMasterDateTimeAsync(DateTime? terminalMasterDateTime)
        {
            var driverStatus = await _driverStatusRepository.AsQueryable().FirstOrDefaultAsync();
            if (driverStatus == null) return 0;
            driverStatus.TerminalMasterDateTime = terminalMasterDateTime;
            return await _driverStatusRepository.UpdateAsync(driverStatus);
        }

        public async Task<DateTime?> GetContainerMasterDateTimeAsync()
        {
            var driverStatus = await _driverStatusRepository.AsQueryable().FirstOrDefaultAsync();
            return driverStatus?.ContainerMasterDateTime;
        }

        public async Task<int> UpdateContainerMasterDateTimeAsync(DateTime? containerMasterDateTime)
        {
            var driverStatus = await _driverStatusRepository.AsQueryable().FirstOrDefaultAsync();
            if (driverStatus == null) return 0;
            driverStatus.ContainerMasterDateTime = containerMasterDateTime;
            return await _driverStatusRepository.UpdateAsync(driverStatus);
        }
    }
}