using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class DelayViewModel : BaseViewModel
    {
        private readonly ICodeTableService _codeTableService;
        private readonly IDriverService _driverService;

        public DelayViewModel(ICodeTableService codeTableService, IDriverService driverService)
        {
            _driverService = driverService;
            _codeTableService = codeTableService;
            Title = AppResources.Delay;
        }

        public void Init(string delayCode, string delayReason)
        {
            DelayCode = delayCode;
            DelayReason = delayReason;
        }

        public override async void Start()
        {
            var reasonList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.DelayCodes);
            var reasonDesc = reasonList.FirstOrDefault(ct => ct.CodeValue == DelayReason);
            SubTitle = reasonDesc.CodeDisp1;
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var delay = await _driverService.ProcessDriverDelayAsync(new DriverDelayProcess
            {
                EmployeeId = CurrentDriver.EmployeeId,
                PowerId = CurrentDriver.PowerId,
                ActionType = DelayActionTypeConstants.Delay,
                ActionDateTime = DateTime.Now,
                TripNumber = CurrentDriver.TripNumber,
                TripSegNumber = CurrentDriver.TripSegNumber,
                DelayCode = DelayCode
            });

            if (!delay.WasSuccessful)
            {
                UserDialogs.Instance.Alert(delay.Failure.Summary, AppResources.Error);
                Close(this);
            }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        private string _delayCode;
        public string DelayCode
        {
            get { return _delayCode; }
            set { SetProperty(ref _delayCode, value); }
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
            {
                var delay = await _driverService.ProcessDriverDelayAsync(new DriverDelayProcess
                {
                    EmployeeId = CurrentDriver.EmployeeId,
                    PowerId = CurrentDriver.PowerId,
                    ActionType = DelayActionTypeConstants.BackOnDuty,
                    ActionDateTime = DateTime.Now,
                    TripNumber = CurrentDriver.TripNumber,
                    TripSegNumber = CurrentDriver.TripSegNumber,
                    DelayCode = DelayCode
                });

                if (delay.WasSuccessful)
                    Close(this);
                else
                    UserDialogs.Instance.Alert(delay.Failure.Summary, AppResources.Error);
            }
        }
    }
}
