using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
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
        private readonly IDriverService _driverService;

        // Inject into ViewModel using constructor injection.  In other words, require the creator of the object to pass in the dependencies.
        public GpsCaptureViewModel(ILocationService locationService, ITripService tripService, IDriverService driverService)
        {
            _locationService = locationService;
            _tripService = tripService;
            _driverService = driverService;

            Title = AppResources.GPSCapture;
            CaptureCommand = new MvxCommand(ExecuteCaptureCommand);
            SkipCommand = new MvxCommand(ExecuteSkipCommand);
        }
        public void Init(string custHostCode, string customerInfo)
        {
            CustHostCode = custHostCode;
            CustomerInfoText = customerInfo;
        }
        public override async void Start()
        {
            GetCurrentLocation();
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

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
        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
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
            var synergyLatitude = currentLocation.Latitude*600000;
            var synergyLongitude = currentLocation.Longitude*600000;

            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var setDriverArrived = await _driverService.ProcessDriverArrivedAsync(new DriverArriveProcess
            {
                EmployeeId = CurrentDriver.EmployeeId,
                PowerId = CurrentDriver.PowerId,
                TripNumber = CurrentDriver.TripNumber,
                TripSegNumber = CurrentDriver.TripSegNumber,
                ActionDateTime = DateTime.Now,
                Odometer = CurrentDriver.Odometer ?? default(int),
                Latitude = (int) synergyLatitude,
                Longitude = (int) synergyLongitude
            });

            // Send to the server and update local stop database latitude/longitude.
            if (setDriverArrived.WasSuccessful)
            {
                CurrentDriver.Status = DriverStatusSRConstants.Arrive;
                await _driverService.UpdateDriver(CurrentDriver);

                await _tripService.UpdateGpsCustomerSegments(CustHostCode, (int) synergyLatitude, (int) synergyLongitude);

                var tripSegments = await _tripService.FindCustomerSegments(CustHostCode);

                string oops = string.Empty;

                foreach (TripSegmentModel segment in tripSegments)
                {
                    if (segment.TripSegEndLatitude == null)
                        oops = "oops";
                }
                if(oops != "oops")
                   UserDialogs.Instance.Toast(AppResources.SegmentUpdated);
            }

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
                    SetDriverArrive();
                    Close(this);
                }
            }
        }
        private async void SetDriverArrive()
        {
            var setDriverArrived = await _driverService.ProcessDriverArrivedAsync(new DriverArriveProcess
            {
                EmployeeId = CurrentDriver.EmployeeId,
                PowerId = CurrentDriver.PowerId,
                TripNumber = CurrentDriver.TripNumber,
                TripSegNumber = CurrentDriver.TripSegNumber,
                ActionDateTime = DateTime.Now,
                Odometer = CurrentDriver.Odometer ?? default(int),
            });

            if (!setDriverArrived.WasSuccessful)
            {
                await UserDialogs.Instance.AlertAsync(setDriverArrived.Failure.Summary,
                    AppResources.Error, AppResources.OK);
            }
            else
            {
                CurrentDriver.Status = DriverStatusSRConstants.Arrive;
                await _driverService.UpdateDriver(CurrentDriver);
                UserDialogs.Instance.Toast(AppResources.SegmentUpdated);
            }
        }
    }
}





