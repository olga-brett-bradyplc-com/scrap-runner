using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Resources;
using BWF.DataServices.PortableClients;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IConnectionService<DataServiceClient> _connection;
        private readonly ICodeTableService _codeTableService;

        public MenuViewModel(IConnectionService<DataServiceClient> connection, ICodeTableService codeTableService)
        {
            _connection = connection;
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
                _connection.DeleteConnection();
                ShowViewModel<SignInViewModel>();
            }
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
            ShowViewModel<MessagesViewModel>(_connection);
        }

        private IMvxCommand _newMessageCommand;
        public IMvxCommand NewMessageCommand
            => _newMessageCommand ?? (_newMessageCommand = new MvxCommand(ExecuteNewMessageCommand));

        private void ExecuteNewMessageCommand()
        {
            ShowViewModel<NewMessageViewModel>(_connection);
        }

        private IMvxAsyncCommand _delayCommandAsync;
        public IMvxAsyncCommand DelayCommandAsync
            => _delayCommandAsync ?? (_delayCommandAsync = new MvxAsyncCommand(ExecuteDelayComandAsync));

        private async Task ExecuteDelayComandAsync()
        {
            var delays = await _codeTableService.FindCodeTableList("DELAYCODES");
            var delayAlertAsync =
                await
                    UserDialogs.Instance.ActionSheetAsync("Select Delay Reason", "", "Cancel",
                        delays.Select(ct => ct.CodeDisp1).ToArray());

            if (delayAlertAsync != "Cancel")
            {
                var delayReasonObj = delays.FirstOrDefault(ct => ct.CodeDisp1 == delayAlertAsync);
                ShowViewModel<DelayViewModel>(new {delayReason = delayReasonObj.CodeValue});
            }

        }
    }
}
