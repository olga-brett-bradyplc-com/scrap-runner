using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class NbScanViewModel : BaseViewModel
    {
        private readonly IContainerService _containerService;
        private readonly ICodeTableService _codeTableService;
        private readonly IDriverService _driverService;

        public NbScanViewModel(IContainerService containerService, ICodeTableService codeTableService, IDriverService driverService)
        {
            _containerService = containerService;
            _codeTableService = codeTableService;
            _driverService = driverService;
            Title = AppResources.AddNewContainer;
        }

        public void Init(string containerNumber, bool loginProcessed, string methodOfEntry)
        {
            ContainerId = containerNumber;
            BarcodeNumber = containerNumber;
            LoginProcessed = loginProcessed;
            MethodOfEntry = methodOfEntry;
        }

        public override async void Start()
        {
            var containerTypes = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);
            var containerSizes = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerSize);
            var containerLevels = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerLevel);

            SizeList = new ObservableCollection<CodeTableModel>(containerSizes);
            LevelList = new ObservableCollection<CodeTableModel>(containerLevels);
            TypeList = new ObservableCollection<CodeTableModel>(containerTypes);

            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
        }

        private string _containerId;
        public string ContainerId
        {
            get { return _containerId; }
            set { SetProperty(ref _containerId, value); }
        }

        private string _barcodeNumber;
        public string BarcodeNumber
        {
            get { return _barcodeNumber; }
            set { SetProperty(ref _barcodeNumber, value); }
        }

        private bool _loginProcessed;
        public bool LoginProcessed
        {
            get { return _loginProcessed; }
            set { SetProperty(ref _loginProcessed, value); }
        }

        private ObservableCollection<CodeTableModel> _typeList;
        public ObservableCollection<CodeTableModel> TypeList
        {
            get { return _typeList; }
            set { SetProperty(ref _typeList, value); }
        }

        private ObservableCollection<CodeTableModel> _sizeList;
        public ObservableCollection<CodeTableModel> SizeList
        {
            get { return _sizeList; }
            set { SetProperty(ref _sizeList, value); }
        }

        private ObservableCollection<CodeTableModel> _levelList;
        public ObservableCollection<CodeTableModel> LevelList
        {
            get { return _levelList; }
            set { SetProperty(ref _levelList, value); }
        }

        private CodeTableModel _selectedType;
        public CodeTableModel SelectedType
        {
            get { return _selectedType; }
            set { SetProperty(ref _selectedType, value); }
        }

        private CodeTableModel _selectedSize;
        public CodeTableModel SelectedSize
        {
            get { return _selectedSize; }
            set { SetProperty(ref _selectedSize, value); }
        }

        private CodeTableModel _selectedLevel;
        public CodeTableModel SelectedLevel
        {
            get { return _selectedLevel; }
            set { SetProperty(ref _selectedLevel, value); }
        }

        private string MethodOfEntry { get; set; }

        private IMvxAsyncCommand _addContainerCommand;
        public IMvxAsyncCommand AddContainerCommand => _addContainerCommand ?? (_addContainerCommand = new MvxAsyncCommand(ExecuteAddContainerCommandAsync));

        protected async Task ExecuteAddContainerCommandAsync()
        {
            var container = await _containerService.FindContainerAsync(ContainerId);

            if (container == null) return;

            using (var loginData = UserDialogs.Instance.Loading(AppResources.AddingContainer, maskType: MaskType.Black)) {

                var containerNewProcess = await _containerService.ProcessNewContainerAsync(new DriverNewContainerProcess
                {
                    EmployeeId = CurrentDriver.EmployeeId,
                    ActionDateTime = DateTime.Now,
                    ContainerNumber = ContainerId,
                    ContainerType = SelectedType.CodeValue,
                    ContainerSize = SelectedSize.CodeValue,
                    ContainerBarcode = BarcodeNumber
                });

                if (!containerNewProcess.WasSuccessful)
                {
                    UserDialogs.Instance.Alert(containerNewProcess.Failure.Summary, AppResources.Error);
                    return;
                }

                loginData.Title = AppResources.LoadingContainer;

                var containerLoadProcess =
                await _containerService.ProcessContainerActionAsync(new DriverContainerActionProcess
                {
                    EmployeeId = CurrentDriver.EmployeeId,
                    PowerId = CurrentDriver.PowerId,
                    ContainerNumber = ContainerId,
                    ActionType = ContainerActionTypeConstants.Load,
                    ActionDateTime = DateTime.Now,
                    MethodOfEntry = MethodOfEntry
                });

                if (!containerLoadProcess.WasSuccessful)
                {
                    UserDialogs.Instance.Alert(containerLoadProcess.Failure.Summary, AppResources.Error);
                    return;
                }

                container.ContainerNumber = ContainerId;
                container.ContainerType = SelectedType.CodeValue;
                container.ContainerSize = SelectedSize.CodeValue;
                container.ContainerBarCodeNo = BarcodeNumber;
                container.ContainerPowerId = CurrentDriver.PowerId;

                await _containerService.UpdateContainerAsync(container);

                Close(this);
                ShowViewModel<LoadDropContainerViewModel>(new {loginProcessed = LoginProcessed});
            }
        }

        private DriverStatusModel CurrentDriver { get; set; }
    }
}
