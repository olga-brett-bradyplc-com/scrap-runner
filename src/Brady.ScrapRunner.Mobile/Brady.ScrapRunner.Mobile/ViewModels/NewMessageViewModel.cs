﻿using System;
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

        public NewMessageViewModel(IDriverService driverService, IMessagesService messagesService)
        {
            _driverService = driverService;
            _messagesService = messagesService;
            Title = AppResources.Message;
        }

        public void Init(string remoteUserId, string remoteUserFullName)
        {
            RemoteUserId = remoteUserId;
            RemoteUserFullName = remoteUserFullName;
        }

        public override async void Start()
        {
            SubTitle = RemoteUserFullName;
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            LocalUserId = CurrentDriver.EmployeeId;

            var messages = await _messagesService.FindMessagesAsync(RemoteUserId);
            Messages = new ObservableCollection<MessagesModel>(messages);
            base.Start();
        }
        
        private string _remoteUserId;
        public string RemoteUserId
        {
            get { return _remoteUserId; }
            set { SetProperty(ref _remoteUserId, value); }
        }

        private string _remoteUserFullName;
        public string RemoteUserFullName
        {
            get { return _remoteUserFullName; }
            set { SetProperty(ref _remoteUserFullName, value); }
        }

        private ObservableCollection<MessagesModel> _messages;
        public ObservableCollection<MessagesModel> Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }

        private string _messageText;
        public string MessageText
        {
            get { return _messageText; }
            set { SetProperty(ref _messageText, value); }
        }

        private string _localUserId;
        public string LocalUserId
        {
            get { return _localUserId; }
            set { SetProperty(ref _localUserId, value); }
        }

        private DriverStatusModel CurrentDriver { get; set; }

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
                var message = await _driverService.ProcessDriverMessageAsync(new DriverMessageProcess
                {
                    EmployeeId = LocalUserId,
                    ActionDateTime = DateTime.Now,
                    SenderId = LocalUserId.Trim(),
                    ReceiverId = RemoteUserId.Trim(),
                    MessageText = MessageText,
                    MessageThread = 0,
                    UrgentFlag = Constants.No
                });

                if (message.WasSuccessful)
                {
                    var receiverName = await _messagesService.FindEmployeeAsync(RemoteUserId);
                    var senderName = await _messagesService.FindEmployeeAsync(LocalUserId);
                    var localMessage = new MessagesModel
                    {
                        CreateDateTime = DateTime.Now,
                        SenderId = LocalUserId,
                        ReceiverId = RemoteUserId,
                        ReceiverName = receiverName.FullName,
                        SenderName = senderName.FullName,
                        MsgText = MessageText,
                        Ack = "N",
                        MessageThread = 1,
                        Urgent = Constants.No,
                        Processed = Constants.No,
                        MsgSource = "R", //Driver's source
                        DeleteFlag = Constants.No
                    };

                    await _messagesService.CreateMessageAsync(localMessage);

                    // Clear the typed message
                    MessageText = "";

                    //Add new message to the chain of messages
                    Messages.Add(localMessage);

                    return true;
                }

                await UserDialogs.Instance.AlertAsync(message.Failure.Summary,
                    AppResources.Error, AppResources.OK);
                return false;
            }
        }
    }
}
