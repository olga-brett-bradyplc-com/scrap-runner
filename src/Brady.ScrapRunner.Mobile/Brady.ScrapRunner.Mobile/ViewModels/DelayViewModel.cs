using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class DelayViewModel : BaseViewModel
    {
        private readonly ICodeTableService _codeTableService;

        public DelayViewModel(ICodeTableService codeTableService)
        {
            _codeTableService = codeTableService;
            Title = "Delay";
        }

        public void Init(string delayReason)
        {
            DelayReason = delayReason;
        }

        public override async void Start()
        {
            var reasonList = await _codeTableService.FindCodeTableList("DELAYCODES");
            var reasonDesc = reasonList.FirstOrDefault(ct => ct.CodeValue == DelayReason);
            SubTitle = reasonDesc.CodeDisp1;
        }

        private string _delayReason;
        public string DelayReason
        {
            get { return _delayReason; }
            set { SetProperty(ref _delayReason, value); }
        }

        private IMvxAsyncCommand _backOnDutyCommand;
        public IMvxAsyncCommand BackOnDutyCommand
            => _backOnDutyCommand ?? (_backOnDutyCommand = new MvxAsyncCommand(ExecuteBackOnDutyCommand));

        private async Task ExecuteBackOnDutyCommand()
        {
            var confirm = await UserDialogs.Instance.ConfirmAsync("Are You Back On Duty?", "Confirm", "OK", "Cancel");
            if (confirm)
            {
                Close(this);
            }
        }
    }
}
