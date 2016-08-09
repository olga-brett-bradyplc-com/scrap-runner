namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface ILocationGeofenceService
    {
        void Start();
        void Stop();
        void StartAutoArrive(string key, int synergyLatitude, int synergyLongitude, short radius);
        void StartAutoDepart(string key, short radius);
    }
}
