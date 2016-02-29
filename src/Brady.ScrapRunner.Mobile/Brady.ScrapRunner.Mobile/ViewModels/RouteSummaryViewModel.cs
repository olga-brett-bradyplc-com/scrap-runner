using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class RouteSummaryViewModel : BaseViewModel
    {
        private readonly IRepository<TripModel> _tripRepository; 

        public RouteSummaryViewModel(
            IRepository<TripModel> tripRepository)
        {
            _tripRepository = tripRepository;
            SubTitle = AppResources.Trip + $" {SelectedTrip.TripNumber}";
            RouteSelectedCommand = new MvxCommand<TripModel>(ExecuteRouteSelectedCommand);
        }

        public override async void Start()
        {
            base.Start();
            var trips = await _tripRepository.AsQueryable()
                .Where(t => t.TripStatus == TripStatusConstants.Pending)
                .OrderBy(t => t.TripSequenceNumber).ToListAsync();
            RouteSummaryList = new ObservableCollection<TripModel>(trips);
        }

        private ObservableCollection<TripModel> _routeSummaryList;
        public ObservableCollection<TripModel> RouteSummaryList
        {
            get { return _routeSummaryList; }
            set { SetProperty(ref _routeSummaryList, value); }
        }

        private TripModel _selectedTrip;
        public TripModel SelectedTrip
        {
            get { return _selectedTrip; }
            set { SetProperty(ref _selectedTrip, value); }
        }

        public MvxCommand<TripModel> RouteSelectedCommand { get; private set; }

        public void ExecuteRouteSelectedCommand(TripModel selectedTrip)
        {
            ShowViewModel<RouteDetailViewModel>(new {tripNumber = selectedTrip.TripNumber});
            SelectedTrip = null;
        }
    }
}