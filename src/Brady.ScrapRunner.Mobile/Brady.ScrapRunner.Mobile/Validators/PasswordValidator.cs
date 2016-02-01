namespace Brady.ScrapRunner.Mobile.Validators
{
    using FluentValidation;

    public class PasswordValidator : AbstractValidator<string>
    {
        private const int PasswordMinLength = 1;
        private const int PasswordMaxLength = 20;

        public PasswordValidator()
        {
            RuleFor(userName => userName)
                .NotEmpty()
                .WithMessage(Resources.AppResources.PasswordNotEmpty);
            RuleFor(userName => userName)
                .Length(PasswordMinLength, PasswordMaxLength)
                .WithMessage(Resources.AppResources.PasswordOutOfRange, PasswordMaxLength);
        }
    }
}