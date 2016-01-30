namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;
    using Resources;

    public class SignInViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public SignInViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = AppResources.SignIn;
            SignInCommand = new RelayCommand(ExecuteSignInCommand);
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                Set(ref _userName, value);
                SignInCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                Set(ref _password, value);
                SignInCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SignInCommand { get; protected set; }

        protected void ExecuteSignInCommand()
        {
            var validator = new Validators.SignInCommandValidator();
            var results = validator.Validate(this);
            var validationSucceeded = results.IsValid;
            if (!validationSucceeded)
            {
                UserDialogs.Instance.Alert(results.Errors.First().ErrorMessage);
                return;
            }
            _navigationService.NavigateTo(Locator.PowerUnitView);
        }
    }
}