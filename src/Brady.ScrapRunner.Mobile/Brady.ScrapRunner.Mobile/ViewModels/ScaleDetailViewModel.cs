using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using Acr.UserDialogs;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class ScaleDetailViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;
        private readonly ICodeTableService _codeTableService;
        private readonly IPreferenceService _preferenceService;
        private readonly IContainerService _containerService;
        private readonly ICustomerService _customerService;

        private static readonly string NoReasonCodeSelected = "NOREASONCODE";

        private static readonly int tarePressed = 2;
        private static readonly int secondGrossPressed = 1;
        private static readonly int grossPressed = 0;

        public ScaleDetailViewModel(
            ITripService tripService, 
            IDriverService driverService, 
            ICodeTableService codeTableService,
            IPreferenceService preferenceService,
            IContainerService containerService,
            ICustomerService customerService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _codeTableService = codeTableService;
            _preferenceService = preferenceService;
            _containerService = containerService;
            _customerService = customerService;

            ContainerSetDownCommand = new MvxAsyncCommand(ExecuteContainerSetDownCommandAsync);
            ContainerLeftOnTruckCommand = new MvxAsyncCommand(ExecuteContainerLeftOnTruckCommandAsync);
        }

        public void Init(string tripNumber, string containerNumber)
        {
            TripNumber = tripNumber;
            ContainerNumber = containerNumber;
        }

        public void Init(string tripNumber, string tripSegNumber, short tripSegContainerSeqNumber, string tripSegContainerNumber)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegNumber;
            TripSegContainerSeqNumber = tripSegContainerSeqNumber;
            TripSegContainerNumber = tripSegContainerNumber;
        }
 
        public override async void Start()
        {
            Title = AppResources.YardScaleDetail;
            SubTitle = $"{AppResources.Trip} {TripNumber}";

            var currentTripSegment = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            TripSegNumber = currentTripSegment.FirstOrDefault().TripSegNumber;

            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
            ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);
            var container = await _containerService.FindContainerAsync(ContainerNumber);

            var tripContainers = await _tripService.FindAllContainersForTripSegmentAsync(container.ContainerCurrentTripNumber, container.ContainerCurrentTripSegNumber);

            var list = new ObservableCollection<Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>>();
            var tempCustomerName = await _customerService.FindCustomerMaster(container.ContainerCustHostCode);
            
            var tempContainerKey = new ContainerGroupKey
            {
                TripNumber = container.ContainerCurrentTripNumber,
                CustHostCode = container.ContainerCustHostCode,
                Name = tempCustomerName?.CustName ?? "Unused"
            };

            var cont = ContTypesList.FirstOrDefault(ct => ct.CodeDisp1 == container.ContainerType);
            container.ContainerTypeDesc = cont != null ? cont.CodeDisp1 : container.ContainerType;

            var tempContainerMasterTripSeg = new List<ContainerMasterWithTripContainer>
            {
                new ContainerMasterWithTripContainer
                {
                    ContainerMaster = container,
                    TripSegmentContainer = tripContainers.Count > 0 ? tripContainers.FirstOrDefault(ts => ts.TripSegNumber == container.ContainerCurrentTripSegNumber) : null
                }
            };

            list.Add(new Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>(tempContainerKey, tempContainerMasterTripSeg));

            Containers = list;

            base.Start();
        }

        // Listview bindings
        private ObservableCollection<Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>> _containers;
        public ObservableCollection<Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        private string _containerNumber;
        public string ContainerNumber
        {
            get { return _containerNumber; }
            set { SetProperty(ref _containerNumber, value); }
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

        private string _tripSegContainerNumber;
        public string TripSegContainerNumber
        {
            get { return _tripSegContainerNumber; }
            set { SetProperty(ref _tripSegContainerNumber, value); }
        }

        private short _tripSegContainerSeqNumber;
        public short TripSegContainerSeqNumber
        {
            get { return _tripSegContainerSeqNumber; }
            set { SetProperty(ref _tripSegContainerSeqNumber, value); }
        }

        private DateTime? _grossTime;
        public DateTime? GrossTime
        {
            get { return _grossTime; }
            set
            {
                SetProperty(ref _grossTime, value);
                SecondGrossWeightSetCommand.RaiseCanExecuteChanged();
                TareWeightSetCommand.RaiseCanExecuteChanged();
            }
        }

        private DateTime? _secondGrossTime;
        public DateTime? SecondGrossTime
        {
            get { return _secondGrossTime; }
            set { SetProperty(ref _secondGrossTime, value); }
        }

        private DateTime? _tareTime;
        public DateTime? TareTime
        {
            get { return _tareTime; }
            set { SetProperty(ref _tareTime, value); }
        }

        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }

        private DriverStatusModel CurrentDriver { get; set; }

        // Command bindings
        public IMvxAsyncCommand ContainerSetDownCommand { get; private set; }
        public IMvxAsyncCommand ContainerLeftOnTruckCommand { get; private set; }

        private IMvxCommand _grossWeightSetCommand;
        public IMvxCommand GrossWeightSetCommand
            => _grossWeightSetCommand ?? (_grossWeightSetCommand = new MvxCommand(ExecuteGrossWeightSetCommand));
        
        private async void ExecuteGrossWeightSetCommand()
        {
            if (!await CheckWeights(grossPressed))
                return;
            GrossTime = DateTime.Now;
            TareWeightSetCommand.RaiseCanExecuteChanged();
            SecondGrossWeightSetCommand.RaiseCanExecuteChanged();
        }

        private IMvxCommand _tareWeightSetCommand;
        public IMvxCommand TareWeightSetCommand
            => _tareWeightSetCommand ?? (_tareWeightSetCommand = new MvxCommand(ExecuteTareWeightSetCommand, IsGrossWeightSet));
        
        private async void ExecuteTareWeightSetCommand()
        {
            if (!await CheckWeights(tarePressed))
                return;

            if (TareWeight > GrossWeight || TareWeight > SecondGrossWeight)
            {
                await
                    UserDialogs.Instance.AlertAsync(AppResources.TareWeightError, AppResources.Error,
                        AppResources.OK);
                return;
            }

            TareTime = DateTime.Now;
        }
        
        private async Task ExecuteContainerSetDownCommandAsync()
        {
            await ProcessContainers(true, AppResources.SetDownContainerMessage);
        }

        private IMvxCommand _secondGrossWeightSetCommand;
        public IMvxCommand SecondGrossWeightSetCommand
            =>
                _secondGrossWeightSetCommand ??
                (_secondGrossWeightSetCommand = new MvxCommand(ExecuteSecondGrossWeightSetCommand, IsGrossWeightSet));
        
        private async void ExecuteSecondGrossWeightSetCommand()
        {
            if (!await CheckWeights(secondGrossPressed))
                return;

            SecondGrossTime = DateTime.Now;
        }

        private async Task ExecuteContainerLeftOnTruckCommandAsync()
        {
            await ProcessContainers(false, AppResources.LeftOnTruckContainerMessage);
        }

        private bool IsGrossWeightSet()
        {
            return GrossTime.HasValue;
        }

        private int _grossWeight;
        public int GrossWeight
        {
            get { return _grossWeight;  }
            set { SetProperty(ref _grossWeight, value); }
        }

        private int _secondGrossWeight;
        public int SecondGrossWeight
        {
            get { return _secondGrossWeight; }
            set { SetProperty(ref _secondGrossWeight, value); }
        }

        private int _tareWeight;
        public int TareWeight
        {
            get { return _tareWeight; }
            set { SetProperty(ref _tareWeight, value); }
        }

        private async Task<bool> CheckWeights(int weightSelected)
        {
            var reqDrvrEnterWeights = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFDriverWeights);

            if (reqDrvrEnterWeights != Constants.Yes)
                return true;

            string title;

            switch (weightSelected)
            {
                case 0:
                title = AppResources.GrossWeight;
                    break;
                case 1:
                title = AppResources.SecondGrossWeight;
                    break;
                case 2:
                title = AppResources.TareWeight;
                    break;
                default:
                    return false;
            }

            var weightPrompt = await UserDialogs.Instance.PromptAsync(title, AppResources.WeightHint,
                AppResources.Save, AppResources.Cancel, "", InputType.Number);

            if (!string.IsNullOrEmpty(weightPrompt.Text))
            {
                using (var loginData = UserDialogs.Instance.Loading(AppResources.Loading, maskType: MaskType.Black))
                {
                    switch (weightSelected)
                    {
                        case 0:
                            GrossWeight = int.Parse(weightPrompt.Text);
                            return GrossWeight > 0;
                        case 1:
                            SecondGrossWeight = int.Parse(weightPrompt.Text);
                            return SecondGrossWeight > 0;
                        case 2:
                            TareWeight = int.Parse(weightPrompt.Text);
                            return TareWeight > 0;
                        default:
                            return false;
                    }
                }
            }

            return false;
        }

        private async Task ProcessContainers(bool setDownInYard, string confirmationMessage)
        {
            var powerContainersOnId = await _containerService.FindPowerIdContainersAsync(CurrentDriver.PowerId);
            var nextSegments = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            // Show review exception dialog if gross time isn't set.
            var reasons = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ReasonCodes);
            var reasonDialogAsync = (!GrossTime.HasValue || !TareTime.HasValue)
                ? await
                    UserDialogs.Instance.ActionSheetAsync(AppResources.SelectReviewReason, AppResources.Cancel, "", null,
                        reasons.Select(ct => ct.CodeDisp1).ToArray())
                : NoReasonCodeSelected;

            if (reasonDialogAsync == AppResources.Cancel || string.IsNullOrEmpty(reasonDialogAsync)) return;

            var reason = reasons.FirstOrDefault(ct => ct.CodeDisp1 == reasonDialogAsync);

            var anyContainersLeft = powerContainersOnId.Any(c => c.ContainerComplete != Constants.Yes && c.ContainerNumber != ContainerNumber);
            var anySegmentsLeft = nextSegments.Any(ts => ts.TripSegNumber != TripSegNumber);

            var completeMessage = (anyContainersLeft || anySegmentsLeft)
                ? confirmationMessage
                : confirmationMessage + "\n\n" + AppResources.CompleteTrip;

            var result = await UserDialogs.Instance.ConfirmAsync(completeMessage, AppResources.ConfirmLabel);

            if (result)
            {
                using (var completeTripSegment = UserDialogs.Instance.Loading(AppResources.SavingContainer, maskType: MaskType.Black))
                {
                    // Go through each container, updating both the local and remote db
                    foreach (var container in Containers.SelectMany(grouping => grouping))
                    {
                        // Common properties regardless if the container belongs to the trip or not
                        if (GrossTime.HasValue && TareTime.HasValue)
                            container.ContainerMaster.ContainerContents = ContainerContentsConstants.Empty;
                        else
                            container.ContainerMaster.ContainerContents = ContainerContentsConstants.Loaded;

                        if (container.TripSegmentContainer == null)
                        {
                            container.ContainerMaster.ContainerReviewFlag = ContainerActionTypeConstants.Done;
                            container.ContainerMaster.ContainerComplete = Constants.Yes;
                            container.ContainerMaster.ContainerToBeUnloaded = (setDownInYard) ? Constants.Yes : Constants.No;
                            container.ContainerMaster.ContainerContents = (setDownInYard)
                                ? ContainerContentsConstants.Empty
                                : ContainerContentsConstants.Loaded;

                            await _containerService.UpdateContainerAsync(container.ContainerMaster);

                            var unusedContainerAction =
                                await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                                {
                                    EmployeeId = CurrentDriver.EmployeeId,
                                    PowerId = CurrentDriver.PowerId,
                                    ActionType = ContainerActionTypeConstants.Dropped,
                                    ActionDateTime = DateTime.Now,
                                    ContainerNumber = container.ContainerMaster.ContainerNumber
                                });

                            if (unusedContainerAction.WasSuccessful)
                            {
                                if (setDownInYard)
                                {
                                    await _containerService.ResetContainer(container.ContainerMaster, true);
                                }
                                else
                                {
                                    await _containerService.ResetContainer(container.ContainerMaster);

                                    // Make sure this container doesn't show up if they have to go back to scale summary screen
                                    container.ContainerMaster.ContainerComplete = Constants.Yes;
                                    await _containerService.UpdateContainerAsync(container.ContainerMaster);
                                }

                                continue;
                            }

                            UserDialogs.Instance.Alert(unusedContainerAction.Failure.Summary, AppResources.Error);
                            return;
                        }
                        else
                        {
                            var driverProcessObj = new DriverContainerActionProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                PowerId = CurrentDriver.PowerId,
                                ActionType =
                                    (string.IsNullOrEmpty(reason?.CodeDisp1))
                                        ? ContainerActionTypeConstants.Done
                                        : ContainerActionTypeConstants.Review,
                                ActionDateTime = DateTime.Now,
                                TripNumber = TripNumber,
                                TripSegNumber = container.TripSegmentContainer.TripSegNumber,
                                ContainerNumber = container.TripSegmentContainer.TripSegContainerNumber,
                                Gross1ActionDateTime = GrossTime,
                                TareActionDateTime = TareTime,
                                Gross2ActionDateTime = SecondGrossTime,
                                SetInYardFlag = (setDownInYard) ? Constants.Yes : Constants.No,
                                ContainerContents =
                                    (GrossTime.HasValue && TareTime.HasValue)
                                        ? ContainerContentsConstants.Empty
                                        : ContainerContentsConstants.Loaded,
                                MethodOfEntry = ContainerMethodOfEntry.Manual,
                                ActionCode = reason?.CodeValue,
                                ActionDesc = reason?.CodeDisp1,
                                Gross1Weight = GrossWeight,
                                Gross2Weight = SecondGrossWeight,
                                TareWeight = TareWeight
                            };

                            var containerAction = await _tripService.ProcessContainerActionAsync(driverProcessObj);

                            if (containerAction.WasSuccessful)
                            {
                                await _tripService.UpdateTripSegmentContainerWeightTimesAsync(container.TripSegmentContainer, GrossTime, SecondGrossTime, TareTime);
                            
                                container.TripSegmentContainer.TripSegContainerReviewReason = reason?.CodeValue;
                                container.TripSegmentContainer.TripSegContainerReivewReasonDesc = reason?.CodeDisp1;
                                container.TripSegmentContainer.TripSegContainerComplete = Constants.Yes;

                                await _tripService.UpdateTripSegmentContainerAsync(container.TripSegmentContainer);

                                // @TODO : Implement once we have our location service working
                                //await _tripService.UpdateTripSegmentContainerLongLatAsync(TripSegmentContainerModel container , Latitude, Longitude);

                                if (setDownInYard)
                                {
                                    await _containerService.ResetContainer(container.ContainerMaster, true);
                                }
                                else
                                {
                                    await _containerService.ResetContainer(container.ContainerMaster);

                                    // Make sure this container doesn't show up if they have to go back to scale summary screen
                                    container.ContainerMaster.ContainerComplete = Constants.Yes;
                                    await _containerService.UpdateContainerAsync(container.ContainerMaster);
                                }

                                continue;
                            }

                            UserDialogs.Instance.Alert(containerAction.Failure.Summary, AppResources.Error);
                            return;
                        }
                    }

                    await ExecuteNextStage();
                }
            }
        }

        private async Task ExecuteNextStage()
        {
            var powerContainersOnId = await _containerService.FindPowerIdContainersAsync(CurrentDriver.PowerId);
            var finishedContainers = powerContainersOnId.Any(c => c.ContainerComplete != Constants.Yes);

            if (finishedContainers)
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new {tripNumber = TripNumber});
            }
            else
            {
                var tripSegment = await _tripService.FindTripSegmentInfoAsync(TripNumber, TripSegNumber);
                var tripSegmentProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                {
                    EmployeeId = CurrentDriver.EmployeeId,
                    TripNumber = TripNumber,
                    TripSegNumber = TripSegNumber,
                    ActionDateTime = DateTime.Now,
                    PowerId = CurrentDriver.PowerId,
                    ActionType = TripSegStatusConstants.Done,
                    Latitude = tripSegment.TripSegEndLatitude,
                    Longitude = tripSegment.TripSegEndLongitude
                });

                if (tripSegmentProcess.WasSuccessful)
                    await _tripService.CompleteTripSegmentAsync(tripSegment);
                else
                {
                    UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                    return;
                }

                var nextTripSegment = await _tripService.FindNextTripSegmentsAsync(TripNumber);
                var firstTripSegment = nextTripSegment.FirstOrDefault();

                if (nextTripSegment.Count == 0)
                {
                    await _tripService.CompleteTripAsync(TripNumber);

                    var nextTrip = await _tripService.FindNextTripAsync();
                    var seg = await _tripService.FindNextTripSegmentsAsync(nextTrip?.TripNumber);

                    CurrentDriver.Status = nextTrip == null ? DriverStatusSRConstants.NoWork : DriverStatusSRConstants.Available;
                    CurrentDriver.TripNumber = nextTrip == null ? "" : nextTrip.TripNumber;
                    CurrentDriver.TripSegNumber = seg.Count < 1 ? "" : seg.FirstOrDefault().TripSegNumber;

                    await _driverService.UpdateDriver(CurrentDriver);

                    Close(this);
                    ShowViewModel<RouteSummaryViewModel>();
                }
                else if (nextTripSegment.FirstOrDefault().TripSegDestCustHostCode != tripSegment.TripSegDestCustHostCode)
                {
                    CurrentDriver.TripSegNumber = nextTripSegment.FirstOrDefault().TripSegNumber;
                    await _driverService.UpdateDriver(CurrentDriver);

                    Close(this);
                    ShowViewModel<RouteDetailViewModel>(new {tripNumber = TripNumber});
                }
                else if (_tripService.IsTripLegTransaction(firstTripSegment))
                {
                    Close(this);
                    ShowViewModel<TransactionSummaryViewModel>(new {tripNumber = TripNumber});
                }
                else if (_tripService.IsTripLegTypePublicScale(firstTripSegment))
                {
                    Close(this);
                    ShowViewModel<PublicScaleSummaryViewModel>(new {tripNumber = TripNumber});
                }
            }
        }
    }
}
