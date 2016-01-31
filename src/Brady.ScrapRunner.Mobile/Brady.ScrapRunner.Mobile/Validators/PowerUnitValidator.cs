namespace Brady.ScrapRunner.Mobile.Validators
{
    using FluentValidation;

    public class PowerUnitValidator : AbstractValidator<string>
    {
        private const int TruckIdMinLength = 1;
        private const int TruckIdMaxLength = 16;

        public PowerUnitValidator()
        {
            RuleFor(truckId => truckId)
                .NotEmpty()
                .WithMessage(Resources.AppResources.TruckIdRequired);
            RuleFor(truckId => truckId)
                .Length(TruckIdMinLength, TruckIdMaxLength)
                .WithMessage(Resources.AppResources.TruckIdOutOfRange, TruckIdMaxLength);
        }
    }
}