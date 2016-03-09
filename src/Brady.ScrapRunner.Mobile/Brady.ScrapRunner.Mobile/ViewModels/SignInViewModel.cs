using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using BWF.DataServices.PortableClients;
    using BWF.DataServices.PortableClients.Interfaces;
    using MvvmCross.Localization;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Acr.UserDialogs;
    using Domain.Models;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Plugins.Sqlite;
    using Resources;
    using Services;
    using Validators;

    public class SignInViewModel : BaseViewModel
    {
        private readonly IMvxSqliteConnectionFactory _sqliteConnectionFactory;
        private readonly IRepository<EmployeeMasterModel> _employeeMasterRepository;
        private readonly IRepository<PreferenceModel> _preferencRepository;
        private readonly DemoDataGenerator _demoDataGenerator;
        private readonly IConnectionService<DataServiceClient> _connection;

        public SignInViewModel(
            IRepository<EmployeeMasterModel> employeeMasterRepository,
            IRepository<PreferenceModel> preferenceRepository, 
            DemoDataGenerator demoDataGenerator,
            IConnectionService<DataServiceClient> connection,
            IMvxSqliteConnectionFactory sqliteConnectionFactory)
        {
            _employeeMasterRepository = employeeMasterRepository;
            _preferencRepository = preferenceRepository;
            _demoDataGenerator = demoDataGenerator;
            _sqliteConnectionFactory = sqliteConnectionFactory;

            _connection = connection;
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
        private string _usernameHint;
        private string _passwordHint;
        private string _signinLabel;

        public string UsernameHint
        {
            get { return _usernameHint; }
            set
            {
                SetProperty(ref _usernameHint, value);
            }
        }

        public string PasswordHint
        {
            get { return _passwordHint; }
            set
            {
                SetProperty(ref _passwordHint, value);
            }
        }

        public string SignInLabel
        {
            get { return _signinLabel; }
            set { SetProperty(ref _signinLabel, value); }
        }
        public override async void Start()
        {
            UsernameHint = AppResources.UserNameHint;
            PasswordHint = AppResources.PasswordHint;
            SignInLabel = AppResources.SignIn;

            base.Start();
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
                //var signInResult = await SignInDemoDataAsync();
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
                var message = exception?.InnerException?.Message ?? exception.Message;
                await UserDialogs.Instance.AlertAsync(
                    message, AppResources.Error, AppResources.OK);
            }
        }

        protected bool CanExecuteSignInCommand()
        {
            return !string.IsNullOrWhiteSpace(UserName)
                   && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task<bool> SignInAsync()
        {
            using (var loginData = UserDialogs.Instance.Loading(AppResources.LoggingIn, maskType: MaskType.Clear))
            {
                // Using this to create/delete the tables for now
                // @TODO : Refactor this
                await _demoDataGenerator.GenerateDemoDataAsync();

                // Check username/password against BWF, and create session if valid
                IClientSettings clientSettings = new DemoClientSettings();
                var connectionCreated = _connection.CreateConnection(clientSettings.ServiceBaseUri.ToString(),
                    clientSettings.UserName, clientSettings.Password, "ScrapRunner");

                // 2. Validate that driver exists
                // @TODO : Move to specialized employee service
                var userTask = await _connection.GetConnection().GetAsync<string, EmployeeMaster>(UserName);
                if (userTask == null) return false;
                await SaveEmployeeAsync(userTask);

                // 3. Lookup preferences
                // @TODO : Move to specialized preferences service
                var preferenceTask = await _connection.GetConnection().QueryAsync(new QueryBuilder<Preference>()
                    .Filter(y => y.Property(x => x.TerminalId).EqualTo(userTask.TerminalId)));
                await SavePreferencesAsync(preferenceTask.Records);
            }
            return true;
        }

        private Task SaveEmployeeAsync(EmployeeMaster employeeMaster)
        {
            var mapped = AutoMapper.Mapper.Map<EmployeeMaster, EmployeeMasterModel>(employeeMaster);
            return _employeeMasterRepository.InsertAsync(mapped);
        }

        private Task SavePreferencesAsync(List<Preference> preferences)
        {
            foreach (Preference preference in preferences)
            {
                var mapped = AutoMapper.Mapper.Map<Preference, PreferenceModel>(preference);
                _preferencRepository.InsertAsync(mapped);
            }

            return Task.FromResult(1);
        }

        private MvxCommand _settingsCommand;
        public MvxCommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new MvxCommand(ExecuteSettingsCommand));

        private void ExecuteSettingsCommand()
        {
            ShowViewModel<SettingsViewModel>();
        }

    }
}