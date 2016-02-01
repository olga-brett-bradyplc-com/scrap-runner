namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;
    using Validators;

    public class PowerUnitViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public PowerUnitViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Power Unit ID and Odometer Reading";
            PowerUnitIdCommand = new RelayCommand(ExecutePowerUnitIdCommand, CanExecutePowerUnitIdCommand);
        }

        private string _truckId;
        private int? _odometer;

        public string TruckId
        {
            get { return _truckId; }
            set
            {
                Set(ref _truckId, value);
                PowerUnitIdCommand.RaiseCanExecuteChanged();
            }
        }

        public int? Odometer
        {
            get { return _odometer; }
            set
            {
                Set(ref _odometer, value);
                PowerUnitIdCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand PowerUnitIdCommand { get; protected set; }

        protected void ExecutePowerUnitIdCommand()
        {
            var truckIdResults = Validate<PowerUnitValidator, string>(TruckId);
            if (!truckIdResults.IsValid)
            {
                UserDialogs.Instance.Alert(truckIdResults.Errors.First().ErrorMessage);
                return;
            }
            var odometerResults = Validate<OdometerRangeValidator, int?>(Odometer);
            if (!odometerResults.IsValid)
            {
                UserDialogs.Instance.Alert(odometerResults.Errors.First().ErrorMessage);
                return;
            }
            // @TODO: Validate odometer using BWF Client Library here.
            _navigationService.NavigateTo(Locator.RouteSummaryView);
        }

        protected bool CanExecutePowerUnitIdCommand()
        {
            return !string.IsNullOrWhiteSpace(TruckId)
                   && Odometer.HasValue;
        }
    }
}