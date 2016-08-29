using System.Linq;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDriverService _driverService;
        private readonly IContainerService _containerService;
        private readonly IPreferenceService _preferenceService;

        public MainViewModel(IDriverService driverService, IContainerService containerService, IPreferenceService preferenceService)
        {
            _driverService = driverService;
            _containerService = containerService;
            _preferenceService = preferenceService;
        }

        public void Init(DriverStatusModel currentDriver)
        {
            CurrentDriver = currentDriver;
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var containers = await _containerService.FindPowerIdContainersAsync(CurrentDriver.PowerId);
            var autoDrop =
                await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFAutoDropContainers);
            
            // Load menu viewmodel
            ShowViewModel<MenuViewModel>();

            // Driver was in the middle of a trip during their last session, so return them to the appropiate screen
            if (!string.IsNullOrEmpty(CurrentDriver.TripNumber) &&
                !string.IsNullOrEmpty(CurrentDriver.TripSegNumber) &&
                (CurrentDriver.Status == DriverStatusSRConstants.Enroute || CurrentDriver.Status == DriverStatusSRConstants.Arrive || CurrentDriver.Status == DriverStatusSRConstants.Done))
            {
                ShowViewModel<RouteDetailViewModel>(new { tripNumber = CurrentDriver.TripNumber, status = CurrentDriver.Status });
                UserDialogs.Instance.Toast(AppResources.SessionRestoreHeader);
            }
            else if (containers.Any() && autoDrop == Constants.No)
            {
                ShowViewModel<LoadDropContainerViewModel>(new { loginProcessed = true });
            }
            else
            {
                ShowViewModel<RouteSummaryViewModel>();
            }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        public void ShowMenu()
        {
            //ShowViewModel<SignInViewModel>();
            //ShowViewModel<MenuViewModel>();
        }
    }
}