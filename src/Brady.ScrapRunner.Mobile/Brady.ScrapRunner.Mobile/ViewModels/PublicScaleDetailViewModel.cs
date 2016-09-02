using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;

/* TODO: Add process of time out for public scale - after certain period of time delay screen pops up*/
namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using Acr.UserDialogs;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class PublicScaleDetailViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;
        private readonly ICodeTableService _codeTableService;

        public PublicScaleDetailViewModel(ITripService tripService, IDriverService driverService,
            ICodeTableService codeTableService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _codeTableService = codeTableService;

            Title = AppResources.PublicScaleDetail;

            CantProcessLabel = AppResources.CantProcess; //init

            GrossWeightSetCommand = new MvxCommand(ExecuteGrossWeightSetCommand);
            SecondGrossWeightSetCommand = new MvxCommand(ExecuteSecondGrossWeightSetCommand, IsGrossWeightSet);
            TareWeightSetCommand = new MvxCommand(ExecuteTareWeightSetCommand, IsGrossWeightSet);

            ContinueCommand = new MvxAsyncCommand(ExecuteContinueCommandAsync);
        }

        private IMvxAsyncCommand _noProcessCommandAsync;

        public IMvxAsyncCommand NoProcessCommandAsync
            => _noProcessCommandAsync ?? (_noProcessCommandAsync = new MvxAsyncCommand(ExecuteNoProcessCommandDialog));

        public void Init(string tripNumber, string tripSegNumber, short tripSegContainerSeqNumber,
            string tripSegContainerNumber, string methodOfEntry)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegNumber;
            TripSegContainerSeqNumber = tripSegContainerSeqNumber;
            TripSegContainerNumber = tripSegContainerNumber;
            MethodOfEntry = methodOfEntry;
            SubTitle = $"{AppResources.Trip} {TripNumber}";
        }

        private string _methodOfEntry;
        public string MethodOfEntry
        {
            get { return _methodOfEntry; }
            set { SetProperty(ref _methodOfEntry, value); }
        }
        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }

        private DriverStatusModel CurrentDriver { get; set; }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
            var list = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();
            ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);

            foreach (var tsm in segments)
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                foreach (var cont in containers)
                {
                    var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == cont.TripSegContainerType?.TrimEnd());
                    cont.TripSegContainerTypeDesc = contType != null ? contType.CodeDisp1?.TrimEnd() : cont.TripSegContainerType ;
                }
                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                list.Add(grouping);
            }

            if (list.Any())
            {
                Containers = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();
                Containers = list;
            }

            base.Start();
        }

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        private string _cantProcessLabel;
        public string CantProcessLabel
        {
            get { return _cantProcessLabel; }
            set
            {
                SetProperty(ref _cantProcessLabel, value);
                NoProcessCommandAsync.RaiseCanExecuteChanged();
            }
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

        private string _selectedReason;
        public string SelectedReason
        {
            get { return _selectedReason; }
            set { SetProperty(ref _selectedReason, value); }
        }

        // Command bindings
        public IMvxAsyncCommand ContinueCommand { get; private set; }
        public MvxCommand GrossWeightSetCommand { get; private set; }
        public MvxCommand TareWeightSetCommand { get; private set; }
        public MvxCommand SecondGrossWeightSetCommand { get; private set; }

        // Command impl
        /*When driver selects the “Continue” button, the message “Finished with scale?” displays.
        When driver selects Yes, segment is complete.*/
        private async Task ExecuteContinueCommandAsync()
        {
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var lastSegment = Containers.Any(ts => ts.Key.TripSegNumber == tripSegments.Last().TripSegNumber);
            
            var message = (lastSegment)
                ? AppResources.PerformTripSegmentComplete + "\n\n" + AppResources.CompleteTrip
                : AppResources.PerformTripSegmentComplete;

            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes, AppResources.No);
            if (confirm)
            {
                using (var completeTripSegment = UserDialogs.Instance.Loading(AppResources.CompletingTripSegment, maskType: MaskType.Black))
                {
                    var containerProcess =
                        await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                        {
                            EmployeeId = CurrentDriver.EmployeeId,
                            PowerId = CurrentDriver.PowerId,
                            ActionType = ContainerActionTypeConstants.Done,
                            ActionDateTime = DateTime.Now,
                            TripNumber = TripNumber,
                            TripSegNumber = TripSegNumber,
                            ContainerNumber = TripSegContainerNumber,
                            ActionDesc = SelectedReason,
                            Gross1ActionDateTime = GrossTime,
                            Gross2ActionDateTime = SecondGrossTime,
                            TareActionDateTime = TareTime
                        });

                    var segment = await _tripService.FindTripSegmentInfoAsync(TripNumber, TripSegNumber);

                    var doneProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        ActionType = TripSegmentActionTypeConstants.Done,
                        PowerId = CurrentDriver.PowerId,
                        DriverModified = Constants.Yes,
                        Latitude = segment.TripSegEndLatitude,
                        Longitude = segment.TripSegEndLongitude

                    });

                    if (!doneProcess.WasSuccessful)
                        await UserDialogs.Instance.AlertAsync(doneProcess.Failure.Summary,
                            AppResources.Error, AppResources.OK);
                }


                await ExecuteNextStage();
            }
        }

        private ObservableCollection<CodeTableModel> _reviewReasonsList;
        public ObservableCollection<CodeTableModel> ReviewReasonsList
        {
            get { return _reviewReasonsList; }
            set { SetProperty(ref _reviewReasonsList, value); }
        }

        /*Selecting “Can’t Process” displays the exception list.
         "Reason for not being able to process container xxxxx” is displayed with a drop-down list of exceptions.
          Driver selects an exception and then clicks OK button.

          Then the Scale screen displays  and the “Can’t Process” button now displays as “Can Process.”
          If “Can Process” is selected, the button changes back to “Can’t Process.”

          If “Can’t Process” is selected, BG marks container as an exception, ReviewFlag is set to E = Exception, ReviewReason is set to exception description (reason desc).
          Segment status is set to E = Exception, Trip status is set to E = Exception*/
        protected async Task ExecuteNoProcessCommandDialog()
        {
            if (CantProcessLabel.Equals(AppResources.CantProcess))
            {
                // Replace this with an actual query of relevant CodeTable objs from SQLite DB 
                var reasons = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ReasonCodes);
                ReviewReasonsList = new ObservableCollection<CodeTableModel>(reasons);

                var alertAsync =
                    await
                        UserDialogs.Instance.ActionSheetAsync(AppResources.SelectReviewReason, AppResources.Cancel, "", null,
                            ReviewReasonsList.Select(cm => cm.CodeDisp1).ToArray());

                var currentUser = await _driverService.GetCurrentDriverStatusAsync();

                var reasonItem = ReviewReasonsList.FirstOrDefault(cm => cm.CodeDisp1 == alertAsync);

                if (reasonItem != null)
                {
                    await _tripService.MarkExceptionTripAsync(TripNumber);

                    foreach (var segment in Containers)
                    {
                        var doneProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                        {
                            EmployeeId = currentUser.EmployeeId,
                            TripNumber = TripNumber,
                            TripSegNumber = TripSegNumber,
                            ActionDateTime = DateTime.Now,
                            PowerId = currentUser.PowerId,
                            DriverModified = Constants.Yes,
                            Latitude = segment.Key.TripSegEndLatitude,
                            Longitude = segment.Key.TripSegEndLongitude
                        });

                        if (!doneProcess.WasSuccessful)
                            await UserDialogs.Instance.AlertAsync(doneProcess.Failure.Summary,
                                AppResources.Error, AppResources.OK);

                        await _tripService.MarkExceptionTripSegmentAsync(segment.Key);

                        foreach (var container in segment)
                        {
                            var containerProcess = await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                            {
                                EmployeeId = currentUser.EmployeeId,
                                PowerId = currentUser.PowerId,
                                ActionType = ContainerActionTypeConstants.Exception,
                                MethodOfEntry = MethodOfEntry,
                                ActionDateTime = DateTime.Now,
                                TripNumber = TripNumber,
                                TripSegNumber = container.TripSegNumber,
                                ContainerNumber = container.TripSegContainerNumber,
                                ActionDesc = reasonItem.CodeDisp1
                            });

                            if (!containerProcess.WasSuccessful)
                                await UserDialogs.Instance.AlertAsync(containerProcess.Failure.Summary, AppResources.Error, AppResources.OK);

                            await _tripService.MarkExceptionTripSegmentContainerAsync(container, reasonItem.CodeDisp1);
                        }
                    }
                }

                CantProcessLabel = AppResources.CanProcess;
            }
            else
            {
                CantProcessLabel = AppResources.CantProcess;
            }
        }

        private async Task ExecuteNextStage()
        {
            // Are there any more containers that need to be weighed?
            // Check to see if any containers/segments exists
            // If not, delete the trip and return to route summary
            // Otherwise, we'd go to the next point in the trip
            var tripSegmentContainers = await _tripService.FindNextTripSegmentContainersAsync(TripNumber, TripSegNumber);

            if (!tripSegmentContainers.TakeWhile(tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete)).Any())
            {
                foreach (var segment in Containers)
                {
                    var tripSegmentProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = segment.Key.TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        PowerId = CurrentDriver.PowerId,
                        ActionType = TripSegStatusConstants.Done,
                        Latitude = segment.Key.TripSegEndLatitude,
                        Longitude = segment.Key.TripSegEndLongitude
                    });

                    if (tripSegmentProcess.WasSuccessful)
                        await _tripService.CompleteTripSegmentAsync(segment.Key);
                    else
                        UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                }

                var nextTripSegment = await _tripService.FindNextTripSegmentsAsync(TripNumber);

                if(nextTripSegment.Any())
                    await _tripService.PropagateContainerUpdates(TripNumber, Containers);

                // We check to see if the next trip segment has the same TripSegDestCustHostCode as the current segment, and if so,
                // send them to the transaction summary screen. Right now we're making the assumption that we would never go from the
                // public scale screen to the yard scale screen. Please fix if that assumption is wrong
                // We also call Close for each scenrio seperately because of the slight delay ClearDriverStatus and CompeteTripAsync cause,
                // which causes weird UI issues to pop up if we close before we call those two methods
                if (Containers.LastOrDefault().Key?.TripSegDestCustHostCode == nextTripSegment?.FirstOrDefault().TripSegDestCustHostCode)
                {
                    Close(this);
                    ShowViewModel<TransactionSummaryViewModel>(new {tripNumber = TripNumber});
                }
                else if (nextTripSegment.Any())
                {
                    CurrentDriver.Status = DriverStatusSRConstants.Done;
                    CurrentDriver.TripSegNumber = nextTripSegment.FirstOrDefault().TripSegNumber;
                    await _driverService.UpdateDriver(CurrentDriver);

                    Close(this);
                    ShowViewModel<RouteDetailViewModel>(new { tripNumber = TripNumber });
                }
                else
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
            else
            {
                Close(this);
                ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber, methodOfEntry = MethodOfEntry });
            }
        }

        private void ExecuteGrossWeightSetCommand()
        {
            GrossTime = DateTime.Now;
            //TODO: enable 2nd gross, tare button (not material grading)
        }

        private void ExecuteSecondGrossWeightSetCommand()
        {
            SecondGrossTime = DateTime.Now;
        }

        private void ExecuteTareWeightSetCommand()
        {
            TareTime = DateTime.Now;
        }

        private bool IsGrossWeightSet()
        {
            return GrossTime != null;
        }

    }
}
