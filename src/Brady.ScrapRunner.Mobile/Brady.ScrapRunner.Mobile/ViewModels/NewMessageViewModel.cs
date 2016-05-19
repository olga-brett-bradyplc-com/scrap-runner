using System;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;
using Brady.ScrapRunner.Mobile.Resources;
using Brady.ScrapRunner.Mobile.Validators;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class NewMessageViewModel : BaseViewModel
    {
        private readonly IMessagesService _messagesService;
        private readonly IDriverService _driverService;

        private string _messageText;

        public NewMessageViewModel(IDriverService driverService, 
                                   IMessagesService messagesService)
        {
            _driverService = driverService;
            _messagesService = messagesService;
            Title = AppResources.Message;
        }

        public override async void Start()
        {
            var _messages = await _messagesService.FindMsgsFromAsync(SenderId);

            base.Start();
        }

        private IMvxAsyncCommand _sendMessageCommand;
        public IMvxAsyncCommand SendMessageCommand => _sendMessageCommand ??
            (_sendMessageCommand = new MvxAsyncCommand(ExecuteSendMessageCommandAsync));

        protected async Task ExecuteSendMessageCommandAsync()
        {
            try
            {
                var sendMessageResult = await SaveSendMessageAsync();
                if (!sendMessageResult)
                    return;

                Close(this);
            }
            catch (Exception exception)
            {
                var message = exception?.InnerException?.Message ?? exception.Message;
                await UserDialogs.Instance.AlertAsync(
                    message, AppResources.Error, AppResources.OK);
            }
        }
        private async Task<bool> SaveSendMessageAsync()
        {
            using (var loginData = UserDialogs.Instance.Loading(AppResources.SavingData, maskType: MaskType.Black))
            {
                var currentUser = await _driverService.GetCurrentDriverStatusAsync();

                var message = await _driverService.SendMessageRemoveAsync(new DriverMessageProcess
                {
                    EmployeeId = currentUser.EmployeeId,
                    ActionDateTime = DateTime.Now,
                    SenderId = currentUser.EmployeeId,
                    ReceiverId = ReceiverId,
                    MessageText = MessageText,
                    MessageThread = 0,
                    UrgentFlag = "N"
                });

                if (message.WasSuccessful)
                {
                    //Add new message to the chain of messages
                    _messages.Add(new MessagesModel
                    {
                        TerminalId = currentUser.TerminalId,
                        CreateDateTime = DateTime.Now,
                        SenderId = currentUser.EmployeeId,
                        ReceiverId = SenderId,
                        MsgText = MessageText,
                        Ack = "N",
                        MessageThread = 1,
                        Area = currentUser.DriverArea,
                        SenderName = "", //TODO: get Sender's name
                        ReceiverName = "", //TODO: get Receiver's name
                        Urgent = "N",
                        Processed = "N",
                        MsgSource = "R",//Driver's source
                        DeleteFlag = "N"
                    });

                    _messageText = String.Empty;

                    return true;
                }

                await UserDialogs.Instance.AlertAsync(message.Failure.Summary,
                    AppResources.Error, AppResources.OK);
                return false;
            }
        }
        private string _senderId;
        public string SenderId
        {
            get { return _senderId; }
            set
            {
                SetProperty(ref _senderId, value);
            }
        }
        private ObservableCollection<MessagesModel> _messages;
        public ObservableCollection<MessagesModel> Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }

        public string MessageText
        {
            get { return _messageText; }
            set { SetProperty(ref _messageText, value); }
        }

        private string _receiverId;

        public string ReceiverId
        {
            get { return _receiverId; }
            set { SetProperty(ref _receiverId, value); }
        }

    }
}
