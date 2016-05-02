namespace Brady.ScrapRunner.Mobile.Services
{
    using System.Threading.Tasks;
    using Interfaces;
    using Models;

    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueItemModel> _repository;

        public QueueService(IRepository<QueueItemModel> repository)
        {
            _repository = repository;
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
