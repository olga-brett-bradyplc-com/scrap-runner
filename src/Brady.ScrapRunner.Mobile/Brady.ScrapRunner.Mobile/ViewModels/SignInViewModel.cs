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
                var message = exception.InnerException.Message ?? exception.Message;
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
            // Using this to create/delete the tables for now
            await _demoDataGenerator.GenerateDemoDataAsync();

            // Check username/password against BWF, and create session if valid
            var session = _connection.CreateConnection("", "", "ScrapRunner");

            // 2. Validate that driver exists
            var userTask = await _connection.GetConnection().GetAsync<string, EmployeeMaster>(UserName);
            if (userTask == null) return false;
            await SaveEmployeeAsync(userTask);

            // 3. Lookup preferences
            var preferenceTask = await _connection.GetConnection().QueryAsync(new QueryBuilder<Preference>()
                .Filter( y => y.Property(x => x.TerminalId).EqualTo(userTask.TerminalId)));
            await SavePreferencesAsync(preferenceTask.Records);

            return true;
        }

        //private async Task<bool> SignInDemoDataAsync()
        //{
        //    await _demoDataGenerator.GenerateDemoDataAsync();
        //    var employeeMaster = await GetEmployeeMasterAsync();
        //    if (employeeMaster == null) return false;
        //    await SaveEmployeeAsync(employeeMaster);
        //    return true;
        //}

        //private Task<EmployeeMaster> GetEmployeeMasterAsync()
        //{
        //    // @TODO: Get EmployeeMaster where EmployeeId = {Username} using BWF Client Library.
        //    return Task.FromResult(new EmployeeMaster
        //    {
        //        EmployeeId = UserName,
        //        AreaId = "ALL",
        //        FirstName = "BRADY TEST",
        //        LastName = "DRIVER",
        //        RegionId = "SDF",
        //        TerminalId = "F1"
        //    });
        //}

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
        public IMvxLanguageBinder TextSource
        {
            get { return new MvxLanguageBinder("", GetType().Name); }
        }

    }
}