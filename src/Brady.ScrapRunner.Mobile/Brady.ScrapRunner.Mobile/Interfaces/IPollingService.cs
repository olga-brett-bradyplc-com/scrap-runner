namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;

    public interface IPollingService
    {
        Task PollForChangesAsync(string driverId, string terminalId, string regionId, string areaId);
    }
}