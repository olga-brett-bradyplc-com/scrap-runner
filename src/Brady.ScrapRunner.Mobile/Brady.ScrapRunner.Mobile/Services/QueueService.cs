namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Threading.Tasks;
    using BWF.DataServices.PortableClients.Interfaces;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Platform;
    using Newtonsoft.Json;

    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueItemModel> _repository;
        private readonly IMvxJsonConverter _jsonConverter;
        private readonly IConnectionService<OfflineCapableDataServiceClient> _connectionService;

        public QueueService(
            IRepository<QueueItemModel> repository, 
            IMvxJsonConverter jsonConverter,
            IConnectionService<OfflineCapableDataServiceClient> connectionService)
        {
            _repository = repository;
            _jsonConverter = jsonConverter;
            _connectionService = connectionService;
        }

        public Task InsertQueueItemAsync<T>(T obj, QueueItemVerb verb, string dataService)
        {
            var queueItem = new QueueItemModel
            {
                RecordType = obj.GetType().ToString(),
                SerializedRecord = _jsonConverter.SerializeObject(obj),
                Verb = verb,
                DataService = dataService
            };
            return _repository.InsertAsync(queueItem);
        }

        public async Task ProcessQueueAsync()
        {
            Mvx.Trace("ProcessQueueAsync started");
            var onlineConnection = _connectionService.GetConnection().DataServiceClient;
            var queueItemList = await _repository.AsQueryable().ToListAsync();
            foreach (var queueItem in queueItemList)
            {
                Mvx.Trace($"Processing QueueItem {queueItem.RecordId}");
                dynamic json = JsonConvert.DeserializeObject(queueItem.SerializedRecord);
                switch (queueItem.Verb)
                {
                    case QueueItemVerb.Create:
                        var createResult = await CreateAsync(onlineConnection, json, queueItem.DataService);
                        if (createResult)
                            await DeleteQueueItem(queueItem);
                        break;
                    case QueueItemVerb.Update:
                        var updateResult = await UpdateAsync(onlineConnection, json, queueItem.DataService);
                        if (updateResult)
                            await DeleteQueueItem(queueItem);
                        break;
                    case QueueItemVerb.Delete:
                        // @TODO: Implement
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            Mvx.Trace("ProcessQueueAsync stopped");
        }

        private async Task<bool> CreateAsync<T>(IDataServiceClient dataServiceClient, T item, string dataService)
        {
            var response = await dataServiceClient.CreateAsync(item, dataService, false);
            return response.WasSuccessful;
        }

        private async Task<bool> UpdateAsync<T>(IDataServiceClient dataServiceClient, T item, string dataService)
        {
            var response = await dataServiceClient.UpdateAsync(item, dataService, false);
            return response.WasSuccessful;
        }

        private Task DeleteQueueItem(QueueItemModel queueItem)
        {
            return _repository.DeleteAsync(queueItem);
        }

        public async Task<bool> IsEmptyAsync()
        {
            return await _repository.AsQueryable().CountAsync() > 0;
        }
    }
}
