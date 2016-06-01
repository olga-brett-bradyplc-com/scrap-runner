namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Models;
    using BWF.DataServices.Metadata;
    using BWF.DataServices.Metadata.Interfaces;
    using BWF.DataServices.Metadata.Models;
    using BWF.DataServices.PortableClients;
    using BWF.DataServices.PortableClients.Interfaces;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;
    using Newtonsoft.Json;

    public class QueuedDataServiceClient : IDataServiceClient
    {
        private readonly INetworkAvailabilityService _networkAvailabilityService;
        private readonly IQueueService _queueService;
        private readonly DataServiceClient _dataServiceClient;

        public QueuedDataServiceClient(
            INetworkAvailabilityService networkAvailabilityService, 
            IQueueService queueService, 
            DataServiceClient dataServiceClient)
        {
            _networkAvailabilityService = networkAvailabilityService;
            _queueService = queueService;
            _dataServiceClient = dataServiceClient;
        }

        public void Dispose()
        {
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

        public Task<ChangeSetResult<Tid>> ProcessChangeSetAsync<Tid, Titem>(ChangeSet<Tid, Titem> changeSet, string dataService = null) where Titem : IHaveId<Tid>
        {
            return _dataServiceClient.ProcessChangeSetAsync(changeSet, dataService);
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
                try
                {
                    return await _dataServiceClient.CreateAsync(item, dataService, requeryCreated);
                }
                catch (Exception e)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, e.Message);
                }
            }
            var queueItem = CreateQueueItemByObject(item, QueueItemVerb.Create, dataService);
            // The actual id is rarely set at this point, but we can probably extract the type of the id so it's easier to build a ChangeSet.
            SetQueueItemIdFromObject(item, queueItem);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResultWithItem<T> { WasSuccessful = true };
        }

        public async Task<ChangeResultWithItem<T>> UpdateAsync<T>(T item, string dataService = null, bool requeryUpdated = true)
        {
            if (IsConnected())
            {
                try
                {
                    return await _dataServiceClient.UpdateAsync(item, dataService, requeryUpdated);
                }
                catch (Exception e)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, e.Message);
                }
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
                try
                {
                    return await _dataServiceClient.DeleteAsync<Tid, Titem>(id, dataService);
                }
                catch (Exception e)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, e.Message);
                }
            }
            var queueItem = CreateQueueItemById(id, QueueItemVerb.Delete, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResult { WasSuccessful = true };
        }

        public async Task<ChangeResult> DeleteAsync<Titem>(long id, string dataService = null)
        {
            if (IsConnected())
            {
                try
                {
                    return await _dataServiceClient.DeleteAsync<Titem>(id, dataService);
                }
                catch (Exception e)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, e.Message);
                }
            }
            var queueItem = CreateQueueItemById(id, QueueItemVerb.Delete, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResult { WasSuccessful = true };
        }

        public async Task<ChangeResult> DeleteAsync<Titem>(int id, string dataService = null)
        {
            if (IsConnected())
            {
                try
                {
                    return await _dataServiceClient.DeleteAsync<Titem>(id, dataService);
                }
                catch (Exception e)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, e.Message);
                }
            }
            var queueItem = CreateQueueItemById(id, QueueItemVerb.Delete, dataService);
            await _queueService.EnqueueItemAsync(queueItem);
            return new ChangeResult { WasSuccessful = true };
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

        private QueueItemModel CreateQueueItemByObject<T>(T item, QueueItemVerb verb, string dataService = null)
        {
            var queueItem = new QueueItemModel
            {
                Verb = verb,
                DataService = dataService,
                RecordType = item.GetType().AssemblyQualifiedName,
                SerializedRecord = JsonConvert.SerializeObject(item)
            };
            SetQueueItemIdFromObject(item, queueItem);
            return queueItem;
        }

        private QueueItemModel CreateQueueItemById<TId>(TId id, QueueItemVerb verb, string dataService = null)
        {
            var queueItem = new QueueItemModel
            {
                Verb = verb,
                DataService = dataService,
                IdType = id.GetType().AssemblyQualifiedName,
                SerializedId = JsonConvert.SerializeObject(id)
            };
            return queueItem;
        }
        
        public string HostAddress => "https://maunb-jtw10.bradyplc.com:7776";

        public Task<TypeMetadata[]> GetMetadataForTypeAsync(string type, string dataService = null)
        {
            throw new NotImplementedException();
        }
    }
}