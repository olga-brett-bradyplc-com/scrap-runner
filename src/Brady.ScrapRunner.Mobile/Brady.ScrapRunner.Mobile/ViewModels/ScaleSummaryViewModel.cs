using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class ScaleSummaryViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public ScaleSummaryViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Yard/Scale";
            ItemList = CreateDummyData();
            ContainerSelectedCommand = new RelayCommand(ExecuteContainerSelectedCommand);
        }

        // Listview bindings
        public ObservableCollection<ContainerMasterModel> ItemList { get; private set; }

        // Command bindings
        public RelayCommand ContainerSelectedCommand { get; private set; }

        // Field bindings
        private ContainerMasterModel _containerSelected;
        public ContainerMasterModel ContainerSelected
        {
            get { return _containerSelected; }
            set { Set(ref _containerSelected, value); }
        }

        // Command impl
        public void ExecuteContainerSelectedCommand()
        {
            _navigationService.NavigateTo(Locator.ScaleDetailView);
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
