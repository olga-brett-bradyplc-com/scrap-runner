namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;

    public interface ILocationGeofenceService
    {
        void StartAutoArrive(string key, int synergyLatitude, int synergyLongitude, short radius);
        void StartAutoDepart(string key);
    }
}
