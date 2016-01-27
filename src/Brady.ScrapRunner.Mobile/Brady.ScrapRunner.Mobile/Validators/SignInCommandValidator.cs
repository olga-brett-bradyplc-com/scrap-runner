namespace Brady.ScrapRunner.Mobile.Validators
{
    using FluentValidation;
    using ViewModels;

    public class SignInCommandValidator : AbstractValidator<SignInViewModel>
    {
        public SignInCommandValidator()
        {
            RuleFor(vm => vm.UserName)
                .NotEmpty()
                .WithMessage(Resources.AppResources.UserNameNotEmpty);
            RuleFor(vm => vm.Password)
                .NotEmpty()
                .WithMessage(Resources.AppResources.PasswordNotEmpty);
        }
    }
}
