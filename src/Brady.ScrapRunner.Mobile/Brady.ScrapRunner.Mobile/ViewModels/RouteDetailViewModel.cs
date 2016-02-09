namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Acr.UserDialogs;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;
        private string _custHostCode;

        public RouteDetailViewModel(
            IRepository<TripModel> tripRepository, 
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripRepository = tripRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            DirectionsCommand = new MvxCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new MvxCommand(ExecuteEnRouteCommand);
            ArriveCommand = new MvxCommand(ExecuteArriveCommand);
            TransactionCommand = new MvxCommand(ExecuteTransactionCommand);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }

        public override async void Start()
        {
            base.Start();
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == TripNumber);
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

            var containers = await _tripSegmentContainerRepository.ToListAsync(tsc => tsc.TripNumber == TripNumber);
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
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _tripDriverInstructions;
        public string TripDriverInstructions
        {
            get { return _tripDriverInstructions; }
            set { SetProperty(ref _tripDriverInstructions, value); }
        }

        private string _tripCustName;
        public string TripCustName
        {
            get { return _tripCustName; }
            set { SetProperty(ref _tripCustName, value); }
        }

        private string _tripCustAddress;
        public string TripCustAddress
        {
            get { return _tripCustAddress; }
            set { SetProperty(ref _tripCustAddress, value); }
        }

        private string _tripCustCityStateZip;
        public string TripCustCityStateZip
        {
            get { return _tripCustCityStateZip; }
            set { SetProperty(ref _tripCustCityStateZip, value); }
        }

        private string _currentStatus;

        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { SetProperty(ref _currentStatus, value); }
        }

        private ObservableCollection<TripSegmentContainerModel> _containers; 
        public ObservableCollection<TripSegmentContainerModel> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand DirectionsCommand { get; private set; }
        public MvxCommand EnRouteCommand { get; private set; }
        public MvxCommand ArriveCommand { get; private set; }
        public MvxCommand TransactionCommand { get; private set; }

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            if (!string.IsNullOrEmpty(_custHostCode))
                ShowViewModel<RouteDirectionsViewModel>(new {custHostCode = _custHostCode});
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
            ShowViewModel<TransactionSummaryViewModel>();
        }
    }
}