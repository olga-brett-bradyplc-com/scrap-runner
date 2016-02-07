namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Interfaces;
    using Models;

    public class RouteSummaryViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IRepository<TripModel> _tripRepository; 

        public RouteSummaryViewModel(
            INavigationService navigationService, 
            IRepository<TripModel> tripRepository)
        {
            _navigationService = navigationService;
            _tripRepository = tripRepository;
            Title = "Route Summary";
            RouteSelectedCommand = new RelayCommand<TripModel>(ExecuteRouteSelectedCommand);
            ShowData();
        }

        public async void ShowData()
        {
            // @TODO: Use a better query
            var trips = await _tripRepository.AsQueryable()
                .Where(t => t.TripStatus != "D")
                .OrderBy(t => t.TripSequenceNumber).ToListAsync();
            RouteSummaryList = new ObservableCollection<TripModel>(trips);
        }

        private ObservableCollection<TripModel> _routeSummaryList;
        public ObservableCollection<TripModel> RouteSummaryList
        {
            get { return _routeSummaryList; }
            set { Set(ref _routeSummaryList, value); }
        }

        private TripModel _selectedTrip;
        public TripModel SelectedTrip
        {
            get { return _selectedTrip; }
            set { Set(ref _selectedTrip, value); }
        }

        public RelayCommand<TripModel> RouteSelectedCommand { get; private set; }

        public void ExecuteRouteSelectedCommand(TripModel selectedTrip)
        {
            _navigationService.NavigateTo(Locator.RouteDetailView, selectedTrip.TripNumber);
            SelectedTrip = null;
        }
    }
}