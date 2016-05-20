using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Services;
using Brady.ScrapRunner.Mobile.Validators;
using BWF.DataServices.Metadata.Fluent.Utils;
using BWF.DataServices.PortableClients;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Localization;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Globalization;
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
            MessageSelectedCommand = new MvxCommand<MessagesModel>(ExecuteMessageSelectedCommand);
        }
        public override async void Start()
        {
            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var messages = await _messagesService.SortedDrvrMsgsAsync();
            Messages = new ObservableCollection<MessagesModel>(messages);

            if (Messages.Count == 0)
            {
                MessagesModel m1 = new MessagesModel();
                m1.MsgText = "Terminal is closing";
                m1.Ack = "N";
                m1.CreateDateTime = DateTime.Now;
                m1.MsgId = 1;
                m1.ReceiverId = "930";
                m1.SenderId = "Dispatcher #1";
                m1.ReceiverName = "Steve Hartman";
                m1.SenderName = "John Smith";

                Messages.Add(m1);

                MessagesModel m2 = new MessagesModel();
                m2.MsgText = "Be carefule on icy road";
                m2.Ack = "N";
                m2.CreateDateTime = DateTime.Now;
                m2.MsgId = 2;
                m2.ReceiverId = "930";
                m2.SenderId = "Dispatcher #2";
                m2.ReceiverName = "Steve Hartman";
                m2.SenderName = "Julian Northman";

                Messages.Add(m2);

            }

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
            set
            {
                _selectedMessage = value; RaisePropertyChanged(() => SelectedMessage);
            }
        }
        public MvxCommand<MessagesModel> MessageSelectedCommand { get; private set; }

        public void ExecuteMessageSelectedCommand(MessagesModel selectedMessage)
        {
            Close(this);
            ShowViewModel<NewMessageViewModel>(new { message = selectedMessage.SenderId });
        }
    }
}
