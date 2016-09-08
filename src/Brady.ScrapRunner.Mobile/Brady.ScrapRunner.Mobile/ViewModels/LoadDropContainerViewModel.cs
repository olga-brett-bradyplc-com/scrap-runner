using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class LoadDropContainerViewModel : BaseViewModel
    {
        private readonly IContainerService _containerService;
        private readonly IDriverService _driverService;

        public LoadDropContainerViewModel(IContainerService containerService, IDriverService driverService)
        {
            _containerService = containerService;
            _driverService = driverService;
            Title = AppResources.LoadDropContainer;
        }

        public void Init(bool loginProcessed)
        {
            LoginProcessed = loginProcessed;
        }

        // Coming from exception processing
        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var containers = await _containerService.FindPowerIdContainersAsync(CurrentDriver.PowerId);

            CurrentContainers = new ObservableCollection<ContainerWrapper>();

            foreach (var container in containers)
                CurrentContainers.Add(new ContainerWrapper(container, this));
        }

        // Used to know whether or not this was passed from initial login, or from selecting menu option
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

        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private DriverStatusModel CurrentDriver { get; set; }

        private IMvxAsyncCommand _addContainerCommand;
        public IMvxAsyncCommand AddContainerCommand => _addContainerCommand ?? ( _addContainerCommand = new MvxAsyncCommand(ExecuteAddContainerCommand));

        private async Task ExecuteAddContainerCommand()
        {
            var containerNumberPrompt =
                await
                    UserDialogs.Instance.PromptAsync(AppResources.EnterContainerNumber, AppResources.AddContainer, AppResources.OK,
                        AppResources.Cancel, AppResources.ContainerNumber);

            if (!string.IsNullOrEmpty(containerNumberPrompt.Text) && containerNumberPrompt.Text != AppResources.Cancel)
                await ProcessContainerLoad(containerNumberPrompt.Text, ContainerMethodOfEntry.Manual);
        }

        private IMvxAsyncCommand _scanContainerCommand;
        public IMvxAsyncCommand ScanContainerCommand => _scanContainerCommand ?? (_scanContainerCommand = new MvxAsyncCommand<string>(ExecuteScanContainerCommand));

        protected async Task ExecuteScanContainerCommand(string containerNumber)
        {
            await ProcessContainerLoad(containerNumber, ContainerMethodOfEntry.Scanned);
        }

        private async Task ProcessContainerLoad(string containerNumber, string methodOfEntry)
        {
            var container = await _containerService.FindContainerAsync(containerNumber);

            // Container doesn't exist in container master
            if (container == null)
            {
                UserDialogs.Instance.Alert(string.Format(AppResources.ContainerNotFound, containerNumber), AppResources.Error);
                return;
            }

            // Container has already been loaded
            if (CurrentContainers.Any(ct => ct.ContainerMasterItem.ContainerNumber == containerNumber))
            {
                UserDialogs.Instance.Alert(string.Format(AppResources.ContainerLoaded, containerNumber), AppResources.Error);
                return;
            }

            // If new bin, redirect to NbScan view
            if (container.ContainerType == MobileConstants.NewContainerKey)
            {
                Close(this);
                ShowViewModel<NbScanViewModel>(new { containerNumber = containerNumber, loginProcessed = LoginProcessed, methodOfEntry = methodOfEntry });
            }
            // Load the container
            else
            {
                using (var loginData = UserDialogs.Instance.Loading(AppResources.LoadingContainer, maskType: MaskType.Black))
                {
                    var containerProcess =
                        await _containerService.ProcessContainerActionAsync(new DriverContainerActionProcess
                        {
                            EmployeeId = CurrentDriver.EmployeeId,
                            PowerId = CurrentDriver.PowerId,
                            ContainerNumber = container.ContainerNumber,
                            ActionType = ContainerActionTypeConstants.Load,
                            ActionDateTime = DateTime.Now,
                            MethodOfEntry = methodOfEntry
                        });

                    if (containerProcess.WasSuccessful)
                    {
                        await _containerService.LoadContainerOnPowerIdAsync(CurrentDriver.PowerId, containerNumber);
                        CurrentContainers.Add(new ContainerWrapper(container, this));
                        ConfirmContainersCommand.RaiseCanExecuteChanged();
                    }
                    else
                        UserDialogs.Instance.Alert(containerProcess.Failure.Summary, AppResources.Error);
                }
            }
        }

        private bool CanExecuteContinue()
        {
            return CurrentContainers.Count > 0 || TripNumber == null;
        }

        private IMvxCommand _confirmContainersCommand;
        public IMvxCommand ConfirmContainersCommand => _confirmContainersCommand ?? (_confirmContainersCommand = new MvxCommand(ExecuteConfirmContainersCommand, CanExecuteContinue));

        public void ExecuteConfirmContainersCommand()
        {   
            Close(this);
            if (LoginProcessed)
                ShowViewModel<RouteSummaryViewModel>();
        }

        // Helper method used with ContainerWrapper
        protected async Task ExecuteDropContainer(string powerId, string containerNumber)
        {
            using (var loginData = UserDialogs.Instance.Loading(AppResources.DroppingContainer, maskType: MaskType.Black))
            {
                var containerProcess =
                await _containerService.ProcessContainerActionAsync(new DriverContainerActionProcess
                {
                    EmployeeId = CurrentDriver.EmployeeId,
                    PowerId = CurrentDriver.PowerId,
                    ContainerNumber = containerNumber,
                    ActionType = ContainerActionTypeConstants.Dropped,
                    ActionDateTime = DateTime.Now,
                    MethodOfEntry = ContainerMethodOfEntry.Manual
                });

                if (containerProcess.WasSuccessful)
                {
                    CurrentContainers.Remove(CurrentContainers.First(ct => ct.ContainerMasterItem.ContainerNumber == containerNumber));
                    ConfirmContainersCommand.RaiseCanExecuteChanged();

                    var containerMaster = await _containerService.FindContainerAsync(containerNumber);
                    containerMaster.ContainerPowerId = null;
                    await _containerService.UpdateContainerAsync(containerMaster);
                }
                else
                {
                    UserDialogs.Instance.Alert(containerProcess.Failure.Summary, AppResources.Error);
                }
            }
        }

        // Utility class that allows us to bind multiple click events within an mvxlistview item
        public class ContainerWrapper
        {
            private readonly LoadDropContainerViewModel _parent;

            public ContainerWrapper(ContainerMasterModel containerMaster, LoadDropContainerViewModel parent)
            {
                _parent = parent;
                ContainerMasterItem = containerMaster;
            }

            private IMvxAsyncCommand _dropContainerCommand;
            public IMvxAsyncCommand DropContainerCommand => _dropContainerCommand ?? (_dropContainerCommand = new MvxAsyncCommand(ExecuteDropContainerCommand));

            protected async Task ExecuteDropContainerCommand()
            {
                await _parent.ExecuteDropContainer(ContainerMasterItem.ContainerPowerId, ContainerMasterItem.ContainerNumber);
            }

            public ContainerMasterModel ContainerMasterItem { get; }
        }


    }

}
