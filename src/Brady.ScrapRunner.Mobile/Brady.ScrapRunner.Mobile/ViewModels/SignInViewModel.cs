using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Brady.ScrapRunner.Domain.Process;

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
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;
        private readonly DemoDataGenerator _demoDataGenerator;
        private readonly IConnectionService<DataServiceClient> _connection;

        public SignInViewModel(
            IRepository<EmployeeMasterModel> employeeMasterRepository,
            IRepository<PreferenceModel> preferenceRepository,
            IRepository<TripModel> tripRepository,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository,
            DemoDataGenerator demoDataGenerator,
            IConnectionService<DataServiceClient> connection,
            IMvxSqliteConnectionFactory sqliteConnectionFactory)
        {
            _employeeMasterRepository = employeeMasterRepository;
            _preferencRepository = preferenceRepository;
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            _demoDataGenerator = demoDataGenerator;
            _sqliteConnectionFactory = sqliteConnectionFactory;

            _connection = connection;
            Title = AppResources.SignInTitle;
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

        private string _truckId;
        public string TruckId 
        {
            get { return _truckId; }
            set
            {
                SetProperty(ref _truckId, value);
                SignInCommand.RaiseCanExecuteChanged();
            }
        }

        private int _odometer;
        public int Odometer
        {
            get { return _odometer; }
            set
            {
                SetProperty(ref _odometer, value);
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
                    return;
                }
                Close(this);
                ShowViewModel<RouteSummaryViewModel>();
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
                   && !string.IsNullOrWhiteSpace(Password)
                   && !string.IsNullOrWhiteSpace(TruckId);
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
                //var userTask = await _connection.GetConnection().GetAsync<string, EmployeeMaster>(UserName);
                //if (userTask == null) return false;
                //await SaveEmployeeAsync(userTask);

                var loginProcessObj = new DriverLoginProcess
                {
                    EmployeeId = UserName,
                    Password = Password,
                    PowerId = TruckId,
                    Odometer = Odometer,
                    LocaleCode = 1033,
                    OverrideFlag = "N",
                    Mdtid = "Phone",
                    LoginDateTime = DateTime.Now
                };

                var loginProcess =
                    await _connection.GetConnection().UpdateAsync(loginProcessObj, requeryUpdated: false);

                if (loginProcess.WasSuccessful)
                {
                    var temp = loginProcess.Item;
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(loginProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }
                
                // 3. Lookup preferences
                // @TODO : Move to specialized preferences service
                //var preferenceTask = await _connection.GetConnection().QueryAsync(new QueryBuilder<Preference>()
                //    .Filter(y => y.Property(x => x.TerminalId).EqualTo(userTask.TerminalId)));
                //await SavePreferencesAsync(preferenceTask.Records);

                loginData.Title = "Loading Preferences";

                var preferenceObj = new PreferencesProcess
                {
                    EmployeeId = UserName
                };

                var preferenceProcess = await _connection.GetConnection().UpdateAsync(preferenceObj, requeryUpdated: false);

                if (preferenceProcess.WasSuccessful)
                {
                    await SavePreferencesAsync(preferenceProcess.Item.Preferences);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(preferenceProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

                loginData.Title = AppResources.LoadingTripInformation;

                var tripObj = new TripInfoProcess
                {
                    EmployeeId = UserName
                };

                var tripProcess = await _connection.GetConnection().UpdateAsync(tripObj, requeryUpdated: false);

                if (tripProcess.WasSuccessful)
                {
                    await SaveTripsAsync(tripProcess.Item.Trips);
                    await SaveTripSegmentsAsync(tripProcess.Item.TripSegments);
                    await SaveTripSegmentContainersAsync(tripProcess.Item.TripSegmentContainers);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(tripProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

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
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Preference>, IEnumerable<PreferenceModel>>(preferences);
            return _preferencRepository.InsertRangeAsync(mapped);
        }

        private Task SaveTripsAsync(List<Trip> trips)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(trips);
            return _tripRepository.InsertRangeAsync(mapped);
        }
        
        private Task SaveTripSegmentsAsync(List<TripSegment> tripSegments)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<TripSegment>, IEnumerable<TripSegmentModel>>(tripSegments);
            return _tripSegmentRepository.InsertRangeAsync(mapped);
        }

        private Task SaveTripSegmentContainersAsync(List<TripSegmentContainer> containers)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<TripSegmentContainer>, IEnumerable<TripSegmentContainerModel>>(containers);
            return _tripSegmentContainerRepository.InsertRangeAsync(mapped);
        }

        private MvxCommand _settingsCommand;
        public MvxCommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new MvxCommand(ExecuteSettingsCommand));

        private void ExecuteSettingsCommand()
        {
            ShowViewModel<SettingsViewModel>();
            Close(this);
        }
    }
}