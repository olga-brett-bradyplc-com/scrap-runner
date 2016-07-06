using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Plugins.Sqlite;
using SQLite.Net.Async;

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
            await asyncConnection.DropTableAsync<CodeTableModel>();
            await asyncConnection.DropTableAsync<MessagesModel>();
            await asyncConnection.DropTableAsync<YardModel>();
            await asyncConnection.DropTableAsync<TerminalChangeModel>();
            await asyncConnection.DropTableAsync<CustomerMasterModel>();
            //await asyncConnection.DropTableAsync<ContainerMasterModel>();
            //await asyncConnection.DropTableAsync<ContainerChangeModel>();

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
            await asyncConnection.CreateTableAsync<CodeTableModel>();
            await asyncConnection.CreateTableAsync<MessagesModel>();
            await asyncConnection.CreateTableAsync<QueueItemModel>();
            await asyncConnection.CreateTableAsync<YardModel>();
            await asyncConnection.CreateTableAsync<TerminalChangeModel>();
            await asyncConnection.CreateTableAsync<CustomerMasterModel>();

            if (! await TableExists("ContainerChange", asyncConnection) )
                await asyncConnection.CreateTableAsync<ContainerChangeModel>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task RefreshTable<T>() where T : class
        {
            var asyncConnection = _sqliteConnectionFactory.GetAsyncConnection("scraprunner");
            await asyncConnection.DropTableAsync<T>();
            await asyncConnection.CreateTableAsync<T>();
        }

        private async Task<bool> TableExists(string tableName, SQLiteAsyncConnection connection)
        {
            var query = "SELECT * FROM sqlite_master WHERE type='table' AND name=?";
            var cmd = await connection.QueryAsync<SqliteMasterTable>(query, tableName);
            var any = cmd.Any();
            return any;
        }

        // Because QueryAsync doesn't allow us to pass non referenced types for whatever reason ...
        private class SqliteMasterTable
        {
            public string type { get; set; }
            public string name { get; set; }
        }

    }
}
