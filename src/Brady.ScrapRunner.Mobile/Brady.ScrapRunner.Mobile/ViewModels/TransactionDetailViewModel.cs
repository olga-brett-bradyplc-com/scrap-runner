using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
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
        private readonly IDriverService _driverService;
        private readonly IPreferenceService _preferenceService;
        private readonly IContainerService _containerService;

        public TransactionDetailViewModel(ITripService tripService, 
            ICodeTableService codeTableService, 
            ICustomerService customerService,
            IPreferenceService preferenceService,
            IContainerService containerService,
            IDriverService driverService)
        {
            _tripService = tripService;
            _codeTableService = codeTableService;
            _customerService = customerService;
            _preferenceService = preferenceService;
            _containerService = containerService;
            _driverService = driverService;
        }

        // Initialize parameters passed from transaction summary screen
        public void Init(string tripNumber, string tripSegmentNumber, short tripSegmentSeqNo)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegmentNumber;
            TripSegContainerSeqNumber = tripSegmentSeqNo;
        }
        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }

        public override async void Start()
        {
            Title = AppResources.TransactionDetail;
            SubTitle = $"{AppResources.Trip} {TripNumber}";
            ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);

            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            Segment = await _tripService.FindTripSegmentInfoAsync(TripNumber, TripSegNumber);
            Container =
                await _tripService.FindTripSegmentContainer(TripNumber, TripSegNumber, TripSegContainerSeqNumber);

            var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == Container.TripSegContainerType?.TrimEnd());
            Container.TripSegContainerTypeDesc = contType != null ? contType.CodeDisp1?.TrimEnd() : Container.TripSegContainerType;

            TripSegContainerNumber = Container?.TripSegContainerNumber ?? "";
            Location = Container?.TripSegContainerLocation ?? "";
            Commodity = Container?.TripSegContainerCommodityDesc ?? "";
            Notes = Container?.TripSegContainerComment ?? "";

            var containerLevels = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerLevel);
            LevelList = new ObservableCollection<CodeTableModel>(containerLevels.OrderBy(l => int.Parse(l.CodeValue)));
            LevelList.Insert(0, new CodeTableModel
            {
                CodeName = null,
                CodeValue = null,
                CodeDisp1 = AppResources.NoLevelSelected
            });

            var customerLocations = await _customerService.FindCustomerLocations(Segment.TripSegDestCustHostCode);
            CustomerLocationList = new ObservableCollection<CustomerLocationModel>(customerLocations);
            CustomerLocationList.Insert(0, new CustomerLocationModel
            {
                CustHostCode = null,
                CustLocation = CustomerLocationList.Count > 0 ? AppResources.NoLocationSelected : AppResources.NoLocationAval
            });

            var customLocation = customerLocations.Any(l => l.CustLocation == Container?.TripSegContainerLocation);

            // Was a custom location entered in dispatch
            if(!customLocation && Container?.TripSegContainerLocation != null )
                CustomerLocationList.Insert(1, new CustomerLocationModel()
                {
                    CustHostCode = Segment.TripSegDestCustHostCode,
                    CustLocation = Container.TripSegContainerLocation
                });

            var customerCommodities = await _customerService.FindCustomerCommodites(Segment.TripSegDestCustHostCode);
            CustomerCommodityList = new ObservableCollection<CustomerCommodityModel>(customerCommodities);
            CustomerCommodityList.Insert(0, new CustomerCommodityModel
            {
                CustHostCode = null,
                CustCommodityCode = null,
                CustCommodityDesc = CustomerCommodityList.Count > 0 ? AppResources.NoCommoditySelected : AppResources.NoCommoditiesAval
            });

            var allowCommoditySelection =
                await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFCommodSelection);

            if ( allowCommoditySelection == Constants.Yes )
                CommoditySelectionEnabled = true;

            if (!string.IsNullOrEmpty(Container?.TripSegContainerCommodityCode))
                SelectedCommodity =
                    CustomerCommodityList.FirstOrDefault(
                        cc => cc.CustCommodityCode == Container.TripSegContainerCommodityCode);

            if (Container?.TripSegContainerLevel != null)
                SelectedLevel = LevelList.FirstOrDefault(l => l.CodeValue == Container.TripSegContainerLevel.ToString());

            if (!string.IsNullOrEmpty(Container?.TripSegContainerLocation))
                SelectedLocation =
                    CustomerLocationList.FirstOrDefault(l => string.Equals(l.CustLocation, Container.TripSegContainerLocation, StringComparison.CurrentCultureIgnoreCase));
            
            base.Start();
        }

        private IMvxAsyncCommand _transactionCompleteCommand;
        // Command bindings
        public IMvxAsyncCommand TransactionCompleteCommand
            => _transactionCompleteCommand ?? (_transactionCompleteCommand = new MvxAsyncCommand(ExecuteTransactionCompleteCommand, CanExecuteTransactionCompleteCommand));

        private async Task ExecuteTransactionCompleteCommand()
        {
            var container = await _containerService.FindContainerAsync(TripSegContainerNumber);

            if (container == null)
            {
                UserDialogs.Instance.Alert(AppResources.ContainerNotFound, AppResources.Error);
                return;
            }

            string sCount = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFContainerValidationCount);

            if (sCount == null) sCount = "1";

            int iCount = int.Parse(sCount);
            if (iCount > 1)
            {
                for (var i = 0; i < iCount; i++)
                {
                    var contPrompt = await UserDialogs.Instance.PromptAsync(AppResources.EnterContainerNumber, "",
                        AppResources.Save, AppResources.Cancel, "", InputType.Default);
                    if (TripSegContainerNumber != contPrompt.Text)
                        i--;
                    else
                        TripSegContainerNumber = contPrompt.Text;
                }
            }

            var useContainerLevel =
                await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFUseContainerLevel);

            if ( useContainerLevel == Constants.Yes && SelectedLevel.CodeValue == null && _tripService.IsTripLegLoaded(Segment))
            {
                UserDialogs.Instance.Alert(AppResources.LevelRequired, AppResources.Error, AppResources.OK);
                return;
            }

            using (var completeTripSegmentContainer = UserDialogs.Instance.Loading(AppResources.Loading,maskType: MaskType.Black))
            {
                if (!string.IsNullOrEmpty(TripSegContainerNumber))
                    Container.TripSegContainerNumber = TripSegContainerNumber;

                Container.TripSegContainerLevel = string.IsNullOrEmpty(SelectedLevel?.CodeValue) ? (short?)null : short.Parse(SelectedLevel?.CodeValue);

                if (SelectedCommodity?.CustHostCode != null)
                {
                    Container.TripSegContainerCommodityCode = SelectedCommodity.CustCommodityCode;
                    Container.TripSegContainerCommodityDesc = SelectedCommodity.CustCommodityDesc;
                }
                
                if( SelectedLocation?.CustHostCode != null )
                    Container.TripSegContainerLocation = SelectedLocation.CustLocation;
                
                Container.MethodOfEntry = TripMethodOfCompletionConstants.Manual;

                // Container.TripSegContainerNotes = not implemented server side

                if (_tripService.IsTripLegLoaded(Segment, true))
                {
                    await _containerService.LoadContainerOnPowerIdAsync(CurrentDriver.PowerId, TripSegContainerNumber, Segment.TripSegDestCustHostCode);
                    container.ContainerContents = ContainerContentsConstants.Loaded;
                    await _containerService.UpdateContainerAsync(container);
                }
                else if (_tripService.IsTripLegLoaded(Segment)) // Pickup Empty
                {
                    await _containerService.LoadContainerOnPowerIdAsync(CurrentDriver.PowerId, TripSegContainerNumber, Segment.TripSegDestCustHostCode);
                    container.ContainerContents = ContainerContentsConstants.Empty;
                    await _containerService.UpdateContainerAsync(container);
                }
                else if (_tripService.IsTripLegDropped(Segment))
                {
                    await _containerService.UnloadContainerFromPowerIdAsync(CurrentDriver.PowerId, TripSegContainerNumber);
                    container.ContainerContents = ContainerContentsConstants.Empty;
                    await _containerService.UpdateContainerAsync(container);
                }

                await _tripService.CompleteTripSegmentContainerAsync(Container);

                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber, methodOfEntry = MethodOfEntry });
             }
        }

        private bool CanExecuteTransactionCompleteCommand()
        {
            return !string.IsNullOrEmpty(TripSegContainerNumber);
        }

        private IMvxAsyncCommand _transactionUnableToProcessCommand;
        public IMvxAsyncCommand TransactionUnableToProcessCommand
            => _transactionUnableToProcessCommand ?? (_transactionUnableToProcessCommand = new MvxAsyncCommand(ExecuteTransactionUnableToProcessCommand));

        private async Task ExecuteTransactionUnableToProcessCommand()
        {
            var exceptions = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ExceptionCodes);
            var exceptionDialogAsync = await UserDialogs.Instance.ActionSheetAsync(AppResources.SelectException, "", AppResources.Cancel, null,
                        exceptions.Select(ct => ct.CodeDisp1).ToArray());

            if (exceptionDialogAsync != AppResources.Cancel && !string.IsNullOrEmpty(exceptionDialogAsync))
            {
                using ( var markTripExceptionContainer = UserDialogs.Instance.Loading(AppResources.Loading, maskType: MaskType.Black))
                {
                    short selectedLevel;
                    var exceptionObj = exceptions.FirstOrDefault(ct => ct.CodeDisp1 == exceptionDialogAsync);
                    if (!string.IsNullOrEmpty(TripSegContainerNumber))
                        Container.TripSegContainerNumber = TripSegContainerNumber;

                    Container.MethodOfEntry = TripMethodOfCompletionConstants.Manual;

                    Container.TripSegContainerLevel = short.TryParse(SelectedLevel.CodeValue, out selectedLevel) ? selectedLevel : (short?)null;

                    await _tripService.UpdateTripSegmentContainerAsync(Container);

                    await _tripService.MarkExceptionTripAsync(TripNumber);
                    await _tripService.MarkExceptionTripSegmentAsync(Segment);
                    await _tripService.MarkExceptionTripSegmentContainerAsync(Container, exceptionObj?.CodeValue);

                    Close(this);
                    ShowViewModel<TransactionSummaryViewModel>(
                        new { tripNumber = TripNumber, methodOfEntry = MethodOfEntry });
                }
            }
        }

        private string _methodOfEntry;
        public string MethodOfEntry
        {
            get { return _methodOfEntry; }
            set { SetProperty(ref _methodOfEntry, value); }
        }

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
            set
            {
                SetProperty(ref _tripSegContainerNumber, value);
                TransactionCompleteCommand.RaiseCanExecuteChanged();
            }
        }

        private string _containerId;
        public string ContainerId
        {
            get { return _containerId; }
            set { SetProperty(ref _containerId, value); }
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

        private bool _commoditySelectionEnabled;
        public bool CommoditySelectionEnabled
        {
            get { return _commoditySelectionEnabled; }
            set { SetProperty(ref _commoditySelectionEnabled, value); }
        }


        private CodeTableModel _selectedLevel;
        public CodeTableModel SelectedLevel
        {
            get { return _selectedLevel; }
            set{ SetProperty(ref _selectedLevel, value); }
        }

        private CustomerLocationModel _selectedLocation;
        public CustomerLocationModel SelectedLocation
        {
            get { return _selectedLocation; }
            set { SetProperty(ref _selectedLocation, value); }
        }

        private CustomerCommodityModel _selectedCommodity;
        public CustomerCommodityModel SelectedCommodity
        {
            get { return _selectedCommodity; }
            set { SetProperty(ref _selectedCommodity, value); }
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

        private TripSegmentContainerModel Container { get; set; }

        private TripSegmentModel Segment { get; set; }

        private DriverStatusModel CurrentDriver { get; set; }

        private async Task ProcessContainer()
        {
            var reviewReason = await
                _codeTableService.FindCodeTableObject(CodeTableNameConstants.ExceptionCodes,
                    Container.TripSegContainerReviewReason);
            var containerAction =
            await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
            {
                EmployeeId = CurrentDriver.EmployeeId,
                PowerId = CurrentDriver.PowerId,
                ActionType = (Container.TripSegContainerReviewFlag == TripSegStatusConstants.Exception) ? ContainerActionTypeConstants.Exception : ContainerActionTypeConstants.Done,
                ActionCode = (Container.TripSegContainerReviewFlag == TripSegStatusConstants.Exception) ? Container.TripSegContainerReviewReason : null,
                ActionDesc = reviewReason?.CodeDisp1,
                ActionDateTime = DateTime.Now,
                MethodOfEntry = TripMethodOfCompletionConstants.Manual,
                TripNumber = TripNumber,
                TripSegNumber = Container.TripSegNumber,
                ContainerNumber = Container.TripSegContainerNumber,
                ContainerLevel = Container.TripSegContainerLevel
            });
        }
    }
}