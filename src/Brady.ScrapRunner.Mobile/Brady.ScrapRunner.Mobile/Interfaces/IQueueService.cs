namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;

    public interface IQueueService
    {
        Task ProcessQueueAsync();
        bool IsEmpty { get; }
    }
}