using System.Collections.Generic;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Interfaces;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using MvvmCross.Core.ViewModels;
    using Resources;
    using Acr.UserDialogs;
    using Models;

    public class GpsCaptureViewModel : BaseViewModel
    {
        // This is a dependency.
        private readonly ILocationService _locationService;
        private readonly ITripService _tripService;

        // Inject into ViewModel using constructor injection.  In other words, require the creator of the object to pass in the dependencies.
        public GpsCaptureViewModel(ILocationService locationService, ITripService tripService)
        {
            _locationService = locationService;
            _tripService = tripService;
            Title = AppResources.GPSCapture;
            CaptureCommand = new MvxCommand(ExecuteCaptureCommand);
            SkipCommand = new MvxCommand(ExecuteSkipCommand);
        }
        public void Init(string custHostCode, string customerInfo)
        {
            CustHostCode = custHostCode;
            CustomerInfoText = customerInfo;
        }
        public override void Start()
        {
            CustomerInfoText = CustomerInfoText;

            GetCurrentLocation();

            base.Start();
        }

        private string _gpsStatusText;
        private string _gpsCustomerInfo;
        private string _custHostCode;

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

        public string CustHostCode
        {
            get { return _custHostCode; }
            set
            {
                SetProperty(ref _custHostCode, value);
            }
        }

        private List<TripSegmentModel> Segments { get; set; }
        public MvxCommand CaptureCommand { get; protected set; }
        public MvxCommand SkipCommand { get; protected set; }
        private void GetCurrentLocation()
        {
            var currentLocation = _locationService.CurrentLocation;
            if (currentLocation == null)
            {
                // GPS is temporarily unavailable – warn the user
                GpsStatusText = AppResources.NoGpsAvailable;
                UserDialogs.Instance.Alert(GpsStatusText, AppResources.Error);
                return;
            }
            GpsStatusText = AppResources.GpsAvailableStatus;
        }

        protected async void ExecuteCaptureCommand()
        {
            var currentLocation = _locationService.CurrentLocation;
            // Now we need to convert from standard decimal degrees GPS to synergy coordinates before sending to the server.
            var synergyLatitude = currentLocation.Latitude * 600000;
            var synergyLongitude = currentLocation.Longitude * 600000;

            // Send to the server and update local stop database latitude/longitude.
            Segments = await _tripService.FindCustomerSegments(CustHostCode);
            await _tripService.UpdateTripSegments(Segments);
            Close(this);
        }
        protected async void ExecuteSkipCommand()
        {
            var currentLocation = _locationService.CurrentLocation;
            if (currentLocation != null)
            {
                var message = string.Format(AppResources.GpsCancelConfirm);
                var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmEnrouteTitle);
                if (confirm)
                {
                    Close(this);
                }
            }
        }
    }
}





