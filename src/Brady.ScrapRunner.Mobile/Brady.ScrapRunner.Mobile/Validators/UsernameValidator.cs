namespace Brady.ScrapRunner.Mobile.Validators
{
    using FluentValidation;

    public class UsernameValidator : AbstractValidator<string>
    {
        private const int UsernameMinLength = 1;
        private const int UsernameMaxLength = 10;

        public UsernameValidator()
        {
            RuleFor(userName => userName)
                .NotEmpty()
                .WithMessage(Resources.AppResources.UserNameNotEmpty);
            RuleFor(userName => userName)
                .Length(UsernameMinLength, UsernameMaxLength)
                .WithMessage(Resources.AppResources.UserNameOutOfRange, UsernameMaxLength);
        }
    }
}
