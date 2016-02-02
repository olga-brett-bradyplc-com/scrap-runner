namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Acr.UserDialogs;
    using Domain.Models;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Interfaces;
    using Models;
    using Resources;
    using Validators;
    using Xamarin.Forms;

    public class SignInViewModel : BaseViewModel
    {
        private readonly ISqliteDatabase _sqliteDatabase;
        private readonly INavigationService _navigationService;
        private readonly IRepository<EmployeeMasterModel> _employeeMasterRepository;

        public SignInViewModel(
            INavigationService navigationService,
            IRepository<EmployeeMasterModel> employeeMasterRepository
            )
        {
            _navigationService = navigationService;
            _employeeMasterRepository = employeeMasterRepository;
            _sqliteDatabase = DependencyService.Get<ISqliteDatabase>();
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

        protected async void ExecuteSignInCommand()
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
            try
            {
                var signInResult = await SignInAsync();
                if (!signInResult)
                {
                    await UserDialogs.Instance.AlertAsync(AppResources.SignInProcessFailed, 
                        AppResources.Error, AppResources.OK);
                    return;
                }
                _navigationService.NavigateTo(Locator.PowerUnitView);
            }
            catch (Exception exception)
            {
                await UserDialogs.Instance.AlertAsync(
                    exception.Message, AppResources.Error, AppResources.OK);
            }
        }

        protected bool CanExecuteSignInCommand()
        {
            return !string.IsNullOrWhiteSpace(UserName)
                   && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task<bool> SignInAsync()
        {
            await _sqliteDatabase.InitializeAsync();
            var employeeMaster = await GetEmployeeMasterAsync();
            if (employeeMaster == null) return false;
            await SaveEmployeeAsync(employeeMaster);
            return true;
        }

        private Task<EmployeeMaster> GetEmployeeMasterAsync()
        {
            // @TODO: Get EmployeeMaster where EmployeeId = {Username} using BWF Client Library.
            return Task.FromResult(new EmployeeMaster
            {
                EmployeeId = UserName,
                AreaId = "ALL",
                FirstName = "BRADY TEST",
                LastName = "DRIVER",
                RegionId = "SDF",
                TerminalId = "F1"
            });
        }

        private Task SaveEmployeeAsync(EmployeeMaster employeeMaster)
        {
            var mapped = AutoMapper.Mapper.Map<EmployeeMaster, EmployeeMasterModel>(employeeMaster);
            return _employeeMasterRepository.InsertAsync(mapped);
        }

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new RelayCommand(ExecuteSettingsCommand));

        private void ExecuteSettingsCommand()
        {
            _navigationService.NavigateTo(Locator.SettingsView);
        }
    }
}