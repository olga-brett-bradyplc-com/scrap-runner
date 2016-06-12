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
        private readonly ITripService _tripService; 

        public RouteSummaryViewModel(ITripService tripService)
        {
            _tripService = tripService;
            Title = AppResources.RouteSummary;
            RouteSelectedCommand = new MvxCommand<TripModel>(ExecuteRouteSelectedCommand);
        }

        public override async void Start()
        {
            var trips = await _tripService.FindTripsAsync();
            RouteSummaryList = new ObservableCollection<TripModel>(trips);

            base.Start();
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

        // @TODO : Put in logic that makes this read-only if driver is currently on a trip
        public void ExecuteRouteSelectedCommand(TripModel selectedTrip)
        {
            ShowViewModel<RouteDetailViewModel>(new {tripNumber = selectedTrip.TripNumber});
        }
    }
}