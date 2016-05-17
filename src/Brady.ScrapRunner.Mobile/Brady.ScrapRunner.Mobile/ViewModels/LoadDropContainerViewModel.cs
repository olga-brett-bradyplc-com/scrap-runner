using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class LoadDropContainerViewModel : BaseViewModel
    {
        private readonly IContainerService _containerService;
        private readonly IDriverService _driverService;

        public LoadDropContainerViewModel(IContainerService containerService, IDriverService driverService)
        {
            _containerService = containerService;
            _driverService = driverService;
            Title = "Load/Drop Containers";
        }

        public override async void Start()
        {
            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var containers = await _containerService.FindPowerIdContainers(currentDriver.PowerId);

            CurrentContainers = new ObservableCollection<ContainerMasterModel>(containers);
        }

        private string _currentContainersLabel;
        public string CurrentContainersLabel
        {
            get { return _currentContainersLabel; }
            set { SetProperty(ref _currentContainersLabel, value); }
        }

        private string _addContainerLabel;
        public string AddContainerLabel
        {
            get { return _addContainerLabel; }
            set { SetProperty(ref _addContainerLabel, value); }
        }

        private ObservableCollection<ContainerMasterModel> _currentContainer;
        public ObservableCollection<ContainerMasterModel> CurrentContainers
        {
            get { return _currentContainer; }
            set { SetProperty(ref _currentContainer, value); }
        }
    }
}
