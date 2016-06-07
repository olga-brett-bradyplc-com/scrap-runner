namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using Models;

    public interface ILocationService
    {
        void Start();

        void Stop();

        LocationModel CurrentLocation { get; }
    }
}
