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
            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var messages = await _messagesService.SortedDrvrMsgsAsync();
            var messagegroupedby = messages.GroupBy(m => m.SenderId).Select(grp => grp.First()).ToList();

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

        private IMvxCommand _messageSelectedCommand;
        public IMvxCommand MessageSelectedCommand => _messageSelectedCommand ?? (_messageSelectedCommand = new MvxCommand<MessagesModel>(ExecuteMessageSelectedCommand));

        public void ExecuteMessageSelectedCommand(MessagesModel messageModel)
        {
            ShowViewModel<NewMessageViewModel>(new { remoteUserId = messageModel.SenderId });
        }
    }
}
