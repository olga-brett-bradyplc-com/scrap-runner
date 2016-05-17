namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Threading.Tasks;
    using BWF.DataServices.Metadata.Models;
    using Interfaces;
    using Models;
    using Newtonsoft.Json;

    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueItemModel> _repository;
        private readonly IConnectionService _connectionService;

        public QueueService(
            IRepository<QueueItemModel> repository, 
            IConnectionService connectionService)
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
            var client = _connectionService.GetConnection();
            var queueItems = await _repository.AsQueryable().ToListAsync();
            foreach (var queueItem in queueItems)
            {
                var success = false;
                switch (queueItem.Verb)
                {
                    case QueueItemVerb.Create:
                        dynamic createItem = JsonConvert.DeserializeObject(queueItem.SerializedRecord,
                            Type.GetType(queueItem.RecordType));
                        var createResult = await client.CreateAsync(createItem, queueItem.DataService, false);
                        var createChangeResult = createResult as ChangeResult;
                        if (createChangeResult != null) success = createChangeResult.WasSuccessful;
                        break;
                    case QueueItemVerb.Update:
                        dynamic updateItem = JsonConvert.DeserializeObject(queueItem.SerializedRecord,
                            Type.GetType(queueItem.RecordType));
                        var updateResult = await client.UpdateAsync(updateItem, queueItem.DataService, false);
                        var updateChangeResult = updateResult as ChangeResult;
                        if (updateChangeResult != null) success = updateChangeResult.WasSuccessful;
                        break;
                    case QueueItemVerb.Delete:
                        dynamic deleteId = JsonConvert.DeserializeObject(queueItem.SerializedId,
                            Type.GetType(queueItem.IdType));
                        var deleteResult = await client.DeleteAsync<dynamic>(deleteId, queueItem.DataService);
                        var deleteChangeResult = deleteResult as ChangeResult;
                        if (deleteChangeResult != null) success = deleteChangeResult.WasSuccessful;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (success) await _repository.DeleteAsync(queueItem);
            }
        }

        public async Task<bool> IsEmptyAsync()
        {
            return await _repository.AsQueryable().CountAsync() > 0;
        }
    }
}