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

        public MenuViewModel(IConnectionService<DataServiceClient> connection)
        {
            _connection = connection;
        }

        private IMvxAsyncCommand _logoutCommand;
        public IMvxAsyncCommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new MvxAsyncCommand(ExecuteLogoutAsync));

        public async Task ExecuteLogoutAsync()
        {
            var logoutDialog = await UserDialogs.Instance.ConfirmAsync(
                AppResources.LogOutMessage, AppResources.LogOut, AppResources.Yes, AppResources.No);

            if (logoutDialog)
            {
                _connection.DeleteConnection();
                ShowViewModel<SignInViewModel>();
            }
        }
        private MvxCommand _fuelentryCommand;

        public MvxCommand FuelEntryCommand
            => _fuelentryCommand ?? (_fuelentryCommand = new MvxCommand(ExecuteFuelEntryCommand));

        public void ExecuteFuelEntryCommand()
        {
            ShowViewModel<FuelEntryViewModel>();
        }
    }
}
