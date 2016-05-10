using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IConnectionService<DataServiceClient> _connection;
        private readonly IRepository<DriverStatusModel> _driverStatusRepository;

        public DriverService(IRepository<DriverStatusModel> driverStatusRepository, 
            IConnectionService<DataServiceClient> connection)
        {
            _driverStatusRepository = driverStatusRepository;
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
        /// <param name="driverEnrouteProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverEnrouteProcess>> SetDriverEnroute(DriverEnrouteProcess driverEnrouteProcess)
        {
            var enrouteProcess = await _connection.GetConnection().UpdateAsync(driverEnrouteProcess, requeryUpdated: false);
            return enrouteProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverArriveProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverArriveProcess>> SetDriverArrived(DriverArriveProcess driverArriveProcess)
        {
            var arriveProcess = await _connection.GetConnection().UpdateAsync(driverArriveProcess, requeryUpdated: false);
            return arriveProcess;
        }

        /// <summary>
        /// Return current driver status
        /// Right now, making an assumption that there will only ever be one record in the local
        ///     DriverStatus SQLite table
        /// </summary>
        /// <returns></returns>
        public async Task<DriverStatusModel> GetCurrentDriverStatus()
        {
            var driver = await _driverStatusRepository.AllAsync();
            return driver.FirstOrDefault();
        }

        public async Task<ChangeResultWithItem<DriverFuelEntryProcess>> SetFuelEntry(
            DriverFuelEntryProcess driverFuelEntryProcess)
        {
            var fuelEntry = await _connection.GetConnection().UpdateAsync(driverFuelEntryProcess, requeryUpdated: false);
            return fuelEntry;
        }
    }
}
