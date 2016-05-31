using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;

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

        public void Init(bool loginProcessed)
        {
            // Was this screen loaded from login?
            LoginProcessed = loginProcessed;
        }

        public override async void Start()
        {
            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var containers = await _containerService.FindPowerIdContainersAsync(currentDriver.PowerId);

            CurrentContainers = new ObservableCollection<ContainerWrapper>();

            foreach (var container in containers.Where(container => container.ContainerType != "NB#"))
                CurrentContainers.Add(new ContainerWrapper(container, this));
        }

        // Being used to know whether or not this was passed from initial login, or from selecting menu option
        public bool LoginProcessed { get; set; }

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

        private ObservableCollection<ContainerWrapper> _currentContainer;
        public ObservableCollection<ContainerWrapper> CurrentContainers
        {
            get { return _currentContainer; }
            set { SetProperty(ref _currentContainer, value); }
        }

        public async Task ExecuteDropContainer(string powerId, string containerNumber)
        {
            await _containerService.RemoveContainerFromPowerId(powerId, containerNumber);
            CurrentContainers.Remove(CurrentContainers.First(ct => ct.ContainerMasterItem.ContainerNumber == containerNumber));
        }

        private IMvxAsyncCommand _scanContainerCommand;
        public IMvxAsyncCommand ScanContainerCommand => _scanContainerCommand ?? (_scanContainerCommand = new MvxAsyncCommand<string>(ExecuteScanContainerCommand));

        private IMvxCommand _confirmContainersCommand;
        public IMvxCommand ConfirmContainersCommand => _confirmContainersCommand ?? (_confirmContainersCommand = new MvxCommand(ExecuteConfirmContainersCommand));

        public void ExecuteConfirmContainersCommand()
        {
            Close(this);
            if (LoginProcessed)
                ShowViewModel<RouteSummaryViewModel>();
        }

        protected async Task ExecuteScanContainerCommand(string containerNumber)
        {
            // @TODO : Add error checking ( container already loaded, container doesn't exist, etc. )
            var container = await _containerService.FindContainerAsync(containerNumber);

            if (container.ContainerType == "NB#")
            {
                Close(this);
                ShowViewModel<NbScanViewModel>(new { containerNumber = containerNumber, loginProcessed = LoginProcessed});
            }
            else
            {
                CurrentContainers.Add(new ContainerWrapper(container, this));
            }
        }
    }

    // Utility class that allows us to bind click events within a MvxListView
    public class ContainerWrapper
    {
        private readonly LoadDropContainerViewModel _parent;
        private readonly ContainerMasterModel _containerMaster;

        public ContainerWrapper(ContainerMasterModel containerMaster, LoadDropContainerViewModel parent)
        {
            _parent = parent;
            _containerMaster = containerMaster;
        }

        private IMvxAsyncCommand _dropContainerCommand;
        public IMvxAsyncCommand DropContainerCommand => _dropContainerCommand ?? (_dropContainerCommand = new MvxAsyncCommand(ExecuteDropContainerCommand));

        protected async Task ExecuteDropContainerCommand()
        {
            await _parent.ExecuteDropContainer(ContainerMasterItem.ContainerPowerId, ContainerMasterItem.ContainerNumber);
        }

        public ContainerMasterModel ContainerMasterItem => _containerMaster;
    }

}
