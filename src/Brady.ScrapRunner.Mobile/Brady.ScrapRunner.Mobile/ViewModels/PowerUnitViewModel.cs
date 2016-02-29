using MvvmCross.Localization;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using MvvmCross.Core.ViewModels;
    using Validators;

    public class PowerUnitViewModel : BaseViewModel
    {
        public PowerUnitViewModel()
        {
            Title = "Power Unit ID and Odometer Reading";
            PowerUnitIdCommand = new MvxCommand(ExecutePowerUnitIdCommand, CanExecutePowerUnitIdCommand);
        }

        private string _truckId;
        private int? _odometer;

        public string TruckId
        {
            get { return _truckId; }
            set
            {
                SetProperty(ref _truckId, value);
                PowerUnitIdCommand.RaiseCanExecuteChanged();
            }
        }

        public int? Odometer
        {
            get { return _odometer; }
            set
            {
                SetProperty(ref _odometer, value);
                PowerUnitIdCommand.RaiseCanExecuteChanged();
            }
        }

        public MvxCommand PowerUnitIdCommand { get; protected set; }

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
            Close(this);
            ShowViewModel<RouteSummaryViewModel>();
        }

        protected bool CanExecutePowerUnitIdCommand()
        {
            return !string.IsNullOrWhiteSpace(TruckId)
                   && Odometer.HasValue;
        }
    }
}