using System.Collections.ObjectModel;
using System.Linq;
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

        public ScaleDetailViewModel(ITripService tripService, IDriverService driverService, ICodeTableService codeTableService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _codeTableService = codeTableService;

            Title = AppResources.YardScaleDetail;

            ContainerSetDownCommand = new MvxAsyncCommand(ExecuteContainerSetDownCommandAsync);
            ContainerLeftOnTruckCommand = new MvxAsyncCommand(ExecuteContainerLeftOnTruckCommandAsync);
        }

        public void Init(string tripNumber, string tripSegNumber, short tripSegContainerSeqNumber, string tripSegContainerNumber)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegNumber;
            TripSegContainerSeqNumber = tripSegContainerSeqNumber;
            TripSegContainerNumber = tripSegContainerNumber;
            SubTitle = $"{AppResources.Trip} {TripNumber}";
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
            var list = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();

            foreach (var tsm in segments) 
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                list.Add(grouping);
            }

            if (list.Any())
                Containers = list;

            base.Start();
        }

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
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

        private DriverStatusModel CurrentDriver { get; set; }

        // Command bindings
        public IMvxAsyncCommand ContainerSetDownCommand { get; private set; }
        public IMvxAsyncCommand ContainerLeftOnTruckCommand { get; private set; }

        private IMvxCommand _grossWeightSetCommand;
        public IMvxCommand GrossWeightSetCommand
            => _grossWeightSetCommand ?? (_grossWeightSetCommand = new MvxCommand(ExecuteGrossWeightSetCommand));
        
        private void ExecuteGrossWeightSetCommand()
        {
            GrossTime = DateTime.Now;
        }

        private IMvxCommand _tareWeightSetCommand;
        public IMvxCommand TareWeightSetCommand
            => _tareWeightSetCommand ?? (_tareWeightSetCommand = new MvxCommand(ExecuteTareWeightSetCommand));
        
        private void ExecuteTareWeightSetCommand()
        {
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
                (_secondGrossWeightSetCommand = new MvxCommand(ExecuteSecondGrossWeightSetCommand));
        
        private void ExecuteSecondGrossWeightSetCommand()
        {
            SecondGrossTime = DateTime.Now;
        }

        private async Task ExecuteContainerLeftOnTruckCommandAsync()
        {
            await ProcessContainers(false, AppResources.LeftOnTruckContainerMessage);
        }

        private bool IsGrossWeightSet()
        {
            return GrossTime != null;
        }

        private async Task ProcessContainers(bool setDownInYard, string confirmationMessage)
        {
            var reasons = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ReasonCodes);
            var tripSegmentContainers = await _tripService.FindNextTripSegmentContainersAsync(TripNumber, TripSegNumber);

            // Show review exception dialog if gross time isn't set
            var reasonDialogAsync = (!GrossTime.HasValue)
                ? await
                    UserDialogs.Instance.ActionSheetAsync(AppResources.SelectReviewReason, AppResources.Cancel, "", null,
                        reasons.Select(ct => ct.CodeDisp1).ToArray())
                : "";

            if (reasonDialogAsync == AppResources.Cancel) return;

            var reason = reasons.FirstOrDefault(ct => ct.CodeDisp1 == reasonDialogAsync);

            var completeMessage = tripSegmentContainers.TakeWhile(
                                       tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete) && tscm.TripSegContainerNumber != TripSegContainerNumber).Any()
                ? confirmationMessage
                : confirmationMessage + "\n\n" + AppResources.CompleteTrip;

            var result = await UserDialogs.Instance.ConfirmAsync(completeMessage, AppResources.ConfirmLabel);

            // If user confirms action
            if (result)
            {
                using (var completeTripSegment = UserDialogs.Instance.Loading(AppResources.CompletingTripSegment, maskType: MaskType.Black))
                {
                    // Go through each container, updating both the local and remote db
                    foreach (var container in Containers.SelectMany(grouping => grouping))
                    {
                        await _tripService.UpdateTripSegmentContainerWeightTimesAsync(container, GrossTime, SecondGrossTime, TareTime);
                        await _tripService.CompleteTripSegmentContainerAsync(container);
                        // @TODO : Implement once we have our location service working
                        //await _tripService.UpdateTripSegmentContainerLongLatAsync(TripSegmentContainerModel container , Latitude, Longitude);

                        var containerAction =
                            await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                PowerId = CurrentDriver.PowerId,
                                ActionType = (string.IsNullOrEmpty(reason?.CodeDisp1)) ? ContainerActionTypeConstants.Done : ContainerActionTypeConstants.Review,
                                ActionDateTime = DateTime.Now,
                                TripNumber = TripNumber,
                                TripSegNumber = container.TripSegNumber,
                                ContainerNumber = container.TripSegContainerNumber,
                                Gross1ActionDateTime = GrossTime,
                                TareActionDateTime = TareTime,
                                Gross2ActionDateTime = SecondGrossTime,
                                SetInYardFlag = setDownInYard ? Constants.Yes : Constants.No,
                                MethodOfEntry = ContainerMethodOfEntry.Manual,
                                ActionCode = reason?.CodeValue,
                                ActionDesc = reason?.CodeDisp1
                            });

                        if (containerAction.WasSuccessful) continue;

                        UserDialogs.Instance.Alert(containerAction.Failure.Summary, AppResources.Error);
                        return;
                    }

                    await ExecuteNextStage();
                }
            }
        }

        private async Task ExecuteNextStage()
        {
            // Are there any more containers that need to be weighed?
            // Check to see if any containers/segments exists
            // If not, mark the trip as complete and return to route summary
            // Otherwise, we'd go to the next point in the trip
            var tripSegmentContainers = await _tripService.FindNextTripSegmentContainersAsync(TripNumber, TripSegNumber);

            if (!tripSegmentContainers.TakeWhile(tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete)).Any())
            {
                await _driverService.ClearDriverStatus(CurrentDriver, true);
                await _tripService.CompleteTripAsync(TripNumber);

                foreach (var segment in Containers)
                {
                    var tripSegmentProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = segment.Key.TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        PowerId = CurrentDriver.PowerId,
                        ActionType = TripSegStatusConstants.Done
                    });

                    if (tripSegmentProcess.WasSuccessful)
                        await _tripService.CompleteTripSegmentAsync(segment.Key);
                    else
                        UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                }

                Close(this);
                ShowViewModel<RouteSummaryViewModel>();
            }
            else
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }
    }
}
