namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface ILocationGeofenceService
    {
        void Start();
        void Stop();
        void StartAutoArrive(string key, int synergyLatitude, int synergyLongitude, int radius);
        void StartAutoDepart(string key, int radius);
    }
}
