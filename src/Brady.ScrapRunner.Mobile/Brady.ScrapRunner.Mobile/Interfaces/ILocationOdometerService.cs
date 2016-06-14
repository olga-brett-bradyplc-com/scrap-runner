namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface ILocationOdometerService
    {
        void Start(double startingOdometer);
        void Stop();

        double? CurrentOdometer { get; }
    }
}