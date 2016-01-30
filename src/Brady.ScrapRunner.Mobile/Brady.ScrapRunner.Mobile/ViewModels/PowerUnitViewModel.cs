namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;

    public class PowerUnitViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public PowerUnitViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Power Unit ID and Odometer Reading";
            PowerUnitIdCommand = new RelayCommand(ExecutePowerUnitIdCommand);
        }

        private string _truckId;
        private string _odometer;

        public string TruckId
        {
            get { return _truckId; }
            set { Set(ref _truckId, value); }
        }

        public string Odometer
        {
            get { return _odometer; }
            set { Set(ref _odometer, value); }
        }

        public RelayCommand PowerUnitIdCommand { get; protected set; }

        protected void ExecutePowerUnitIdCommand()
        {
            var validator = new Validators.PowerUnitValidator();
            var results = validator.Validate(this);
            var validationSucceeded = results.IsValid;
            if (!validationSucceeded)
            {
                UserDialogs.Instance.Alert(results.Errors.First().ErrorMessage);
                return;
            }
            _navigationService.NavigateTo(Locator.RouteSummaryView);
        }
    }
}