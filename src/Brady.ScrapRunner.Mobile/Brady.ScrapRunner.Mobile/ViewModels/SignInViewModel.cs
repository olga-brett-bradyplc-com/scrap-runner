namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Acr.UserDialogs;
    using Domain.Models;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;
    using Services;
    using Validators;
    using Xamarin.Forms;

    public class SignInViewModel : BaseViewModel
    {
        private readonly ISqliteDatabase _sqliteDatabase;
        private readonly IRepository<EmployeeMasterModel> _employeeMasterRepository;
        private readonly DemoDataGenerator _demoDataGenerator;

        public SignInViewModel(
            IRepository<EmployeeMasterModel> employeeMasterRepository, 
            DemoDataGenerator demoDataGenerator
            )
        {
            _employeeMasterRepository = employeeMasterRepository;
            _demoDataGenerator = demoDataGenerator;
            _sqliteDatabase = DependencyService.Get<ISqliteDatabase>();
            Title = AppResources.SignIn;
            SignInCommand = new MvxCommand(ExecuteSignInCommand, CanExecuteSignInCommand);
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                SetProperty(ref _userName, value);
                SignInCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                SignInCommand.RaiseCanExecuteChanged();
            }
        }

        public MvxCommand SignInCommand { get; protected set; }

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
                Close(this);
                ShowViewModel<PowerUnitViewModel>();
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
            await _demoDataGenerator.GenerateDemoDataAsync();
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

        private MvxCommand _settingsCommand;
        public MvxCommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new MvxCommand(ExecuteSettingsCommand));

        private void ExecuteSettingsCommand()
        {
            ShowViewModel<SettingsViewModel>();
        }
    }
}