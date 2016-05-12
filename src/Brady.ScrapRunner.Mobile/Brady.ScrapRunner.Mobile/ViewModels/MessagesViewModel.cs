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

            var messages = await _messagesService.FindDrvrMsgsAsync(currentDriver.EmployeeId);
            Messages = new ObservableCollection<MessagesModel>(messages);

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
            Close(this); // temporary fix
            ShowViewModel<NewMessageViewModel>(new { message = selectedMessage.MsgId });
        }
    }
}
