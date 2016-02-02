namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Acr.UserDialogs;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;
    using Resources;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public RouteDetailViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Switch Trip";
            DirectionsCommand = new RelayCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new RelayCommand(ExecuteEnRouteCommand);
            ArriveCommand = new RelayCommand(ExecuteArriveCommand);
        }

        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { Set(ref _tripNumber, value); }
        }

        public RelayCommand DirectionsCommand { get; private set; }
        public RelayCommand EnRouteCommand { get; private set; }
        public RelayCommand ArriveCommand { get; private set; }

        private void ExecuteDrivingDirectionsCommand()
        {
            _navigationService.NavigateTo(Locator.RouteDirectionsView);
        }

        private void ExecuteEnRouteCommand()
        {
        }

        private void ExecuteArriveCommand()
        {
        }
    }
}