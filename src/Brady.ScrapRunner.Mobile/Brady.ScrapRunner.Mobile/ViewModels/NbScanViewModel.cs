using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class NbScanViewModel : BaseViewModel
    {
        private readonly IContainerService _containerService;
        private readonly ICodeTableService _codeTableService;

        public NbScanViewModel(IContainerService containerService, ICodeTableService codeTableService)
        {
            _containerService = containerService;
            _codeTableService = codeTableService;
            Title = "Add New Container";
        }

        public void Init(string containerNumber, bool loginProcessed)
        {
            ContainerId = containerNumber;
            BarcodeNumber = containerNumber;
            LoginProcessed = loginProcessed;
        }

        public override async void Start()
        {
            var containerTypes = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);
            var containerSizes = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerSize);
            var containerLevels = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerLevel);

            SizeList = new ObservableCollection<CodeTableModel>(containerSizes);
            LevelList = new ObservableCollection<CodeTableModel>(containerLevels);
            TypeList = new ObservableCollection<CodeTableModel>(containerTypes);
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

        private IMvxAsyncCommand _addContainerCommand;
        public IMvxAsyncCommand AddContainerCommand => _addContainerCommand ?? (_addContainerCommand = new MvxAsyncCommand(ExecuteAddContainerCommandAsync));

        protected async Task ExecuteAddContainerCommandAsync()
        {
            await _containerService.UpdateNbContainerAsync(ContainerId);
            ShowViewModel<LoadDropContainerViewModel>(new {loginProcessed = LoginProcessed});
        }
    }
}
