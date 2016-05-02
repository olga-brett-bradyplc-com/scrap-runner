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
    using Services;

    public class MenuViewModel : BaseViewModel
    {
        private readonly IConnectionService<OfflineCapableDataServiceClient> _connection;
        private readonly IQueueScheduler _queueScheduler;

        public MenuViewModel(IConnectionService<OfflineCapableDataServiceClient> connection, IQueueScheduler queueScheduler)
        {
            _connection = connection;
            _queueScheduler = queueScheduler;
        }

        private MvxCommand _logoutCommand;
        public MvxCommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new MvxCommand(ExecuteLogout));

        public async void ExecuteLogout()
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
    }
}
