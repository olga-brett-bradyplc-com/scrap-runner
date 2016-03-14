namespace Brady.ScrapRunner.Mobile.Validators
{
    using FluentValidation;
    using Resources;

    public class OdometerRangeValidator : AbstractValidator<int?>
    {
        private const int OdometerMinValue = 0;
        private const int OdometerMaxValue = 9999999;

        public OdometerRangeValidator()
        {
            RuleFor(odometer => odometer)
                .Must(HaveValue)
                .WithMessage(AppResources.OdometerRequired);
            RuleFor(odometer => odometer)
                .InclusiveBetween(OdometerMinValue, OdometerMaxValue)
                .WithMessage(AppResources.OdometerOutOfRange, OdometerMinValue, OdometerMaxValue);
        }

        private bool HaveValue(int? odometer)
        {
            return odometer.HasValue;
        }
    }
}