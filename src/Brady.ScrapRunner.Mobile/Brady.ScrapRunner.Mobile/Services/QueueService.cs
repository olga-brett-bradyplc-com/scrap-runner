namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Interfaces;
    using BWF.DataServices.Metadata.Models;
    using BWF.DataServices.PortableClients.Interfaces;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;
    using Newtonsoft.Json;

    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueItemModel> _repository;
        private readonly IConnectionService<QueuedDataServiceClient> _connectionService;

        public QueueService(
            IRepository<QueueItemModel> repository, 
            IConnectionService<QueuedDataServiceClient> connectionService)
        {
            _repository = repository;
            _connectionService = connectionService;
        }

        public Task EnqueueItemAsync(QueueItemModel queueItem)
        {
            return _repository.InsertAsync(queueItem);
        }

        public async Task ProcessQueueAsync()
        {
            var queueItem = await _repository.AsQueryable().FirstOrDefaultAsync();
            if (queueItem == null) return;
            var client = _connectionService.GetConnection().DataServiceClient;
            var success = false;
            switch (queueItem.Verb)
            {
                case QueueItemVerb.Create:
                    var createResult = await CreateAsync(client, queueItem);
                    success = createResult.WasSuccessful;
                    Mvx.Trace("QueueService.CreateAsync");
                    break;
                case QueueItemVerb.Update:
                    var updateResult = await UpdateAsync(client, queueItem);
                    success = updateResult.WasSuccessful;
                    Mvx.Trace("QueueService.UpdateAsync");
                    break;
                case QueueItemVerb.Delete:
                    // @TODO: Implement.
                    break;
                case QueueItemVerb.ChangeSet:
                    var changeSetResult = await ProcessChangeSetAsync(client, queueItem);
                    // @TODO: How to determine if IChangeSetResult succeeded or failed?
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (success)
                await _repository.DeleteAsync(queueItem);
        }

        public async Task<bool> IsEmptyAsync()
        {
            return await _repository.AsQueryable().CountAsync() > 0;
        }

        private Task<ChangeResult> CreateAsync(IDataServiceClient client, QueueItemModel queueItem)
        {
            var objectType = Type.GetType(queueItem.RecordType);
            dynamic dynamicObject = JsonConvert.DeserializeObject(queueItem.SerializedRecord, objectType);
            return client.CreateAsync(dynamicObject, queueItem.DataService, false);
        }

        private Task<ChangeResult> UpdateAsync(IDataServiceClient client, QueueItemModel queueItem)
        {
            var objectType = Type.GetType(queueItem.RecordType);
            dynamic dynamicObject = JsonConvert.DeserializeObject(queueItem.SerializedRecord, objectType);
            return client.UpdateAsync(dynamicObject, queueItem.DataService, false);
        }

        //private Task<ChangeResult> DeleteAsync(IDataServiceClient client, QueueItemModel queueItem)
        //{
        //    var idType = Type.GetType(queueItem.IdType);
        //    dynamic dynamicId = JsonConvert.DeserializeObject(queueItem.SerializedId, idType);
        //    return client.DeleteAsync(dynamicId, queueItem.DataService);
        //}

        private Task<IChangeSetResult> ProcessChangeSetAsync(IDataServiceClient client, QueueItemModel queueItem)
        {
            var objectType = Type.GetType(queueItem.SerializedRecord);
            dynamic changeSet = JsonConvert.DeserializeObject(queueItem.SerializedRecord, objectType);
            return client.ProcessChangeSetAsync(changeSet, queueItem.DataService);
        }
    }
}