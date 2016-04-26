using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Plugins.Sqlite;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class DbService : IDbService
    {
        private readonly IMvxSqliteConnectionFactory _sqliteConnectionFactory;

        public DbService(IMvxSqliteConnectionFactory sqliteConnectionFactory)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
        }

        /// <summary>
        /// Delete, Create tables. 
        /// @TODO : We're probably going to want a finer grained interface than what's here currently. Refactor when needed/appropiate
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAll()
        {
            var asyncConnection = _sqliteConnectionFactory.GetAsyncConnection("scraprunner");

            await asyncConnection.DropTableAsync<ContainerMasterModel>();
            await asyncConnection.DropTableAsync<CustomerDirectionsModel>();
            await asyncConnection.DropTableAsync<DriverStatusModel>();
            await asyncConnection.DropTableAsync<EmployeeMasterModel>();
            await asyncConnection.DropTableAsync<PowerMasterModel>();
            await asyncConnection.DropTableAsync<PreferenceModel>();
            await asyncConnection.DropTableAsync<TripModel>();
            await asyncConnection.DropTableAsync<TripSegmentModel>();
            await asyncConnection.DropTableAsync<TripSegmentContainerModel>();
            await asyncConnection.DropTableAsync<CustomerCommodityModel>();
            await asyncConnection.DropTableAsync<CustomerLocationModel>();

            await asyncConnection.CreateTableAsync<ContainerMasterModel>();
            await asyncConnection.CreateTableAsync<CustomerDirectionsModel>();
            await asyncConnection.CreateTableAsync<DriverStatusModel>();
            await asyncConnection.CreateTableAsync<EmployeeMasterModel>();
            await asyncConnection.CreateTableAsync<PowerMasterModel>();
            await asyncConnection.CreateTableAsync<PreferenceModel>();
            await asyncConnection.CreateTableAsync<TripModel>();
            await asyncConnection.CreateTableAsync<TripSegmentModel>();
            await asyncConnection.CreateTableAsync<TripSegmentContainerModel>();
            await asyncConnection.CreateTableAsync<CustomerCommodityModel>();
            await asyncConnection.CreateTableAsync<CustomerLocationModel>();
            await asyncConnection.CreateTableAsync<QueueItemModel>();

            //await asyncConnection.DeleteAllAsync<ContainerMasterModel>();
            //await asyncConnection.DeleteAllAsync<CustomerDirectionsModel>();
            //await asyncConnection.DeleteAllAsync<DriverStatusModel>();
            //await asyncConnection.DeleteAllAsync<EmployeeMasterModel>();
            //await asyncConnection.DeleteAllAsync<PowerMasterModel>();
            //await asyncConnection.DeleteAllAsync<PreferenceModel>();
            //await asyncConnection.DeleteAllAsync<TripModel>();
            //await asyncConnection.DeleteAllAsync<TripSegmentModel>();
            //await asyncConnection.DeleteAllAsync<TripSegmentContainerModel>();
            //await asyncConnection.DeleteAllAsync<CustomerCommodityModel>();
            //await asyncConnection.DeleteAllAsync<CustomerLocationModel>();
        }
        
    }
}
