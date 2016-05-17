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

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class NewMessageViewModel : BaseViewModel
    {
        private readonly IMessagesService _messagesService;

        private string _msgText;

        public NewMessageViewModel(IMessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        public void Init(int? msgId)
        {
            MsgId = msgId;
        }

        public override async void Start()
        {
            var message = await _messagesService.FindMessageAsync(MsgId);

            if (message != null)
            {
                _msgText = message.MsgText;
            }

            base.Start();
        }

        private int? _msgId;
        public int? MsgId
        {
            get { return _msgId; }
            set
            {
                SetProperty(ref _msgId, value);
            }
        }

        public string MsgText
        {
            get { return _msgText; }
            set { SetProperty(ref _msgText, value); }
        }

    }
}
