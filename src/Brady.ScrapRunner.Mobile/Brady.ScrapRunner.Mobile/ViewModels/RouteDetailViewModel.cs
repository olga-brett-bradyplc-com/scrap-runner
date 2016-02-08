using System.Drawing;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Services;
using Xamarin.Forms;

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
            // This seems clunky to do it this way, but having issues getting
            // view triggers to work with enumerations
            CurrentStatus = ((int) TripStatusEnum.Stationary).ToString();

            DirectionsCommand = new RelayCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new RelayCommand(ExecuteEnRouteCommand);
            ArriveCommand = new RelayCommand(ExecuteArriveCommand);
            TransactionCommand = new RelayCommand(ExecuteTransactionsCommand);
        }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { Set(ref _tripNumber, value); }
        }

        private string _currentStatus;
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { Set(ref _currentStatus, value); }
        }

        // Command bindings
        public RelayCommand DirectionsCommand { get; private set; }
        public RelayCommand EnRouteCommand { get; private set; }
        public RelayCommand ArriveCommand { get; private set; }
        public RelayCommand TransactionCommand { get; private set; }

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            _navigationService.NavigateTo(Locator.RouteDirectionsView);
        }

        private async void ExecuteEnRouteCommand()
        {
            var message = string.Format(AppResources.ConfirmEnRouteMessage, 
                "\n\n",
                "\n", 
                "Kaman Aerospace",
                "1701 Indianwood Circle",
                "Maumee",
                "OH",
                "43537");
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmEnrouteTitle);
            if (confirm)
            {
                //_navigationService.NavigateTo(Locator.TransactionSummaryView);
                CurrentStatus = ((int)TripStatusEnum.Enroute).ToString();
            }
        }

        private async void ExecuteArriveCommand()
        {
            var message = string.Format(AppResources.ConfirmArrivalMessage,
                "\n\n",
                "\n",
                "Kaman Aerospace",
                "1701 Indianwood Circle",
                "Maumee",
                "OH",
                "43537");
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmArrivalTitle);
            if (confirm)
            {
                CurrentStatus = ((int) TripStatusEnum.Arrived).ToString();
            }
        }

        private void ExecuteTransactionsCommand()
        {
            _navigationService.NavigateTo(Locator.TransactionSummaryView);
        }
    }
}