namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;

    public interface IQueueService
    {
        Task ProcessQueueAsync();
        Task<bool> IsEmptyAsync();
    }
}