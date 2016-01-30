namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using GalaSoft.MvvmLight.Views;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public RouteDetailViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Switch Trip";
        }
    }
}