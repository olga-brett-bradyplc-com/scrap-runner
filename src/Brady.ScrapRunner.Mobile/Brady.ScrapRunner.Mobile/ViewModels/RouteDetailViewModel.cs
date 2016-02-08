using Acr.UserDialogs;
using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Interfaces;
    using Models;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;
        private string _custHostCode;

        public RouteDetailViewModel(
            INavigationService navigationService, 
            IRepository<TripModel> tripRepository, 
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _navigationService = navigationService;
            _tripRepository = tripRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            DirectionsCommand = new RelayCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new RelayCommand(ExecuteEnRouteCommand);
            ArriveCommand = new RelayCommand(ExecuteArriveCommand);
            TransactionCommand = new RelayCommand(ExecuteTransactionCommand);
        }
        
        public async Task LoadAsync(string tripNumber)
        {
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            if (trip != null)
            {
                _custHostCode = trip.TripCustHostCode;
                Title = trip.TripTypeDesc;
                TripNumber = $"Trip {trip.TripNumber}";
                TripCustName = trip.TripCustName;
                TripDriverInstructions = trip.TripDriverInstructions;
                TripCustAddress = trip.TripCustAddress1 + trip.TripCustAddress2;
                TripCustCityStateZip = $"{trip.TripCustCity}, {trip.TripCustState} {trip.TripCustZip}";
            }

            var containers = await _tripSegmentContainerRepository.ToListAsync(tsc => tsc.TripNumber == tripNumber);
            if (containers.Any())
            {
                Containers = new ObservableCollection<TripSegmentContainerModel>(containers);
            }
        }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { Set(ref _tripNumber, value); }
        }

        private string _tripDriverInstructions;
        public string TripDriverInstructions
        {
            get { return _tripDriverInstructions; }
            set { Set(ref _tripDriverInstructions, value); }
        }

        private string _tripCustName;
        public string TripCustName
        {
            get { return _tripCustName; }
            set { Set(ref _tripCustName, value); }
        }

        private string _tripCustAddress;
        public string TripCustAddress
        {
            get { return _tripCustAddress; }
            set { Set(ref _tripCustAddress, value); }
        }

        private string _tripCustCityStateZip;
        public string TripCustCityStateZip
        {
            get { return _tripCustCityStateZip; }
            set { Set(ref _tripCustCityStateZip, value); }
        }

        private string _currentStatus;

        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { Set(ref _currentStatus, value); }
        }

        private ObservableCollection<TripSegmentContainerModel> _containers; 
        public ObservableCollection<TripSegmentContainerModel> Containers
        {
            get { return _containers; }
            set { Set(ref _containers, value); }
        }

        // Command bindings
        public RelayCommand DirectionsCommand { get; private set; }
        public RelayCommand EnRouteCommand { get; private set; }
        public RelayCommand ArriveCommand { get; private set; }
        public RelayCommand TransactionCommand { get; private set; }

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            if (!string.IsNullOrEmpty(_custHostCode))
                _navigationService.NavigateTo(Locator.RouteDirectionsView, _custHostCode);
        }

        private async void ExecuteEnRouteCommand()
        {
            var message = string.Format(AppResources.ConfirmEnRouteMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmEnrouteTitle);
            if (confirm)
            {
                CurrentStatus = "EN"; //DriverStatusConstants.EnRoute;
            }
        }

        private async void ExecuteArriveCommand()
        {
            var message = string.Format(AppResources.ConfirmArrivalMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmArrivalTitle);
            if (confirm)
            {
                CurrentStatus = "AR"; //DriverStatusConstants.Arrive;
            }
        }

        private void ExecuteTransactionCommand()
        {
            _navigationService.NavigateTo(Locator.TransactionSummaryView);
        }
    }
}