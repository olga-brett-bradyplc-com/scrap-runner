namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using BWF.DataServices.Domain.Models;
    using Domain;
    using Domain.Models;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Platform;
    using Resources;

    public class MessageNotificationViewModel : BaseViewModel
    {
        private readonly IMessagesService _messagesService;
        private readonly IConnectionService _connectionService;
        private int _messageId;

        public MessageNotificationViewModel(IMessagesService messagesService, IConnectionService connectionService)
        {
            _messagesService = messagesService;
            _connectionService = connectionService;
        }

        private string _notificationMessage;
        public string NotificationMessage
        {
            get { return _notificationMessage; }
            set { SetProperty(ref _notificationMessage, value); }
        }

        private string _messageText;
        public string MessageText
        {
            get { return _messageText; }
            set { SetProperty(ref _messageText, value); }
        }

        public void Init(int messageId)
        {
            _messageId = messageId;
        }

        public override async void Start()
        {
            try
            {
                base.Start();
                await StartAsync();
            }
            catch (Exception e)
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Error starting MessageNotificationViewModel: {e.Message}");
            }
        }

        private async Task StartAsync()
        {
            var message = await GetMessageAsync();
            if (message != null)
            {
                Title = AppResources.NotificationNewMessage;
                NotificationMessage = string.Format(AppResources.MessageNotificationMessage,
                    message.SenderName.TrimEnd(),
                    message.CreateDateTime.ToString("g"));
                MessageText = message.MsgText;
            }
        }

        private MvxAsyncCommand _ackCommand;
        public IMvxAsyncCommand AckCommand => _ackCommand ?? (_ackCommand = new MvxAsyncCommand(ExecuteAckCommandAsync));

        private async Task<MessagesModel> GetMessageAsync()
        {
            var message = await _messagesService.FindMessageAsync(_messageId);
            if (message == null)
            {
                Mvx.TaggedWarning(Constants.ScrapRunner, $"Failed to find message {_messageId}");
                return null;
            }
            return message;
        }

        private async Task AckMessageAsync()
        {
            var message = await GetMessageAsync();
            if (message != null)
            {
                var mappedMessage = Mapper.Map<MessagesModel, Messages>(message);
                mappedMessage.Processed = Constants.Yes;
                mappedMessage.Ack = Constants.Yes;
                var updateChangeSet = new ChangeSet<int, Messages>();
                updateChangeSet.AddUpdate(mappedMessage.Id, mappedMessage);
                await _connectionService.GetConnection(ConnectionType.Offline).ProcessChangeSetAsync(updateChangeSet);
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Ack Message {mappedMessage.Id}");
            }
        }

        private async Task ExecuteAckCommandAsync()
        {
            await AckMessageAsync();
            Close(this);
        }
    }
}