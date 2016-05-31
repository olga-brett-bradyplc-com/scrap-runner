namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;
    using Models;

    public interface IQueueService
    {
        Task EnqueueItemAsync(QueueItemModel queueItem);
        Task ProcessQueueAsync();
        Task<bool> IsEmptyAsync();
    }
}