namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using MvvmCross.Plugins.Location;

    public interface ILocationService
    {
        void Start();

        void Stop();

        MvxGeoLocation CurrentLocation { get; }
    }
}
