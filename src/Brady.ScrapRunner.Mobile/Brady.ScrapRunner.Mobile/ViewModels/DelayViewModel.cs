using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class DelayViewModel : BaseViewModel
    {
        private readonly ICodeTableService _codeTableService;

        public DelayViewModel(ICodeTableService codeTableService)
        {
            _codeTableService = codeTableService;
            Title = AppResources.Delay;
        }

        public void Init(string delayReason)
        {
            DelayReason = delayReason;
        }

        public override async void Start()
        {
            var reasonList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.DelayCodes);
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
            var confirm = await UserDialogs.Instance.ConfirmAsync(AppResources.ConfirmBackOnDuty, AppResources.BackOnDuty, AppResources.Yes, AppResources.No);
            if (confirm)
                Close(this);
        }
    }
}
