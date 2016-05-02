namespace Brady.ScrapRunner.Mobile.Services
{
    using System.Threading.Tasks;
    using Interfaces;
    using Models;
    using MvvmCross.Platform.Platform;

    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueItemModel> _repository;
        private readonly IMvxJsonConverter _jsonConverter;

        public QueueService(
            IRepository<QueueItemModel> repository, 
            IMvxJsonConverter jsonConverter)
        {
            _repository = repository;
            _jsonConverter = jsonConverter;
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
            await Task.Delay(0); // Async noop
        }

        public async Task<bool> IsEmptyAsync()
        {
            return await _repository.AsQueryable().CountAsync() > 0;
        }
    }
}
