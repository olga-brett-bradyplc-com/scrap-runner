namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using Resources;
    using Xamarin.Forms;

    public class SignInViewModel : BaseViewModel
    {
        public SignInViewModel()
        {
            Title = AppResources.SignIn;
            SignInCommand = new Command(ExecuteSignInCommand);
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                SetProperty(ref _userName, value);
                SignInCommand.ChangeCanExecute();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                SignInCommand.ChangeCanExecute();
            }
        }

        public Command SignInCommand { get; protected set; }

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
            // @TODO: INavigation.PushAsync(new TruckView());
        }
    }
}
