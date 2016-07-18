using Brady.ScrapRunner.Mobile.Interfaces;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class GpsCaptureViewModel : BaseViewModel
    {
        // This is a dependency.
        private readonly ILocationService _locationService;

        // Inject into ViewModel using constructor injection.  In other words, require the creator of the object to pass in the dependencies.
        public GpsCaptureViewModel(ILocationService locationService)
        {
            _locationService = locationService;
            Title = AppResources.GPSCapture;
            SkipCommand = new MvxCommand(ExecuteSkipCommand);
        }
        public void Init(string customerInfo, string status)
        {
            GpsStatusText = status;
            CustomerInfoText = customerInfo;
        }

        private string _gpsStatusText;
        private string _gpsCustomerInfo;

        public string GpsStatusText
        {
            get { return _gpsStatusText; }
            set
            {
                SetProperty(ref _gpsStatusText, value);
            }
        }
        public string CustomerInfoText
        {
            get { return _gpsCustomerInfo; }
            set
            {
                SetProperty(ref _gpsCustomerInfo, value);
            }
        }
        public MvxCommand SkipCommand { get; protected set; }
        private void GetCurrentLocation()
        {
            var currentLocation = _locationService.CurrentLocation;
            if (currentLocation == null)
            {
                // GPS is temporarily unavailable – warn the user?
                return;
            }
            // Now we need to convert from standard decimal degrees GPS to synergy coordinates before sending to the server.
            var synergyLatitude = currentLocation.Latitude * 600000;
            var synergyLongitude = currentLocation.Longitude * 600000;
            // @TODO: Send to the server and update local stop database latitude/longitude.
        }

        protected void ExecuteSkipCommand()
        {
            Close(this);
        }
    }
}


