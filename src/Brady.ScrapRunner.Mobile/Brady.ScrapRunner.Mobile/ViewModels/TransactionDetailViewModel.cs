﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionDetailViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly ICodeTableService _codeTableService;
        private readonly ICustomerService _customerService;

        public TransactionDetailViewModel(ITripService tripService, ICodeTableService codeTableService, ICustomerService customerService)
        {
            _tripService = tripService;
            _codeTableService = codeTableService;
            _customerService = customerService;

            Title = AppResources.TransactionDetail;
        }

        // Initialize parameters passed from transaction summary screen
        public void Init(string tripNumber, string tripSegmentNumber, short tripSegmentSeqNo)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegmentNumber;
            TripSegContainerSeqNumber = tripSegmentSeqNo;
            SubTitle = AppResources.Trip + $" {TripNumber}";
        }

        public override async void Start()
        {
            var container =
                await _tripService.FindTripSegmentContainer(TripNumber, TripSegNumber, TripSegContainerSeqNumber);

            ContainerId = container?.TripSegContainerNumber ?? "";
            Location = container?.TripSegContainerLocation ?? "";
            Commodity = container?.TripSegContainerCommodityDesc ?? "";
            Notes = container?.TripSegContainerComment ?? "";

            var containerLevels = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerLevel);
            LevelList = new ObservableCollection<CodeTableModel>(containerLevels);

            var customerLocations = await _customerService.FindCustomerLocations();
            CustomerLocationList = new ObservableCollection<CustomerLocationModel>(customerLocations);

            var customerCommodities = await _customerService.FindCustomerCommodites();
            CustomerCommodityList = new ObservableCollection<CustomerCommodityModel>(customerCommodities);

            MenuFilter = MenuFilterEnum.OnTransaction;

            base.Start();
        }

        private IMvxAsyncCommand _transactionCompleteCommand;
        // Command bindings
        public IMvxAsyncCommand TransactionCompleteCommand
            => _transactionCompleteCommand ?? (_transactionCompleteCommand = new MvxAsyncCommand(ExecuteTransactionCompleteCommand, CanExecuteTransactionCompleteCommand));

        private async Task ExecuteTransactionCompleteCommand()
        {
            var confirm = await UserDialogs.Instance.ConfirmAsync("Mark this container as complete?", "Segment Complete");
            if (confirm)
            {
                await
                    _tripService.ProcessTripSegmentContainerAsync(TripNumber, TripSegNumber, TripSegContainerSeqNumber,
                        TripSegContainerNumber, true);
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }

        private bool CanExecuteTransactionCompleteCommand()
        {
            return !string.IsNullOrEmpty(ContainerId);
        }

        private IMvxAsyncCommand _transactionUnableToProcessCommand;
        public IMvxAsyncCommand TransactionUnableToProcessCommand
            => _transactionUnableToProcessCommand ?? (_transactionUnableToProcessCommand = new MvxAsyncCommand(ExecuteTransactionUnableToProcessCommand));

        private async Task ExecuteTransactionUnableToProcessCommand()
        {
            var exceptions = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ExceptionCodes);
            var exceptionDialogAsync = await UserDialogs.Instance.ActionSheetAsync("Select Exception Code", "", "Cancel",
                        exceptions.Select(ct => ct.CodeDisp1).ToArray());
            if (exceptionDialogAsync != "Cancel")
            {
                var exceptionObj = exceptions.FirstOrDefault(ct => ct.CodeDisp1 == exceptionDialogAsync);
                await
                    _tripService.ProcessTripSegmentContainerAsync(TripNumber, TripSegNumber, TripSegContainerSeqNumber,
                        TripSegContainerNumber, false);
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _tripSegNumber;
        public string TripSegNumber
        {
            get { return _tripSegNumber; }
            set { SetProperty(ref _tripSegNumber, value); }
        }

        private short _tripSegContainerSeqNumber;
        public short TripSegContainerSeqNumber
        {
            get { return _tripSegContainerSeqNumber; }
            set { SetProperty(ref _tripSegContainerSeqNumber, value); }
        }

        private string _tripSegContainerNumber;
        public string TripSegContainerNumber
        {
            get { return _tripSegContainerNumber; }
            set { SetProperty(ref _tripSegContainerNumber, value); }
        }

        private string _containerId;
        public string ContainerId
        {
            get { return _containerId; }
            set
            {
                SetProperty(ref _containerId, value);
                TransactionCompleteCommand.RaiseCanExecuteChanged();
            }
        }

        private string _location;
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }

        private string _commodity;
        public string Commodity
        {
            get { return _commodity; }
            set { SetProperty(ref _commodity, value); }
        }

        private int? _level;
        public int? Level
        {
            get { return _level; }
            set{ SetProperty(ref _level, value); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value); }
        }

        private string _referenceNumber;
        public string ReferenceNumber
        {
            get { return _referenceNumber; }
            set { SetProperty(ref _referenceNumber, value); }
        }

        private ObservableCollection<CodeTableModel> _levelList;
        public ObservableCollection<CodeTableModel> LevelList
        {
            get { return _levelList; }
            set { SetProperty(ref _levelList, value); }
        }

        private ObservableCollection<CustomerLocationModel> _customerLocationList;
        public ObservableCollection<CustomerLocationModel> CustomerLocationList
        {
            get { return _customerLocationList; }
            set { SetProperty(ref _customerLocationList, value); }
        }

        private ObservableCollection<CustomerCommodityModel> _customerCommodityList;
        public ObservableCollection<CustomerCommodityModel> CustomerCommodityList
        {
            get { return _customerCommodityList; }
            set { SetProperty(ref _customerCommodityList, value); }
        }

    }
}