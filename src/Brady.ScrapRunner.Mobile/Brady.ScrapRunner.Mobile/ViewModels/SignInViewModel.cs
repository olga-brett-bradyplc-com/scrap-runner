namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;
    using Resources;
    using Validators;

    public class SignInViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public SignInViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = AppResources.SignIn;
            SignInCommand = new RelayCommand(ExecuteSignInCommand, CanExecuteSignInCommand);
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
            var userNameResults = Validate<UsernameValidator, string>(UserName);
            if (!userNameResults.IsValid)
            {
                UserDialogs.Instance.Alert(userNameResults.Errors.First().ErrorMessage);
                return;
            }
            var passwordResults = Validate<PasswordValidator, string>(Password);
            if (!passwordResults.IsValid)
            {
                UserDialogs.Instance.Alert(userNameResults.Errors.First().ErrorMessage);
                return;
            }
            // @TODO: Get Driver data using BWF Client Library Here.
            _navigationService.NavigateTo(Locator.PowerUnitView);
        }

        protected bool CanExecuteSignInCommand()
        {
            return !string.IsNullOrWhiteSpace(UserName)
                   && !string.IsNullOrWhiteSpace(Password);
        }
    }
}