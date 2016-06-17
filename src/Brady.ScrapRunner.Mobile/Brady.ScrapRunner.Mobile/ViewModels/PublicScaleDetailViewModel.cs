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
            SubTitle = $"Trip {TripNumber}";
        }
        private string _methodOfEntry;

        public string MethodOfEntry
        {
            get { return _methodOfEntry; }
            set { SetProperty(ref _methodOfEntry, value); }
        }

        public override async void Start()
        {
            var segments = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            var list = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();

            foreach (var tsm in segments)
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
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
            //popup "Finished with scale"
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.FinishedScale);
            if (result)
            {
                var currentUser = await _driverService.GetCurrentDriverStatusAsync();

                var containerProcess = await _tripService.ProcessPublicScaleAsync(new DriverContainerActionProcess
                {
                    EmployeeId = currentUser.EmployeeId,
                    PowerId = currentUser.PowerId,
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

                var doneProcess = await _tripService.ProcessContainerDoneAsync(new DriverSegmentDoneProcess
                {
                    EmployeeId = currentUser.EmployeeId,
                    TripNumber = TripNumber,
                    TripSegNumber = TripSegNumber,
                    ActionDateTime = DateTime.Now,
                    PowerId = currentUser.PowerId,
                    DriverModified = Constants.Yes
                });

                if (!doneProcess.WasSuccessful)
                    await UserDialogs.Instance.AlertAsync(doneProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);

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
                        UserDialogs.Instance.ActionSheetAsync("Select Reason Code", "", "Cancel",
                            ReviewReasonsList.Select(cm => cm.CodeDisp1).ToArray());

                var currentUser = await _driverService.GetCurrentDriverStatusAsync();

                var reasonItem = ReviewReasonsList.FirstOrDefault(cm => cm.CodeDisp1 == alertAsync);

                if (reasonItem != null)
                {
                    var containerProcess = await _tripService.ProcessPublicScaleAsync(new DriverContainerActionProcess
                    {
                        EmployeeId = currentUser.EmployeeId,
                        PowerId = currentUser.PowerId,
                        ActionType = ContainerActionTypeConstants.Exception,
                        MethodOfEntry = MethodOfEntry,
                        ActionDateTime = DateTime.Now,
                        TripNumber = TripNumber,
                        TripSegNumber = TripSegNumber,
                        ContainerNumber = TripSegContainerNumber,
                        ActionDesc = reasonItem.CodeDisp1
                    });

                    if (!containerProcess.WasSuccessful)
                        await UserDialogs.Instance.AlertAsync(containerProcess.Failure.Summary,
                            AppResources.Error, AppResources.OK);

                    var doneProcess = await _tripService.ProcessContainerDoneAsync(new DriverSegmentDoneProcess
                    {
                        EmployeeId = currentUser.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        PowerId = currentUser.PowerId,
                        DriverModified = Constants.Yes
                    });

                    if (!doneProcess.WasSuccessful)
                        await UserDialogs.Instance.AlertAsync(doneProcess.Failure.Summary,
                            AppResources.Error, AppResources.OK);

                    await _tripService.MarkExceptionTripAsync(TripNumber);
                    await _tripService.MarkExceptionTripSegmentAsync(TripNumber, TripSegNumber);
                    await
                        _tripService.MarkExceptionTripSegmentContainerAsync(TripNumber, TripSegNumber,
                            TripSegContainerNumber, reasonItem.CodeDisp1);
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

            if (!tripSegmentContainers.Any())
            {
                await _tripService.CompleteTripAsync(TripNumber);
                await _tripService.CompleteTripSegmentAsync(TripNumber, TripSegNumber);
                Close(this);
                if (await _tripService.IsTripLegTypePublicScale(TripNumber))
                    ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
                else
                    ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else
            {
                Close(this);
                ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
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
