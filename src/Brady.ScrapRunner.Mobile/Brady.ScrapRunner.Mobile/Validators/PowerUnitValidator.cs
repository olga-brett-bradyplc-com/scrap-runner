using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.ViewModels;
using FluentValidation;

namespace Brady.ScrapRunner.Mobile.Validators
{
    public class PowerUnitValidator : AbstractValidator<PowerUnitViewModel>
    {
        public PowerUnitValidator()
        {
            RuleFor(vm => vm.TruckId )
                .NotEmpty()
                .WithMessage(Resources.AppResources.TruckIdRequired);
            RuleFor(vm => vm.Odometer)
                .NotEmpty()
                .WithMessage(Resources.AppResources.OdometerRequired)
                .Must(IsNumeric) // Is there a better way to do this?
                .WithMessage(Resources.AppResources.NumericRequired);
        }

        private bool IsNumeric(PowerUnitViewModel field, string val)
        {
            int n;
            return int.TryParse(val, out n);
        }
    }
}
