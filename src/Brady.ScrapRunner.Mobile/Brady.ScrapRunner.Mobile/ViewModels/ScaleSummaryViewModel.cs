namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class ScaleSummaryViewModel : BaseViewModel
    {
        public ScaleSummaryViewModel()
        {
            Title = "Yard/Scale";
            ItemList = CreateDummyData();
            ContainerSelectedCommand = new MvxCommand(ExecuteContainerSelectedCommand);
        }

        // Listview bindings
        public ObservableCollection<ContainerMasterModel> ItemList { get; private set; }

        // Command bindings
        public MvxCommand ContainerSelectedCommand { get; private set; }

        // Field bindings
        private ContainerMasterModel _containerSelected;
        public ContainerMasterModel ContainerSelected
        {
            get { return _containerSelected; }
            set { SetProperty(ref _containerSelected, value); }
        }

        // Command impl
        public void ExecuteContainerSelectedCommand()
        {
            Close(this);
            ShowViewModel<ScaleDetailViewModel>();
        }

        // @TODO : Refactor using repositories
        private ObservableCollection<ContainerMasterModel> CreateDummyData()
        {
            return new ObservableCollection<ContainerMasterModel>
            {
                new ContainerMasterModel
                {
                    ContainerNumber = "89999",
                    ContainerType = "LUGGER",
                    ContainerSize = "20",
                    ContainerLocation = "DOCK DOOR 14",
                    ContainerCustHostCode = "KAMAN450",
                    ContainerStatus = "C",
                    ContainerCommodityCode = "1234",
                    ContainerCommodityDesc = "#15 SHEARING IRON"
                }
            };
        }
    }
}
