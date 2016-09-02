using Brady.ScrapRunner.Domain.Process;
using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;
using Brady.ScrapRunner.Mobile.Resources;
using Brady.ScrapRunner.Mobile.Validators;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class SignInViewModel : BaseViewModel
    {
        private readonly IDbService _dbService;
        private readonly IPreferenceService _preferenceService;
        private readonly ITripService _tripService;
        private readonly ICustomerService _customerService;
        private readonly IDriverService _driverService;
        private readonly IContainerService _containerService;
        private readonly ICodeTableService _codeTableService;
        private readonly ITerminalService _terminalService;
        private readonly IConnectionService _connection;
        private readonly IMessagesService _messagesService;
        private readonly IBackgroundScheduler _backgroundScheduler;
        private readonly ILocationService _locationService;
        private readonly ILocationOdometerService _locationOdometerService;
        private readonly ILocationGeofenceService _locationGeofenceService;
        private readonly ILocationPathService _locationPathService;
        
        public SignInViewModel(
            IDbService dbService,
            IPreferenceService preferenceService,
            ITripService tripService,
            ICustomerService customerService,
            IDriverService driverService,
            IContainerService containerService,
            ICodeTableService codeTableService,
            ITerminalService terminalService,
            IMessagesService messagesService,
            IConnectionService connection, 
            IBackgroundScheduler backgroundScheduler, 
            ILocationService locationService, 
            ILocationOdometerService locationOdometerService,
            ILocationGeofenceService locationGeofenceService, 
            ILocationPathService locationPathService)
        {
            _dbService = dbService;
            _preferenceService = preferenceService;
            _tripService = tripService;
            _customerService = customerService;
            _driverService = driverService;
            _containerService = containerService;
            _codeTableService = codeTableService;
            _terminalService = terminalService;
            _messagesService = messagesService;
            _locationGeofenceService = locationGeofenceService;
            _locationPathService = locationPathService;

            _connection = connection;
            _backgroundScheduler = backgroundScheduler;
            _locationService = locationService;
            _locationOdometerService = locationOdometerService;
            Title = AppResources.SignInTitle;
            SignInCommand = new MvxAsyncCommand(ExecuteSignInCommandAsync, CanExecuteSignInCommand);
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

        private int? _odometer;
        public int? Odometer
        {
            get { return _odometer; }
            set
            {
                SetProperty(ref _odometer, value);
                SignInCommand.RaiseCanExecuteChanged();
            }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        public IMvxAsyncCommand SignInCommand { get; protected set; }

        protected async Task ExecuteSignInCommandAsync()
        {
            if (string.IsNullOrEmpty(PhoneSettings.ServerSettings))
            {
                UserDialogs.Instance.Toast(AppResources.NoServerSummary);
                return;
            }

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
                    return;

                ShowViewModel<MainViewModel>();
                Close(this);
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
                   && !string.IsNullOrWhiteSpace(TruckId)
                   && Odometer.HasValue;
        }

        private async Task<bool> SignInAsync()
        {
            using (var loginData = UserDialogs.Instance.Loading(AppResources.LoggingIn, maskType: MaskType.Black))
            {
                // Delete/Create necesscary SQLite tables
                await _dbService.RefreshAll();
                _connection.CreateConnection(MobileConstants.DefaultServiceProtocol + PhoneSettings.ServerSettings,
                    UserName, Password, MobileConstants.DefaultServiceName);
                _backgroundScheduler.Unschedule();
                _locationService.Stop();
                _locationOdometerService.Stop();
                _locationGeofenceService.Stop();
                _locationPathService.Stop();

                // Trying to push all remote calls via BWF down into a respective service, since however we don't
                // have a need for a login service, leaving this as is.
                var loginProcess = await _connection.GetConnection(ConnectionType.Online).UpdateAsync(
                    new DriverLoginProcess {
                        EmployeeId = UserName,
                        Password = Password,
                        PowerId = TruckId,
                        Odometer = Odometer,
                        LocaleCode = 1033,
                        OverrideFlag = Constants.No,
                        Mdtid = "Phone",
                        LoginDateTime = DateTime.Now
                    }, requeryUpdated: false);

                if (loginProcess.WasSuccessful)
                {
                    await _messagesService.UpdateApprovedUsersForMessaging(loginProcess.Item.UsersForMessaging);
                    CurrentDriver = new DriverStatusModel
                    {
                        EmployeeId = loginProcess.Item.EmployeeId,
                        Status = loginProcess.Item.DriverStatus ?? DriverStatusSRConstants.LoggedIn,
                        TerminalId = loginProcess.Item.TermId,
                        PowerId = loginProcess.Item.PowerId,
                        RegionId = loginProcess.Item.RegionId,
                        MDTId = loginProcess.Item.Mdtid,
                        LoginDateTime = loginProcess.Item.LoginDateTime,
                        ActionDateTime = loginProcess.Item.LoginDateTime,
                        Odometer = loginProcess.Item.Odometer,
                        LoginProcessedDateTime = loginProcess.Item.LoginDateTime,
                        DriverLCID = loginProcess.Item.LocaleCode,
                        TripNumber = loginProcess.Item.TripNumber,
                        TripSegNumber = loginProcess.Item.TripSegNumber
                    };

                    await _driverService.CreateDriverStatus(CurrentDriver);

                    // Get the EmployeeMaster record for the current driver and update local DB
                    var driverEmployeeRecord =
                        await _driverService.FindEmployeeMasterForDriverRemoteAsync(loginProcess.Item.EmployeeId);
                    await _driverService.UpdateDriverEmployeeRecord(driverEmployeeRecord);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(loginProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

                loginData.Title = AppResources.LoadingPreferences;

                // Retrieve preferences from remote server and populate local DB
                var preferenceProcess = await _preferenceService.FindPreferencesRemoteAsync(new PreferencesProcess { EmployeeId = UserName });

                if (preferenceProcess.WasSuccessful)
                {
                    await _preferenceService.UpdatePreferences(preferenceProcess.Item.Preferences);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(preferenceProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

                // Retrieve code table info from remote server and populate local DB
                var codesTable = await _codeTableService.FindCodesRemoteAsync(new CodeTableProcess { EmployeeId = UserName });

                if (codesTable.WasSuccessful)
                {
                    await _codeTableService.UpdateCodeTable(codesTable.Item.CodeTables);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(codesTable.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }
                
                // If there's no last updated for container settings, manually refresh the containers
                if (PhoneSettings.ContainerSettings == null)
                    await _dbService.RefreshTable<ContainerMasterModel>();

                // Retrieve container info from remote server and populate local DB
                var containerChanges =
                    await _containerService.ProcessContainerChangeAsync(new ContainerChangeProcess
                    {
                        EmployeeId = UserName,
                        LastContainerMasterUpdate = PhoneSettings.ContainerSettings.HasValue ? PhoneSettings.ContainerSettings.Value.ToLocalTime() : (DateTime?) null
                    });

                if (containerChanges.WasSuccessful)
                {
                    if (containerChanges.Item?.Containers?.Count > 0)
                        await _containerService.UpdateContainerChangeIntoMaster(containerChanges.Item.Containers);

                    PhoneSettings.ContainerSettings = DateTime.Now;
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(containerChanges.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

                // Update container master if driver has any containers on their vehicle
                await _containerService.UpdateContainerMaster(loginProcess.Item.ContainersOnPowerId);

                // Retrieve terminal change info and populate local DB
                var terminalChanges =
                    await _terminalService.FindTerminalChangesRemoteAsync(new TerminalChangeProcess
                    {
                        EmployeeId = UserName,
                        TerminalId = loginProcess.Item.TermId
                    });

                if (terminalChanges.WasSuccessful)
                {
                    if (terminalChanges.Item?.Terminals?.Count > 0)
                        await _terminalService.UpdateTerminalChangeIntoMaster(terminalChanges.Item.Terminals);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(terminalChanges.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

                // The long term fix for updating the menu driver information is to have the login & settings
                // view as their own activities, that way we don't have to worry about the menu trying to look
                // up information it doesn't have yet on initial app load, since that info is fetched during the login
                // The menu fragment doesn't need to be loaded on those views
                // @TODO : Implement above
                var employeePowerMaster = await _driverService.FindEmployeePowerMasterRemoteAsync(TruckId);
                await _driverService.UpdatePowerMasterRecord(employeePowerMaster);

                var employeeRecord = await _driverService.FindEmployeeAsync(UserName);
                var terminal = await _terminalService.FindTerminalMasterAsync(CurrentDriver.TerminalId);
                var power = await _driverService.FindPowerMasterAsync(TruckId);

                var messagesTable = await _messagesService.ProcessDriverMessagesAsync(new DriverMessageProcess { EmployeeId = UserName });

                if (messagesTable.WasSuccessful)
                {
                    if (messagesTable.Item?.Messages?.Count > 0)
                        await _messagesService.UpdateMessages(messagesTable.Item.Messages);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(messagesTable.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

                loginData.Title = AppResources.LoadingTripInformation;

                var tripProcess = await _tripService.ProcessTripInfoAsync(new TripInfoProcess
                {
                    EmployeeId = UserName
                });

                if (tripProcess.WasSuccessful)
                {
                    // @TODO : Should we throw an error/alert dialog to the end user if any of these fail?
                    if (tripProcess.Item?.Trips?.Count > 0)
                    {
                        await _tripService.UpdateTrips(tripProcess.Item.Trips);

                        // Acknowledge each trip
                        foreach (var trip in tripProcess.Item.Trips)
                        {
                            var tripAck = await _tripService.ProcessDriverTripAck(new DriverTripAckProcess
                            {
                                EmployeeId = UserName,
                                TripNumber = trip.TripNumber,
                                ActionDateTime = DateTime.Now,
                                Mdtid = UserName
                            });

                            if (!tripAck.WasSuccessful)
                                UserDialogs.Instance.Alert(tripAck.Failure.Summary, AppResources.Error);
                        }
                    }

                    if(tripProcess.Item?.TripSegments?.Count > 0)
                        await _tripService.UpdateTripSegments(tripProcess.Item.TripSegments);

                    if(tripProcess.Item?.TripSegmentContainers?.Count > 0)
                        await _tripService.UpdateTripSegmentContainers(tripProcess.Item.TripSegmentContainers);

                    if(tripProcess.Item?.CustomerCommodities?.Count > 0)
                        await _customerService.UpdateCustomerCommodity(tripProcess.Item.CustomerCommodities);

                    if(tripProcess.Item?.CustomerDirections?.Count > 0)
                        await _customerService.UpdateCustomerDirections(tripProcess.Item.CustomerDirections);

                    if(tripProcess.Item?.CustomerLocations?.Count > 0)
                        await _customerService.UpdateCustomerLocation(tripProcess.Item.CustomerLocations);

                    if (tripProcess.Item?.CustomerMasters?.Count > 0)
                        await _customerService.UpdateCustomerMaster(tripProcess.Item.CustomerMasters);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(tripProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }
            }

            _backgroundScheduler.Schedule(60000);
            _locationService.Start();
            _locationOdometerService.Start(Convert.ToDouble(Odometer));
            _locationGeofenceService.Start();
            _locationPathService.Start();
            return true;
        }

        private MvxCommand _settingsCommand;
        public MvxCommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new MvxCommand(ExecuteSettingsCommand));

        private void ExecuteSettingsCommand()
        {
            ShowViewModel<SettingsViewModel>();
        }
    }
}