namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using Acr.UserDialogs;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class ScaleDetailViewModel : BaseViewModel
    {
        public ScaleDetailViewModel()
        {
            Title = "Yard/Scale";
            Container = CreateDummyData();

            GrossWeightSetCommand = new MvxCommand(ExecuteGrossWeightSetCommand);
            SecondGrossWeightSetCommand = new MvxCommand(ExecuteSecondGrossWeightSetCommand, IsGrossWeightSet);
            TareWeightSetCommand = new MvxCommand(ExecuteTareWeightSetCommand, IsGrossWeightSet);

            ContainerSetDownCommand = new MvxCommand(ExecuteContainerSetDownCommand);
            ContainerLeftOnTruckCommand = new MvxCommand(ExecuteContainerLeftOnTruckCommand);
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
                SetProperty(ref _grossTime, value);
                SecondGrossWeightSetCommand.RaiseCanExecuteChanged();
                TareWeightSetCommand.RaiseCanExecuteChanged();
            }
        }

        private string _secondGrossTime;
        public string SecondGrossTime
        {
            get { return _secondGrossTime; }
            set { SetProperty(ref _secondGrossTime, value); }
        }

        private string _tareTime;
        public string TareTime
        {
            get { return _tareTime; }
            set { SetProperty(ref _tareTime, value); }
        }

        // Command bindings
        public MvxCommand ContainerSetDownCommand { get; private set; }
        public MvxCommand ContainerLeftOnTruckCommand { get; private set; }
        public MvxCommand GrossWeightSetCommand { get; private set; }
        public MvxCommand TareWeightSetCommand { get; private set; }
        public MvxCommand SecondGrossWeightSetCommand { get; private set; }

        // Command impl
        public async void ExecuteContainerSetDownCommand()
        {
            // @TODO : Determine if this is the last set of containers for trip, then either navigate back to scale summary
            // @TODO : or finish route and navigate to route summary screen
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.SetDownContainerMessage, AppResources.SetDown);
            if (result)
            {
                Close(this);
            }
        }

        public async void ExecuteContainerLeftOnTruckCommand()
        {
            // @TODO : Determine if this is the last set of containers for trip, then either navigate back to scale summary
            // @TODO : or finish route and navigate to route summary screen
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.LeftOnTruckContainerMessage, AppResources.LeftOnTruck);
            if (result)
            {
                Close(this);
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
