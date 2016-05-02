namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;
    using Models;

    public interface IQueueService
    {
        Task InsertQueueItemAsync<T>(T obj, QueueItemVerb verb, string dataService);
        Task ProcessQueueAsync();
        Task<bool> IsEmptyAsync();
    }
}