namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Models;
    using BWF.DataServices.Metadata.Interfaces;
    using BWF.DataServices.Metadata.Models;
    using BWF.DataServices.PortableClients;
    using BWF.DataServices.PortableClients.Interfaces;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;

    public class OfflineCapableDataServiceClient : IDataServiceClient
    {
        private readonly INetworkAvailabilityService _networkAvailabilityService;
        private readonly IQueueService _queueService;
        private readonly IDataServiceClient _dataServiceClient;

        public OfflineCapableDataServiceClient(string hosturl, string username, string password, string dataService = null)
        {
            _networkAvailabilityService = Mvx.Resolve<INetworkAvailabilityService>();
            _queueService = Mvx.Resolve<IQueueService>();
            _dataServiceClient = new DataServiceClient(hosturl, username, password, dataService);
        }

        public IDataServiceClient DataServiceClient => _dataServiceClient;

        public void Dispose()
        {
            _dataServiceClient.Dispose();
        }

        public Task<LiveQueryInstance> StartLiveQueryAsync(string query, Action<IEnumerable<RecordChangeDetails>, int> onChange, string dataService = null)
        {
            return _dataServiceClient.StartLiveQueryAsync(query, onChange, dataService);
        }

        public Task<LiveQueryInstance<T>> StartLiveQueryAsync<T>(QueryBuilder<T> queryBuilder, Action<IEnumerable<RecordChangeDetails>, int> onChange, string dataService = null)
        {
            return _dataServiceClient.StartLiveQueryAsync(queryBuilder, onChange, dataService);
        }

        public Task KillLiveQueryAsync(string queryId)
        {
            return _dataServiceClient.KillLiveQueryAsync(queryId);
        }

        public async Task<ChangeResultWithItem<T>> CreateAsync<T>(T item, string dataService = null, bool requeryCreated = true)
        {
            if (_networkAvailabilityService.IsNetworkConnectionAvailable())
            {
                var response = await _dataServiceClient.CreateAsync(item, dataService, requeryCreated);
                if (response.WasSuccessful) return response;
            }
            await _queueService.InsertQueueItemAsync(item, QueueItemVerb.Create, dataService);
            return new ChangeResultWithItem<T>();
        }

        public async Task<ChangeResultWithItem<T>> UpdateAsync<T>(T item, string dataService = null, bool requeryUpdated = true)
        {
            if (_networkAvailabilityService.IsNetworkConnectionAvailable())
            {
                var response = await _dataServiceClient.UpdateAsync(item, dataService, requeryUpdated);
                if (response.WasSuccessful) return response;
            }
            await _queueService.InsertQueueItemAsync(item, QueueItemVerb.Update, dataService);
            return new ChangeResultWithItem<T>();
        }

        public Task<ChangeResult> DeleteAsync<Tid, Titem>(Tid id, string dataService = null)
        {
            // @TODO: Add Offline support
            return _dataServiceClient.DeleteAsync<Tid, Titem>(id, dataService);
        }

        public Task<ChangeResult> DeleteAsync<Titem>(long id, string dataService = null)
        {
            // @TODO: Add Offline support
            return _dataServiceClient.DeleteAsync<Titem>(id, dataService);
        }

        public Task<ChangeResult> DeleteAsync<Titem>(int id, string dataService = null)
        {
            // @TODO: Add Offline support
            return _dataServiceClient.DeleteAsync<Titem>(id, dataService);
        }

        public Task<ChangeSetResult<Tid>> ProcessChangeSetAsync<Tid, Titem>(ChangeSet<Tid, Titem> changeSet, string dataService = null) where Titem : IHaveId<Tid>
        {
            return _dataServiceClient.ProcessChangeSetAsync(changeSet, dataService);
        }

        public Task<Titem> GetAsync<Tid, Titem>(Tid id, string dataService = null)
        {
            // This method doesn't support offline queueing.
            return _dataServiceClient.GetAsync<Tid, Titem>(id, dataService);
        }

        public Task<Titem> GetAsync<Titem>(long id, string dataService = null)
        {
            // This method doesn't support offline queueing.
            return _dataServiceClient.GetAsync<Titem>(id, dataService);
        }

        public Task<Titem> GetAsync<Titem>(int id, string dataService = null)
        {
            // This method doesn't support offline queueing.
            return _dataServiceClient.GetAsync<Titem>(id, dataService);
        }

        public Task<QueryResult> QueryAsync(string query, string dataService = null)
        {
            // This method doesn't support offline queueing.
            return _dataServiceClient.QueryAsync(query, dataService);
        }

        public Task<QueryResult<T>> QueryAsync<T>(QueryBuilder<T> queryBuilder, string dataService = null)
        {
            // This method doesn't support offline queueing.
            return _dataServiceClient.QueryAsync(queryBuilder, dataService);
        }
    }
}