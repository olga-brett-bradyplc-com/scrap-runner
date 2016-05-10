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
    using Newtonsoft.Json;

    public class QueuedDataServiceClient : IDataServiceClient
    {
        private readonly INetworkAvailabilityService _networkAvailabilityService;
        private readonly IQueueService _queueService;
        private readonly IDataServiceClient _dataServiceClient;

        public QueuedDataServiceClient(string hosturl, string username, string password, string dataService = null)
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

        #region IDataServiceClient wrapper methods. These methods do not use the queue.

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

        public Task<Titem> GetAsync<Tid, Titem>(Tid id, string dataService = null)
        {
            return _dataServiceClient.GetAsync<Tid, Titem>(id, dataService);
        }

        public Task<Titem> GetAsync<Titem>(long id, string dataService = null)
        {
            return _dataServiceClient.GetAsync<Titem>(id, dataService);
        }

        public Task<Titem> GetAsync<Titem>(int id, string dataService = null)
        {
            return _dataServiceClient.GetAsync<Titem>(id, dataService);
        }

        public Task<QueryResult> QueryAsync(string query, string dataService = null)
        {
            return _dataServiceClient.QueryAsync(query, dataService);
        }

        public Task<QueryResult<T>> QueryAsync<T>(QueryBuilder<T> queryBuilder, string dataService = null)
        {
            return _dataServiceClient.QueryAsync(queryBuilder, dataService);
        }
        #endregion

        private bool IsConnected()
        {
            return _networkAvailabilityService.IsNetworkConnectionAvailable();
        }

        public async Task<ChangeResultWithItem<T>> CreateAsync<T>(T item, string dataService = null, bool requeryCreated = true)
        {
            if (IsConnected())
            {
                var response = await _dataServiceClient.CreateAsync(item, dataService, requeryCreated);
                if (response.WasSuccessful) return response;
            }
            var queueItem = CreateQueueItemByObject(item, QueueItemVerb.Create, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResultWithItem<T> { WasSuccessful = true };
        }

        public async Task<ChangeResultWithItem<T>> UpdateAsync<T>(T item, string dataService = null, bool requeryUpdated = true)
        {
            if (IsConnected())
            {
                var response = await _dataServiceClient.UpdateAsync(item, dataService, requeryUpdated);
                if (response.WasSuccessful) return response;
            }
            var queueItem = CreateQueueItemByObject(item, QueueItemVerb.Update, dataService);
            SetQueueItemIdFromObject(item, queueItem);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResultWithItem<T> { WasSuccessful = true };
        }

        public async Task<ChangeResult> DeleteAsync<Tid, Titem>(Tid id, string dataService = null)
        {
            if (IsConnected())
            {
                var response = await _dataServiceClient.DeleteAsync<Tid, Titem>(id, dataService);
                if (response.WasSuccessful) return response;
            }
            var queueItem = CreateQueueItemById(id, QueueItemVerb.Delete, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResult { WasSuccessful = true };
        }

        public async Task<ChangeResult> DeleteAsync<Titem>(long id, string dataService = null)
        {
            if (IsConnected())
            {
                var response = await _dataServiceClient.DeleteAsync<Titem>(id, dataService);
                if (response.WasSuccessful) return response;
            }
            var queueItem = CreateQueueItemById(id, QueueItemVerb.Delete, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResult { WasSuccessful = true };
        }

        public async Task<ChangeResult> DeleteAsync<Titem>(int id, string dataService = null)
        {
            if (IsConnected())
            {
                var response = await _dataServiceClient.DeleteAsync<Titem>(id, dataService);
                if (response.WasSuccessful) return response;
            }
            var queueItem = CreateQueueItemById(id, QueueItemVerb.Delete, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResult { WasSuccessful = true };
        }

        public async Task<ChangeSetResult<Tid>> ProcessChangeSetAsync<Tid, Titem>(ChangeSet<Tid, Titem> changeSet, string dataService = null) where Titem : IHaveId<Tid>
        {
            if (IsConnected())
            {
                var response = await _dataServiceClient.ProcessChangeSetAsync(changeSet, dataService);
                var failures = response.FailedCreates?.Count + 
                               response.FailedDeletions?.Count +
                               response.FailedUpdates?.Count;
                if (failures == 0) return response;
            }
            var queueItem = CreateQueueItemByChangeSet(changeSet, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeSetResult<Tid>();
        }

        private void SetQueueItemIdFromObject<T>(T item, QueueItemModel queueItem)
        {
            var haveIdString = item as IHaveId<string>;
            if (haveIdString != null)
            {
                queueItem.IdType = typeof(string).AssemblyQualifiedName;
                queueItem.SerializedId = JsonConvert.SerializeObject(haveIdString.Id);
                return;
            }
            var haveIdInt = item as IHaveId<int>;
            if (haveIdInt != null)
            {
                queueItem.IdType = typeof(int).AssemblyQualifiedName;
                queueItem.SerializedId = JsonConvert.SerializeObject(haveIdInt.Id);
                return;
            }
            var haveIdLong = item as IHaveId<long>;
            if (haveIdLong != null)
            {
                queueItem.IdType = typeof(long).AssemblyQualifiedName;
                queueItem.SerializedId = JsonConvert.SerializeObject(haveIdLong.Id);
                return;
            }
        }

        private void SetQueueItemId<Tid>(Tid id, QueueItemModel queueItem)
        {
            queueItem.IdType = id.GetType().AssemblyQualifiedName;
            queueItem.SerializedId = JsonConvert.SerializeObject(id);
        }

        private QueueItemModel CreateQueueItemByObject<T>(T item, QueueItemVerb verb, string dataService = null)
        {
            var queueItem = new QueueItemModel
            {
                RecordType = item.GetType().AssemblyQualifiedName,
                SerializedRecord = JsonConvert.SerializeObject(item),
                Verb = verb,
                DataService = dataService
            };
            SetQueueItemIdFromObject(item, queueItem);
            return queueItem;
        }

        private QueueItemModel CreateQueueItemById<TId>(TId id, QueueItemVerb verb, string dataService = null)
        {
            var queueItem = new QueueItemModel
            {
                Verb = verb,
                DataService = dataService
            };
            SetQueueItemId(id, queueItem);
            return queueItem;
        }

        private QueueItemModel CreateQueueItemByChangeSet<Tid, Titem>(ChangeSet<Tid, Titem> changeSet, string dataService = null) where Titem : IHaveId<Tid>
        {
            var queueItem = new QueueItemModel
            {
                Verb = QueueItemVerb.ChangeSet,
                DataService = dataService,
                RecordType = changeSet.GetType().AssemblyQualifiedName,
                SerializedRecord = JsonConvert.SerializeObject(changeSet)
            };
            return queueItem;
        }
    }
}