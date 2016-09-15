using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Enums;
using BWF.DataServices.Metadata.Models;
using MvvmCross.Binding.ExtensionMethods;
using Splat;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Helpers;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Brady.ScrapRunner.Mobile.Interfaces;
    using Brady.ScrapRunner.Mobile.Resources;
    using System.Linq.Expressions;

    public class TransactionSummaryViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IPreferenceService _preferenceService;
        private readonly ICustomerService _customerService;
        private readonly IDriverService _driverService;
        private readonly IContainerService _containerService;
        private readonly ICodeTableService _codeTableService;

        public TransactionSummaryViewModel(ITripService tripService, 
            IPreferenceService preferenceService, 
            ICustomerService customerService, 
            IDriverService driverService, 
            ICodeTableService codeTableService,
            IContainerService containerService)
        {
            _tripService = tripService;
            _preferenceService = preferenceService;
            _customerService = customerService;
            _driverService = driverService;
            _codeTableService = codeTableService;
            _containerService = containerService;
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }
        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }

        public override async void Start()
        {
            Title = AppResources.Transactions;
            SubTitle = $"{AppResources.Trip} {TripNumber}";

            FinishLabel = AppResources.FinishLabel;
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
            Containers = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();
            ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);
            /*
                A trip leg can contain, for example, trip segments of DE, PF and SC. We don't want to
                process the SC containers on this screen, but navigate them to the scale screens after
                they're done with the DE/PF containers
            */
            foreach (var tsm in segments.Where(s => !_tripService.IsTripLegScale(s)))
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);

                foreach (var cont in containers)
                {
                    var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == cont.TripSegContainerType?.TrimEnd());
                    cont.TripSegContainerTypeDesc = contType != null ? contType.CodeDisp1?.TrimEnd() : cont.TripSegContainerType;
                }

                // Find first non-completed, non-reviewed container and set it as the current transaction
                if (CurrentTransaction == null && containers.FirstOrDefault(ct => string.IsNullOrEmpty(ct.TripSegContainerComplete) && string.IsNullOrEmpty(ct.TripSegContainerReviewFlag)) != null)
                {
                    var current = containers.FirstOrDefault(ct => string.IsNullOrEmpty(ct.TripSegContainerComplete));
                    current.SelectedTransaction = true;
                    CurrentTransaction = current;
                }

                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                Containers.Add(grouping);
            }

            // Is the user allowed to add a rt/rn segment? RT/RN must not exist as last segment
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var doesRtnSegExistAtEnd = tripSegments.All(sg => sg.TripSegType != BasicTripTypeConstants.ReturnYard && sg.TripSegType != BasicTripTypeConstants.ReturnYardNC);
            // User preference DEFAllowAddRT must be set to 'Y'
            var defAllowAddRt = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFAllowAddRT);

            AllowRtnAdd = doesRtnSegExistAtEnd && Constants.Yes.Equals(defAllowAddRt);

            base.Start();
        }

        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        private TripSegmentContainerModel _currentTransaction;
        public TripSegmentContainerModel CurrentTransaction
        {
            get {  return _currentTransaction; }
            set
            {
                SetProperty(ref _currentTransaction, value);
                ConfirmationSelectedCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _allowRtnAdd;
        public bool? AllowRtnAdd
        {
            get { return _allowRtnAdd; }
            set { SetProperty(ref _allowRtnAdd, value); }
        }

        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _cameraViewLabel;
        public string CameraViewLabel
        {
            get { return _cameraViewLabel; }
            set { SetProperty(ref _cameraViewLabel, value); }
        }

        private string _finishLabel;
        public string FinishLabel
        {
            get { return _finishLabel; }
            set { SetProperty(ref _finishLabel, value); }
        }

        private DriverStatusModel CurrentDriver { get; set; }

        private IMvxCommand _transactionSelectedCommand;
        public IMvxCommand TransactionSelectedCommand => _transactionSelectedCommand ?? (_transactionSelectedCommand = new MvxCommand<TripSegmentContainerModel>(ExecuteTransactionSelectedCommand));

        private IMvxAsyncCommand _transactionScannedCommandAsync;
        public IMvxAsyncCommand TransactionScannedCommandAsync => _transactionScannedCommandAsync ?? (_transactionScannedCommandAsync = new MvxAsyncCommand<string>(ExecuteTransactionScannedCommandAsync));
        //public IMvxCommand SelectNextTransactionCommand { get; private set; }

        private IMvxAsyncCommand _confirmationSelectedCommand;
        public IMvxAsyncCommand ConfirmationSelectedCommand => _confirmationSelectedCommand ?? (_confirmationSelectedCommand = new MvxAsyncCommand(ExecuteConfirmationSelectedCommand, CanExecuteConfirmationSelectedCommand));

        private IMvxCommand _addRtnYardCommand;
        public IMvxCommand AddRtnYardCommand => _addRtnYardCommand ?? (_addRtnYardCommand = new MvxCommand(ExecuteAddRtnYardCommand));

        private void ExecuteAddRtnYardCommand()
        {
            ShowViewModel<ModifyReturnToYardViewModel>(
                new { changeType = TerminalChangeEnum.Add, tripNumber = TripNumber });
        }

        private async Task ExecuteTransactionScannedCommandAsync(string scannedNumber)
        {
            // If all the current containers have been scanned, don't scan any more
            // @TODO : In the future, we may want the ability to add containers on the fly
            if (CanExecuteConfirmationSelectedCommand()) return;

            if (string.IsNullOrEmpty(scannedNumber))
            {
                UserDialogs.Instance.Toast(AppResources.ErrorScanningBarcode);
                return;
            }
            
            foreach (var currentTripSeg in Containers.Select(container2 => container2.FirstOrDefault(tscm => tscm.TripSegContainerNumber == scannedNumber && string.IsNullOrEmpty(tscm.TripSegContainerComplete))))
                CurrentTransaction = currentTripSeg ?? CurrentTransaction;

            var container = await _containerService.FindContainerAsync(scannedNumber);

            if (container == null)
            {
                UserDialogs.Instance.Alert(AppResources.ContainerNotFound, AppResources.Error);
                return;
            }
            var containers = await _tripService.FindAllContainersForTripSegmentAsync(TripNumber, CurrentTransaction.TripSegNumber);

            List<string> containerNumbers = new List<string>();
            foreach (var cont in containers)
                containerNumbers.Add(cont.TripSegContainerNumber);

            if (!containerNumbers.Contains(scannedNumber))
            {
                if (await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFPrevChangContID) ==
                    Constants.No)
                {
                    UserDialogs.Instance.Alert(AppResources.CantChangeContainerNumberError, AppResources.Error,
                        AppResources.OK);
                    return;
                }
                else
                {
                    var confirmChangeContDialog = await UserDialogs.Instance.ConfirmAsync(
                        string.Format(AppResources.NotMatchContainersAlert, scannedNumber,
                        CurrentTransaction.TripSegContainerNumber), AppResources.ConfirmLabel, AppResources.Yes,
                        AppResources.No);

                    if (!confirmChangeContDialog) return;
                }
            }

            CurrentTransaction.TripSegContainerNumber = scannedNumber;

            var levelRequired = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFUseContainerLevel);
            var commodityRequired = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFCommodSelection);

            var currentSegment =
                Containers.FirstOrDefault(ts => ts.Key.TripSegNumber == CurrentTransaction.TripSegNumber).Key;

            // If container level is required, show spinner dialog allowing them to select container level, then continue processing scan
            if (!CurrentTransaction.TripSegContainerLevel.HasValue && levelRequired == Constants.Yes && _tripService.IsTripLegLoaded(currentSegment, true))
            {
                var levels = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerLevel);

                if (levels.Count < 1)
                {
                    UserDialogs.Instance.Alert(AppResources.NoContainerLevels,
                        AppResources.Error);
                    return;
                }

                var levelSelect = await UserDialogs.Instance.ActionSheetAsync(AppResources.SelectLevel, "", "", null, levels.OrderBy(l => int.Parse(l.CodeValue)).Select(l => l.CodeDisp1).ToArray());

                if (string.IsNullOrEmpty(levelSelect)) return;

                var level = levels.First(l => l.CodeDisp1 == levelSelect);

                short levelNum;
                var parsed = short.TryParse(level.CodeValue, out levelNum);

                if (!parsed)
                {
                    UserDialogs.Instance.Alert(AppResources.ErrorProcessingLevels, AppResources.Error);
                    return;
                }

                CurrentTransaction.TripSegContainerLevel = levelNum;
            }

            using ( var completeTripSegment = UserDialogs.Instance.Loading(AppResources.SavingContainer, maskType: MaskType.Black))
            {
                CurrentTransaction.TripSegContainerNumber = scannedNumber;
                CurrentTransaction.TripSegContainerComplete = Constants.Yes;
                CurrentTransaction.TripSegContainerReviewFlag = Constants.No;
                CurrentTransaction.MethodOfEntry = TripMethodOfCompletionConstants.Scanned;

                await _tripService.UpdateTripSegmentContainerAsync(CurrentTransaction);
                await _tripService.CompleteTripSegmentContainerAsync(CurrentTransaction);

                if (_tripService.IsTripLegLoaded(currentSegment, true))
                {
                    container.ContainerContents = ContainerContentsConstants.Loaded;
                    await _containerService.UpdateContainerAsync(container);
                    await _containerService.LoadContainerOnPowerIdAsync(CurrentDriver.PowerId, CurrentTransaction.TripSegContainerNumber, currentSegment.TripSegDestCustHostCode, CurrentTransaction);
                }
                else if (_tripService.IsTripLegLoaded(currentSegment)) // Pickup Empty
                {
                    container.ContainerContents = ContainerContentsConstants.Empty;
                    await _containerService.UpdateContainerAsync(container);
                    await _containerService.LoadContainerOnPowerIdAsync(CurrentDriver.PowerId, CurrentTransaction.TripSegContainerNumber, currentSegment.TripSegDestCustHostCode, CurrentTransaction);
                }
                else if (_tripService.IsTripLegDropped(currentSegment))
                {
                    container.ContainerContents = ContainerContentsConstants.Empty;
                    await _containerService.UpdateContainerAsync(container);
                    await _containerService.UnloadContainerFromPowerIdAsync(CurrentDriver.PowerId, CurrentTransaction.TripSegContainerNumber);
                }

                SelectNextTransactionContainer();
                ConfirmationSelectedCommand.RaiseCanExecuteChanged();
            }
        }

        private void SelectNextTransactionContainer()
        {
            CurrentTransaction.SelectedTransaction = false;

            foreach (var segment in Containers)
            {
                foreach (var container in segment.Where(container => string.IsNullOrEmpty(container.TripSegContainerComplete) && string.IsNullOrEmpty(container.TripSegContainerReviewFlag)))
                {
                    CurrentTransaction = container;
                    CurrentTransaction.SelectedTransaction = true;
                    break;
                }

                // Assume we've found the next transaction
                if (CurrentTransaction.SelectedTransaction)
                    break;
            }
        }

        private void ExecuteTransactionSelectedCommand(TripSegmentContainerModel tripContainer)
        {
            ShowViewModel<TransactionDetailViewModel>(
                new
                {
                    tripNumber = tripContainer.TripNumber,
                    tripSegmentNumber = tripContainer.TripSegNumber,
                    tripSegmentSeqNo = tripContainer.TripSegContainerSeqNumber
                });
        }

        private async Task ExecuteConfirmationSelectedCommand()
        {
            var segment = Containers.FirstOrDefault().Key;
            var customer = await _customerService.FindCustomerMaster(segment.TripSegDestCustHostCode);

            if (customer?.CustSignatureRequired == Constants.Yes)
            {
                Close(this);
                ShowViewModel<TransactionConfirmationViewModel>(new { tripNumber = TripNumber });
                return;
            }

            await FinishTripLeg();
        }

        private bool CanExecuteConfirmationSelectedCommand()
        {
            return Containers.All(segment => !segment.Any(container => string.IsNullOrEmpty(container.TripSegContainerReviewFlag)));
        }

        /*
            The steps for this :
            
                1. Complete any segments the user currently processed on this screen
                2. Propagate any container changes to subsequent segments as necessacary
                3a. Check to see if the next trip segment is a part of this leg and is a scale type
                    If so, navigate them to the appropiate scale view
                3b. Otherwise, if there are no more segments to process, complete the trip and navigate to route summary
                3c. Otherwise, navigate them to the route detail screen to start the next leg
        */
        private async Task FinishTripLeg()
        {
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var lastSegment = Containers.Any(ts => ts.Key.TripSegNumber == tripSegments.Last().TripSegNumber);

            var message = (lastSegment)
                ? AppResources.PerformTripSegmentComplete + "\n\n" + AppResources.CompleteTrip
                : AppResources.PerformTripSegmentComplete;

            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes, AppResources.No);

            if (confirm)
            {
                using ( var completeTripSegment = UserDialogs.Instance.Loading(AppResources.CompletingTripSegment, maskType: MaskType.Black))
                {
                    foreach (var segment in Containers)
                    {
                        foreach (var container in segment)
                        {
                            var containerProcess = await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                PowerId = CurrentDriver.PowerId,
                                ActionType = (container.TripSegContainerReviewFlag == TripSegStatusConstants.Exception) ? ContainerActionTypeConstants.Exception : ContainerActionTypeConstants.Done,
                                ActionCode = (container.TripSegContainerReviewFlag == TripSegStatusConstants.Exception) ? container.TripSegContainerReviewReason : null,
                                ActionDesc = container.TripSegContainerReivewReasonDesc,
                                ActionDateTime = DateTime.Now,
                                MethodOfEntry = container.MethodOfEntry,
                                TripNumber = TripNumber,
                                TripSegNumber = container.TripSegNumber,
                                ContainerNumber = container.TripSegContainerNumber,
                                ContainerLevel = container.TripSegContainerLevel
                            });

                            if (!containerProcess.WasSuccessful)
                                UserDialogs.Instance.Alert(containerProcess.Failure.Summary, AppResources.Error,
                                    AppResources.OK);
                        }

                        var tripSegmentProcess =
                            await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                TripNumber = TripNumber,
                                TripSegNumber = segment.Key.TripSegNumber,
                                ActionType = TripSegmentActionTypeConstants.Done,
                                ActionDateTime = DateTime.Now,
                                PowerId = CurrentDriver.PowerId,
                                Latitude = segment.Key.TripSegEndLatitude,
                                Longitude = segment.Key.TripSegEndLongitude
                            });

                        if (!tripSegmentProcess.WasSuccessful)
                        {
                            UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                            return;
                        }

                        // We've marked the segment with an exception somewhere in the process, so don't mark it as complete
                        if(segment.Key.TripSegStatus != TripSegStatusConstants.Exception )
                            await _tripService.CompleteTripSegmentAsync(segment.Key);
                    }
                    
                    var nextTripSegmentList = await _tripService.FindNextTripSegmentsAsync(TripNumber);
                    var nextTripSegment = nextTripSegmentList.FirstOrDefault();

                    // Exception processing
                    if (Containers.Any(c => c.Key.TripSegStatus == TripSegStatusConstants.Exception) && _tripService.IsTripLegScale(nextTripSegment))
                    {
                        completeTripSegment.Hide();

                        var endTrip =
                            await UserDialogs.Instance.ConfirmAsync(
                                string.Format(AppResources.ConfirmRTNeeded, "\n\n"), 
                                AppResources.ConfirmLabel,
                                AppResources.Yes, 
                                AppResources.No);

                        if (!endTrip)
                        {
                            completeTripSegment.Show();

                            foreach (var segment in nextTripSegmentList)
                            {
                                var tripSegmentProcess =
                                    await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                                    {
                                        EmployeeId = CurrentDriver.EmployeeId,
                                        TripNumber = TripNumber,
                                        TripSegNumber = segment.TripSegNumber,
                                        ActionType = TripSegmentActionTypeConstants.Canceled,
                                        ActionDateTime = DateTime.Now,
                                        PowerId = CurrentDriver.PowerId,
                                        Latitude = segment.TripSegEndLatitude,
                                        Longitude = segment.TripSegEndLongitude
                                    });

                                if (tripSegmentProcess.WasSuccessful) continue;

                                UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary);
                                return;
                            }
                            
                            await CompleteTrip();
                            return;
                        }

                        completeTripSegment.Show();
                    }

                    await _tripService.PropagateContainerUpdates(TripNumber, Containers);

                    if (nextTripSegment != null && nextTripSegment?.TripSegDestCustHostCode == Containers.FirstOrDefault().Key.TripSegDestCustHostCode && _tripService.IsTripLegScale(nextTripSegment))
                    {
                        var nextContainers = await _tripService.FindAllContainersForTripSegmentAsync(nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);

                        Close(this);
                        if (_tripService.IsTripLegTypePublicScale(nextTripSegment) && nextContainers.Count > 1)
                            ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
                        else if (_tripService.IsTripLegTypePublicScale(nextTripSegment) && nextContainers.Count == 1)
                            ShowViewModel<PublicScaleDetailViewModel>(
                                new
                                {
                                    tripNumber = TripNumber,
                                    tripSegNumber = nextTripSegment.TripSegNumber,
                                    tripSegContainerSeqNumber = nextContainers.SingleOrDefault().TripSegContainerSeqNumber,
                                    tripSegContainerNumber = nextContainers.SingleOrDefault().TripSegContainerNumber,
                                    methodOfEntry = ContainerMethodOfEntry.Manual
                                });
                        else if (nextContainers.Count > 1)
                            ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
                        else
                            ShowViewModel<ScaleDetailViewModel>(
                                new
                                {
                                    tripNumber = TripNumber,
                                    tripSegNumber = nextTripSegment.TripSegNumber,
                                    tripSegContainerSeqNumber = nextContainers.SingleOrDefault().TripSegContainerSeqNumber,
                                    tripSegContainerNumber = nextContainers.SingleOrDefault().TripSegContainerNumber
                                });
                    }
                    else if (nextTripSegmentList.Any())
                    {
                        CurrentDriver.Status = DriverStatusSRConstants.Done;
                        CurrentDriver.TripSegNumber = nextTripSegmentList.FirstOrDefault().TripSegNumber;
                        await _driverService.UpdateDriver(CurrentDriver);

                        Close(this);
                        ShowViewModel<RouteDetailViewModel>(new {tripNumber = TripNumber});
                    }
                    else
                    {
                        await CompleteTrip();
                    }
                }
            }
        }

        private async Task CompleteTrip()
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
    }

}