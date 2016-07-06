using System.Threading.Tasks;
using Acr.UserDialogs;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Brady.ScrapRunner.Mobile.Interfaces;
    using Brady.ScrapRunner.Mobile.Models;
    using System.Collections.ObjectModel;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class MessagesViewModel : BaseViewModel
    {
        private readonly IMessagesService _messagesService;
        private readonly IDriverService _driverService;

        public MessagesViewModel(IDriverService driverService,
                                  IMessagesService messagesService)
        {
            _driverService = driverService;
            _messagesService = messagesService;

            Title = AppResources.Messages;
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var messages = await _messagesService.FindMessagesAsync(CurrentDriver.EmployeeId);
            var messagegroupedby = messages.GroupBy(m => m.RemoteUserId).Select(grp => grp.First()).ToList();

            Messages = new ObservableCollection<MessagesModel>(messagegroupedby);
            base.Start();
        }

        private ObservableCollection<MessagesModel> _messages;
        public ObservableCollection<MessagesModel> Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }

        private MessagesModel _selectedMessage;
        public MessagesModel SelectedMessage
        {
            get { return _selectedMessage; }
            set { SetProperty(ref _selectedMessage, value); }
        }

        private DriverStatusModel CurrentDriver { get; set; }

        private IMvxCommand _messageSelectedCommand;
        public IMvxCommand MessageSelectedCommand => _messageSelectedCommand ?? (_messageSelectedCommand = new MvxCommand<MessagesModel>(ExecuteMessageSelectedCommand));

        public void ExecuteMessageSelectedCommand(MessagesModel messageModel)
        {
            ShowViewModel<NewMessageViewModel>(new
            {
                remoteUserId = messageModel.RemoteUserId,
                remoteUserFullName = messageModel.RemoteUserName
            });
        }

        private IMvxAsyncCommand _newMessageCommand;
        public IMvxAsyncCommand NewMessageCommand => _newMessageCommand ?? (_newMessageCommand = new MvxAsyncCommand(ExecuteNewMessageCommand));

        private async Task ExecuteNewMessageCommand()
        {
            var approvedUsers = await _messagesService.FindApprovedUsersForMessagingAsync();
            if (approvedUsers.Count > 0)
            {
                var approvedListAsync =
                    await
                        UserDialogs.Instance.ActionSheetAsync(AppResources.SelectUser, "", AppResources.Cancel, null,
                            approvedUsers.Select(u => u.FullName).ToArray());

                if (approvedListAsync != AppResources.Cancel)
                {
                    var user = approvedUsers.FirstOrDefault(u => u.FullName == approvedListAsync);
                    ShowViewModel<NewMessageViewModel>(
                        new { remoteUserId = user.EmployeeId, remoteUserFullName = user.FullName });
                }
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(AppResources.NoUsers, AppResources.Error);
            }
        }
    }
}
