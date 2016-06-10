using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IConnectionService _connection;
        private readonly IQueueScheduler _queueScheduler;
        private readonly ICodeTableService _codeTableService;

        public MenuViewModel(IConnectionService connection, IQueueScheduler queueScheduler, ICodeTableService codeTableService)
        {
            _connection = connection;
            _queueScheduler = queueScheduler;
            _codeTableService = codeTableService;
        }

        private IMvxAsyncCommand _logoutCommand;
        public IMvxAsyncCommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new MvxAsyncCommand(ExecuteLogoutAsync));

        private async Task ExecuteLogoutAsync()
        {
            var logoutDialog = await UserDialogs.Instance.ConfirmAsync(
                AppResources.LogOutMessage, AppResources.LogOut, AppResources.Yes, AppResources.No);

            if (logoutDialog)
            {
                _queueScheduler.Unschedule();
                _connection.DeleteConnection();
                ShowViewModel<SignInViewModel>();
            }
        }
        //TODO: put in the appropriate spot, called on receiving logoff packet from dispatch (do we have mechanism for receiving packets yet?)
        private IMvxAsyncCommand _forcedLogoffCommand;
        public IMvxAsyncCommand ForcedLogoffCommand => _forcedLogoffCommand ?? (_forcedLogoffCommand = new MvxAsyncCommand(ExecuteForcedLogoffAsync));

        private async Task ExecuteForcedLogoffAsync()
        {
            await UserDialogs.Instance.AlertAsync(
                            AppResources.ForcedLogoffMessage, AppResources.ForcedLogoff, AppResources.OK);

            _queueScheduler.Unschedule();
            _connection.DeleteConnection();
            ShowViewModel<SignInViewModel>();
        }
        private IMvxCommand _fuelentryCommand;
        public IMvxCommand FuelEntryCommand
            => _fuelentryCommand ?? (_fuelentryCommand = new MvxCommand(ExecuteFuelEntryCommand));

        private void ExecuteFuelEntryCommand()
        {
            ShowViewModel<FuelEntryViewModel>();
        }

        private IMvxCommand _messagesCommand;
        public IMvxCommand MessagesCommand
            => _messagesCommand ?? (_messagesCommand = new MvxCommand(ExecuteMessagesCommand));

        private void ExecuteMessagesCommand()
        {
            ShowViewModel<MessagesViewModel>();
        }

        private IMvxCommand _newMessageCommand;
        public IMvxCommand NewMessageCommand
            => _newMessageCommand ?? (_newMessageCommand = new MvxCommand(ExecuteNewMessageCommand));

        private void ExecuteNewMessageCommand()
        {
            ShowViewModel<NewMessageViewModel>();
        }

        private IMvxAsyncCommand _delayCommandAsync;
        public IMvxAsyncCommand DelayCommandAsync
            => _delayCommandAsync ?? (_delayCommandAsync = new MvxAsyncCommand(ExecuteDelayComandAsync));

        private async Task ExecuteDelayComandAsync()
        {
            var delays = await _codeTableService.FindCodeTableList(CodeTableNameConstants.DelayCodes);
            var delayAlertAsync =
                await
                    UserDialogs.Instance.ActionSheetAsync("Select Delay Reason", "", "Cancel",
                        delays.Select(ct => ct.CodeDisp1).ToArray());

            // Hitting "Cancel" on an ActionSheet dialog returns a string of "Cancel" ...
            if (delayAlertAsync != "Cancel")
            {
                var delayReasonObj = delays.FirstOrDefault(ct => ct.CodeDisp1 == delayAlertAsync);
                ShowViewModel<DelayViewModel>(new {delayReason = delayReasonObj.CodeValue});
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
    }
}
