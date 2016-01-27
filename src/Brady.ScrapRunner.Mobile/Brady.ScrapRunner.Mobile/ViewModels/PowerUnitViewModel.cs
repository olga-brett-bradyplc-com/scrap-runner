using System;
using FluentValidation.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using Xamarin.Forms;

    public class PowerUnitViewModel : BaseViewModel
    {

        public PowerUnitViewModel()
        {
            Title = "Power Unit ID and Odometer Reading";
            PowerUnitIdCommand = new Command(ExecutePowerUnitIdCommand);
        }

        private string _truckId;
        private string _odometer;

        public string TruckId
        {
            get { return _truckId; }
            set { SetProperty(ref _truckId, value); }
        }

        public string Odometer
        {
            get { return _odometer; }
            set { SetProperty(ref _odometer, value); }
        }

        public Command PowerUnitIdCommand { get; protected set; }

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
        }

    }
}