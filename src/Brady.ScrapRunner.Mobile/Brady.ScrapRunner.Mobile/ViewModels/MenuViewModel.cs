using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IConnectionService _connection;
        private readonly IBackgroundScheduler _backgroundScheduler;
        private readonly ICodeTableService _codeTableService;
        private readonly IMessagesService _messagesService;
        private readonly ILocationService _locationService;
        private readonly IDriverService _driverService;
        private readonly ITerminalService _terminalService;
        private readonly ILocationGeofenceService _locationGeofenceService;
        private readonly ILocationOdometerService _locationOdometerService;
        private readonly ILocationPathService _locationPathService;

        public MenuViewModel(IConnectionService connection, 
            IBackgroundScheduler backgroundScheduler, 
            ICodeTableService codeTableService, 
            ILocationService locationService, 
            IMessagesService messageService,
            IDriverService driverService,
            ITerminalService terminalService, 
            ILocationGeofenceService locationGeofenceService, 
            ILocationOdometerService locationOdometerService, 
            ILocationPathService locationPathService)
        {
            _connection = connection;
            _backgroundScheduler = backgroundScheduler;
            _codeTableService = codeTableService;
            _locationService = locationService;
            _messagesService = messageService;
            _driverService = driverService;
            _terminalService = terminalService;
            _locationGeofenceService = locationGeofenceService;
            _locationOdometerService = locationOdometerService;
            _locationPathService = locationPathService;
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var employee = await _driverService.FindEmployeeAsync(CurrentDriver.EmployeeId);
            var terminal = await _terminalService.FindTerminalMasterAsync(CurrentDriver.TerminalId);
            var powermaster = await _driverService.FindPowerMasterAsync(CurrentDriver.PowerId);

            DriverFullName = employee.FullName;
            DriverYard = $"{CurrentDriver.TerminalId} - {terminal.TerminalName}";
            DriverVehicle = $"{CurrentDriver.PowerId} - {powermaster.PowerDesc}";
        }
        
        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        private string _driverFullname;
        public string DriverFullName
        {
            get { return _driverFullname; }
            set { SetProperty(ref _driverFullname, value); }
        }

        private string _driverYard;
        public string DriverYard
        {
            get { return _driverYard; }
            set { SetProperty(ref _driverYard, value); }
        }

        private string _driverVehicle;
        public string DriverVehicle
        {
            get { return _driverVehicle; }
            set { SetProperty(ref _driverVehicle, value); }
        }

        private IMvxAsyncCommand _logoutCommand;
        public IMvxAsyncCommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new MvxAsyncCommand(ExecuteLogoutAsync));

        private void Logout()
        {
            _backgroundScheduler.Unschedule();

            _locationService.Stop();
            _locationOdometerService.Stop();
            _locationGeofenceService.Stop();
            _locationPathService.Stop();

            _connection.DeleteConnection();

            ShowViewModel<SignInViewModel>();
        }

        private async Task ExecuteLogoutAsync()
        {
            var logoutDialog = await UserDialogs.Instance.ConfirmAsync(
                AppResources.LogOutMessage, AppResources.LogOut, AppResources.Yes, AppResources.No);

            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

            if (logoutDialog)
            {
                var logoffProcess = await _driverService.ProcessDriverLogoff(new DriverLogoffProcess
                {
                    EmployeeId = currentDriver.EmployeeId,
                    PowerId = currentDriver.PowerId,
                    Odometer = currentDriver.Odometer,
                    ActionDateTime = DateTime.Now
                });

                // The question is, should we stop the logoff process if we encounter an error here, or continue and just present them with a dialog?
                if (!logoffProcess.WasSuccessful)
                    UserDialogs.Instance.Alert(logoffProcess.Failure.Summary, AppResources.Error);

                Logout();
            }
        }

        //put in the appropriate spot, called on receiving logoff packet from dispatch (do we have mechanism for receiving packets yet?)
        private IMvxAsyncCommand _forcedLogoffCommand;
        public IMvxAsyncCommand ForcedLogoffCommand => _forcedLogoffCommand ?? (_forcedLogoffCommand = new MvxAsyncCommand(ExecuteForcedLogoffAsync));

        private async Task ExecuteForcedLogoffAsync()
        {
            await UserDialogs.Instance.AlertAsync(
                            AppResources.ForcedLogoffMessage, AppResources.ForcedLogoff, AppResources.OK);
            Logout();
        }
        private IMvxCommand _fuelentryCommand;
        public IMvxCommand FuelEntryCommand
            => _fuelentryCommand ?? (_fuelentryCommand = new MvxCommand(ExecuteFuelEntryCommand));

        private void ExecuteFuelEntryCommand()
        {
            ShowViewModel<FuelEntryViewModel>();
        }

        private IMvxCommand _routeSummaryCommand;
        public IMvxCommand RouteSummaryCommand => _routeSummaryCommand ?? (_routeSummaryCommand = new MvxCommand(ExecuteRouteSummaryCommand));

        private void ExecuteRouteSummaryCommand()
        {
            ShowViewModel<RouteSummaryViewModel>();
        }

        private IMvxCommand _messagesCommand;
        public IMvxCommand MessagesCommand
            => _messagesCommand ?? (_messagesCommand = new MvxCommand(ExecuteMessagesCommand));

        private void ExecuteMessagesCommand()
        {
            ShowViewModel<MessagesViewModel>();
        }

        private IMvxAsyncCommand _newMessageCommand;
        public IMvxAsyncCommand NewMessageCommand
            => _newMessageCommand ?? (_newMessageCommand = new MvxAsyncCommand(ExecuteNewMessageCommandAsync));

        private async Task ExecuteNewMessageCommandAsync()
        {
            var approvedUsers = await _messagesService.FindApprovedUsersForMessagingAsync();
            if (approvedUsers.Count > 0)
            {
                var approvedListAsync =
                    await
                        UserDialogs.Instance.ActionSheetAsync(AppResources.SelectUser, "", AppResources.Cancel, null,
                            approvedUsers.Select(u => u.FullName).ToArray());

                if (approvedListAsync != AppResources.Cancel && !string.IsNullOrEmpty(approvedListAsync))
                {
                    var user = approvedUsers.FirstOrDefault(u => u.FullName == approvedListAsync);
                    ShowViewModel<NewMessageViewModel>(
                        new {remoteUserId = user.EmployeeId, remoteUserFullName = user.FullName});
                }
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(AppResources.NoUsers, AppResources.Error);
            }
        }

        private IMvxAsyncCommand _delayCommandAsync;
        public IMvxAsyncCommand DelayCommandAsync
            => _delayCommandAsync ?? (_delayCommandAsync = new MvxAsyncCommand(ExecuteDelayComandAsync));

        private async Task ExecuteDelayComandAsync()
        {
            var delays = await _codeTableService.FindCodeTableList(CodeTableNameConstants.DelayCodes);
            var delayAlertAsync =
                await
                    UserDialogs.Instance.ActionSheetAsync(AppResources.SelectDelay, "", AppResources.Cancel, null,
                        delays.Select(ct => ct.CodeDisp1).ToArray());
            
            if (delayAlertAsync != AppResources.Cancel && !string.IsNullOrEmpty(delayAlertAsync))
            {
                var delayReasonObj = delays.FirstOrDefault(ct => ct.CodeDisp1 == delayAlertAsync);
                ShowViewModel<DelayViewModel>(new {delayCode = delayReasonObj.CodeValue, delayReason = delayReasonObj.CodeValue});
            }
        }

        private IMvxCommand _takePictureCommand;
        public IMvxCommand TakePictureCommand
            => _takePictureCommand ?? (_takePictureCommand = new MvxCommand(ExecuteTakePictureCommand));

        private void ExecuteTakePictureCommand()
        {
            ShowViewModel<PhotosViewModel>();
        }

        private IMvxCommand _loadDropContainersCommand;
        public IMvxCommand LoadDropContainersCommand
            => _loadDropContainersCommand ?? (_loadDropContainersCommand = new MvxCommand(ExecuteLoadDropContainersCommand));

        private void ExecuteLoadDropContainersCommand()
        {
            ShowViewModel<LoadDropContainerViewModel>();
        }

        private IMvxAsyncCommand _changeOdometerCommand;
        public IMvxAsyncCommand ChangeOdometerCommand => _changeOdometerCommand ?? ( _changeOdometerCommand = new MvxAsyncCommand(ExecuteChangeOdometer));

        private async Task ExecuteChangeOdometer()
        {
            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var odometerPrompt = await UserDialogs.Instance.PromptAsync(AppResources.EnterOdometer, AppResources.OdometerReadingHint,
                AppResources.OK, AppResources.Cancel, "", InputType.Number);

            if (!string.IsNullOrEmpty(odometerPrompt.Text))
            {
                using ( var loginData = UserDialogs.Instance.Loading(AppResources.Loading, maskType: MaskType.Black))
                {
                    var processOdom = await _driverService.ProcessDriverOdomUpdateAsync(new DriverOdomUpdateProcess
                    {
                        EmployeeId = currentDriver.EmployeeId,
                        ActionDateTime = DateTime.Now,
                        PowerId = currentDriver.PowerId,
                        Odometer = int.Parse(odometerPrompt.Text),
                        Mdtid = currentDriver.EmployeeId
                    });

                    if (!processOdom.WasSuccessful)
                        UserDialogs.Instance.Alert(processOdom.Failure.Summary, AppResources.Error);
                    else
                    {
                        currentDriver.Odometer = int.Parse(odometerPrompt.Text);
                        await _driverService.UpdateDriver(currentDriver);
                    }
                }
            }
        }
    }
}
