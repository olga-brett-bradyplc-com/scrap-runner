using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Resources;
using BWF.DataServices.PortableClients;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.File;
using MvvmCross.Plugins.PictureChooser;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IConnectionService _connection;
        private readonly IQueueScheduler _queueScheduler;
        private readonly ICodeTableService _codeTableService;
        private readonly IMvxPictureChooserTask _pictureChooserTask;
        private readonly IMvxFileStore _fileStore;

        public MenuViewModel(IConnectionService connection, IQueueScheduler queueScheduler, ICodeTableService codeTableService, IMvxPictureChooserTask pictureChooserTask, IMvxFileStore fileStore)
        {
            _connection = connection;
            _queueScheduler = queueScheduler;
            _codeTableService = codeTableService;
            _pictureChooserTask = pictureChooserTask;
            _fileStore = fileStore;
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

        private IMvxCommand _takePictureCommand;
        public IMvxCommand TakePictureCommand
            => _takePictureCommand ?? (_takePictureCommand = new MvxCommand(ExecuteTakePictureCommand));

        private void ExecuteTakePictureCommand()
        {
            _pictureChooserTask.TakePicture(400, 50, OnPictureTaken, () => {/*nothing on cancel*/});
        }

        private void OnPictureTaken(Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            PictureBytes = memoryStream.ToArray();

            var randomFileName = "SRImage" + Guid.NewGuid().ToString("N");
            _fileStore.EnsureFolderExists("Images");
            var path = _fileStore.PathCombine("Images", randomFileName);
            _fileStore.WriteFile(path, PictureBytes);

            //ShowViewModel<PhotosViewModel>();
        }

        private byte[] _pictureBytes;
        public byte[] PictureBytes
        {
            get { return _pictureBytes; }
            set { SetProperty(ref _pictureBytes, value); }
        }
    }
}
