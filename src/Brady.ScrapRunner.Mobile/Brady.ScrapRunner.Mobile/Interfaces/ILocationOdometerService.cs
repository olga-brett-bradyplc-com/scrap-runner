namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface ILocationOdometerService
    {
        void Start(int startingOdometer);
        void Stop();

        int? CurrentOdometer { get; }
    }
}