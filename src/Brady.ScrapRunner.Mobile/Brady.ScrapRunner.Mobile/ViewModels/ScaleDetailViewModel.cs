using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class ScaleDetailViewModel : BaseViewModel
    {
        private INavigationService _navigationService;

        public ScaleDetailViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Yard/Scale";
            Container = CreateDummyData();

            GrossWeightSetCommand = new RelayCommand(ExecuteGrossWeightSetCommand);
            SecondGrossWeightSetCommand = new RelayCommand(ExecuteSecondGrossWeightSetCommand, IsGrossWeightSet);
            TareWeightSetCommand = new RelayCommand(ExecuteTareWeightSetCommand, IsGrossWeightSet);

            ContainerSetDownCommand = new RelayCommand(ExecuteContainerSetDownCommand);
            ContainerLeftOnTruckCommand = new RelayCommand(ExecuteContainerLeftOnTruckCommand);
        }

        // Field bindings
        // @TODO : In the near future, we could have multiple containers for bulk processing
        public ContainerMasterModel Container { get; private set; }

        private string _grossTime;
        public string GrossTime
        {
            get { return _grossTime; }
            set
            {
                Set(ref _grossTime, value);
                SecondGrossWeightSetCommand.RaiseCanExecuteChanged();
                TareWeightSetCommand.RaiseCanExecuteChanged();
            }
        }

        private string _secondGrossTime;
        public string SecondGrossTime
        {
            get { return _secondGrossTime; }
            set { Set(ref _secondGrossTime, value); }
        }

        private string _tareTime;
        public string TareTime
        {
            get { return _tareTime; }
            set { Set(ref _tareTime, value); }
        }

        // Command bindings
        public RelayCommand ContainerSetDownCommand { get; private set; }
        public RelayCommand ContainerLeftOnTruckCommand { get; private set; }
        public RelayCommand GrossWeightSetCommand { get; private set; }
        public RelayCommand TareWeightSetCommand { get; private set; }
        public RelayCommand SecondGrossWeightSetCommand { get; private set; }

        // Command impl
        public async void ExecuteContainerSetDownCommand()
        {
            // @TODO : Determine if this is the last set of containers for trip, then either navigate back to scale summary
            // @TODO : or finish route and navigate to route summary screen
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.SetDownContainerMessage, AppResources.SetDown);
            if (result)
            {
                // @TODO : Make sure these pages are removed from navigation stack
                _navigationService.NavigateTo(Locator.RouteSummaryView);
            }
        }

        public async void ExecuteContainerLeftOnTruckCommand()
        {
            // @TODO : Determine if this is the last set of containers for trip, then either navigate back to scale summary
            // @TODO : or finish route and navigate to route summary screen
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.LeftOnTruckContainerMessage, AppResources.LeftOnTruck);
            if (result)
            {
                // @TODO : Make sure these pages are removed from navigation stack
                _navigationService.NavigateTo(Locator.RouteSummaryView);
            }
        }

        public void ExecuteGrossWeightSetCommand()
        {
            GrossTime = DateTime.Now.ToString("hh:mm tt");
        }

        public void ExecuteSecondGrossWeightSetCommand()
        {
            SecondGrossTime = DateTime.Now.ToString("hh:mm tt");
        }

        public void ExecuteTareWeightSetCommand()
        {
            TareTime = DateTime.Now.ToString("hh:mm tt");
        }

        public bool IsGrossWeightSet()
        {
            return !string.IsNullOrWhiteSpace(GrossTime);
        }

        // @TODO : Refactor using repositories
        private ContainerMasterModel CreateDummyData()
        {
            return new ContainerMasterModel
            {
                ContainerNumber = "89999",
                ContainerType = "LUGGER",
                ContainerSize = "20",
                ContainerLocation = "DOCK DOOR 14",
                ContainerCustHostCode = "KAMAN450",
                ContainerStatus = "C",
                ContainerCommodityCode = "1234",
                ContainerCommodityDesc = "#15 SHEARING IRON"
            };
        }

    }
}
