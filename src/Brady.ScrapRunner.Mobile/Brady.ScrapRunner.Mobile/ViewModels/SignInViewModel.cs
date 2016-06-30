using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.PortableClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Sqlite;
using Brady.ScrapRunner.Mobile.Resources;
using Brady.ScrapRunner.Mobile.Services;
using Brady.ScrapRunner.Mobile.Validators;
using Plugin.Settings.Abstractions;

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
        private readonly IQueueScheduler _queueScheduler;
        private readonly ILocationService _locationService;
        private readonly ILocationOdometerService _locationOdometerService;
        
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
            IQueueScheduler queueScheduler, 
            ILocationService locationService, 
            ILocationOdometerService locationOdometerService)
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

            _connection = connection;
            _queueScheduler = queueScheduler;
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

        public IMvxAsyncCommand SignInCommand { get; protected set; }

        protected async Task ExecuteSignInCommandAsync()
        {
            if (string.IsNullOrEmpty(PhoneSettings.ServerSettings))
            {
                UserDialogs.Instance.ErrorToast(AppResources.NoServerEndpoint, AppResources.NoServerSummary);
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

                Close(this);

                var containers = await _containerService.FindPowerIdContainersAsync(TruckId);

                // @TODO : This is preferenced driven as to whether we take them to LoadDrop screen if containers are on truck
                if (containers.Any())
                    ShowViewModel<LoadDropContainerViewModel>(new { loginProcessed = true });
                else
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
            using (var loginData = UserDialogs.Instance.Loading(AppResources.LoggingIn, maskType: MaskType.Black))
            {
                // Delete/Create necesscary SQLite tables
                await _dbService.RefreshAll();
                IClientSettings clientSettings = new DemoClientSettings();
                _connection.CreateConnection(PhoneSettings.ServerSettings, UserName, Password, "ScrapRunner");

                _queueScheduler.Unschedule();
                _locationService.Stop();
                _locationOdometerService.Stop();

                // Trying to push all remote calls via BWF down into a respective service, since however we don't
                // have a need for a login service, leaving this as it is.
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
                    await _containerService.UpdateContainerMaster(loginProcess.Item.ContainersOnPowerId);
                    await _messagesService.UpdateApprovedUsersForMessaging(loginProcess.Item.UsersForMessaging);
                    await _driverService.UpdateDriverStatus(new DriverStatus
                    {
                        EmployeeId = loginProcess.Item.EmployeeId,
                        Status = DriverStatusSRConstants.LoggedIn,
                        TerminalId = loginProcess.Item.TermId,
                        PowerId = loginProcess.Item.PowerId,
                        RegionId = loginProcess.Item.RegionId,
                        MDTId = loginProcess.Item.Mdtid,
                        LoginDateTime = loginProcess.Item.LoginDateTime,
                        ActionDateTime = loginProcess.Item.LoginDateTime,
                        //DriverCumMinutes = loginProcess.Item.DriverCumlMinutes @TODO Should this be sent in login process?
                        Odometer = loginProcess.Item.Odometer,
                        LoginProcessedDateTime = loginProcess.Item.LoginDateTime,
                        DriverLCID = loginProcess.Item.LocaleCode
                    });

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
                var preferenceObj = new PreferencesProcess { EmployeeId = UserName };
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
                    await _dbService.RefreshTable<ContainerChangeModel>();

                // Retrieve container info from remote server and populate local DB
                var containerChanges =
                    await _containerService.ProcessContainerChangeAsync(new ContainerChangeProcess
                    {
                        EmployeeId = UserName,
                        LastContainerMasterUpdate = PhoneSettings.ContainerSettings.HasValue ? PhoneSettings.ContainerSettings.Value.ToLocalTime() : (DateTime?) null
                    });

                if (containerChanges.WasSuccessful)
                {
                    PhoneSettings.ContainerSettings = DateTime.Now;
                    if (containerChanges.Item?.Containers?.Count > 0)
                        await _containerService.UpdateContainerChange(containerChanges.Item.Containers);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(containerChanges.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

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
                        await _terminalService.UpdateTerminalChange(terminalChanges.Item.Terminals);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(terminalChanges.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

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
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(tripProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }

            }
            _queueScheduler.Schedule(60000);
            _locationService.Start();
            _locationOdometerService.Start(Convert.ToDouble(Odometer));
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