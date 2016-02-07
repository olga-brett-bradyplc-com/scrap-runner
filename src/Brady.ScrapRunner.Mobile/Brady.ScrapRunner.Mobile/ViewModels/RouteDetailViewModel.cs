namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Acr.UserDialogs;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Interfaces;
    using Models;
    using Resources;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

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
        }

        public async Task LoadAsync(string tripNumber)
        {
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            if (trip == null) return;
            Title = trip.TripTypeDesc;
            TripCustName = trip.TripCustName;
            TripCustAddress = trip.TripCustAddress1 + trip.TripCustAddress2;
            TripCustCityStateZip = $"{trip.TripCustCity}, {trip.TripCustState} {trip.TripCustZip}";

            var containers = await _tripSegmentContainerRepository.ToListAsync(tsc =>
                tsc.TripNumber == trip.TripNumber);
            if (!containers.Any()) return;
            Containers = new ObservableCollection<TripSegmentContainerModel>(containers);
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

        private ObservableCollection<TripSegmentContainerModel> _containers; 
        public ObservableCollection<TripSegmentContainerModel> Containers
        {
            get { return _containers; }
            set { Set(ref _containers, value); }
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