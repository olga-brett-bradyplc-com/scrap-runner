namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;

    public class RouteSummaryViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public RouteSummaryViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Route Summary";
            RouteSelectedCommand = new RelayCommand<Route>(ExecuteRouteSelectedCommand);
            CreateDummyData();
        }

        private string _tripId;
        public string TripId
        {
            get { return _tripId; }
            set { Set(ref _tripId, value); }
        }

        public ObservableCollection<Route> RouteSummaryList { get; private set; }

        private Route _selectedRoute;
        public Route SelectedRoute
        {
            get { return _selectedRoute; }
            set { Set(ref _selectedRoute, value); }
        }

        public RelayCommand<Route> RouteSelectedCommand { get; private set; }

        public void ExecuteRouteSelectedCommand(Route selectedRoute)
        {
            _navigationService.NavigateTo(Locator.RouteDetailView, selectedRoute.TripNumber);
        }

        // @TODO : Refactor using Brady.Domain objects when appropiate
        public void CreateDummyData()
        {
            RouteSummaryList = new ObservableCollection<Route>
            {
                new Route
                {
                    Notes = "Do something special with this note",
                    CompanyName = "Kaman Aerospace",
                    TripType = "Switch",
                    Address1 = "1701 Indianwood Circle",
                    City = "Maumee",
                    State = "OH",
                    Zipcode = "43537",
                    CloseTime = "2000",
                    OpenTime = "0900",
                    TripNumber = "615112"
                },
                new Route
                {
                    Notes = "This should be an easy trip",
                    CompanyName = "Jay's Scrap Metal",
                    TripType = "Return To Yard",
                    Address1 = "6560 Brixton Rd",
                    City = "Maumee",
                    State = "OH",
                    Zipcode = "43537",
                    CloseTime = "2000",
                    OpenTime = "2000",
                    TripNumber = "615113"
                },
                new Route
                {
                    Notes = "Just drop a few containers and party",
                    CompanyName = "Jimbo's Recycling",
                    TripType = "Drop Empty",
                    Address1 = "Dingbing Rd",
                    City = "Findlay",
                    State = "OH",
                    Zipcode = "43900",
                    CloseTime = "0500",
                    OpenTime = "0900",
                    TripNumber = "615114"
                },
                new Route
                {
                    Notes =
                        "WHHHHAAATTTTT? This is a test to see if the content will wrap correctly, otherwise back to the drawing board ...",
                    TripType = "Switch",
                    CompanyName = "SIMS Metal Management",
                    Address1 = "1701 Indianwood Circle",
                    City = "Maumee",
                    State = "OH",
                    Zipcode = "43537",
                    CloseTime = "2000",
                    OpenTime = "0900",
                    TripNumber = "615115"
                }
            };
        }
    }
}