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
        private readonly IPreferenceService _preferenceService;

        private static readonly int tarePressed = 2;
        private static readonly int secondGrossPressed = 1;
        private static readonly int grossPressed = 0;

        public ScaleDetailViewModel(ITripService tripService, IDriverService driverService, ICodeTableService codeTableService,
            IPreferenceService preferenceService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _codeTableService = codeTableService;
            _preferenceService = preferenceService;

            ContainerSetDownCommand = new MvxAsyncCommand(ExecuteContainerSetDownCommandAsync);
            ContainerLeftOnTruckCommand = new MvxAsyncCommand(ExecuteContainerLeftOnTruckCommandAsync);
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
        
        private async void ExecuteGrossWeightSetCommand()
        {
            await CheckWeights(grossPressed);
            if (GrossWeight <= 0)
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
            await CheckWeights(tarePressed);
            if (TareWeight <= 0)
                return;

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
            await CheckWeights(secondGrossPressed);
            if (SecondGrossWeight <= 0)
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

        private async Task CheckWeights(int weightSelected)
        {
            var reqDrvrEnterWeights = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFDriverWeights);

            if (reqDrvrEnterWeights != Constants.Yes)
                return;

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
                    return;
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
                            break;
                        case 1:
                            SecondGrossWeight = int.Parse(weightPrompt.Text);
                            break;
                        case 2:
                            TareWeight = int.Parse(weightPrompt.Text);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private async Task ProcessContainers(bool setDownInYard, string confirmationMessage)
        {
            var reasons = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ReasonCodes);
            var tripSegmentContainers = await _tripService.FindNextTripSegmentContainersAsync(TripNumber, TripSegNumber);

            // Show review exception dialog if gross time isn't set.
            var reasonDialogAsync = (!GrossTime.HasValue || !TareTime.HasValue)
                ? await
                    UserDialogs.Instance.ActionSheetAsync(AppResources.SelectReviewReason, AppResources.Cancel, "", null,
                        reasons.Select(ct => ct.CodeDisp1).ToArray())
                : "NOREASONCODE";

            if (reasonDialogAsync == AppResources.Cancel || string.IsNullOrEmpty(reasonDialogAsync)) return;

            var reason = reasons.FirstOrDefault(ct => ct.CodeDisp1 == reasonDialogAsync);

            var completeMessage = tripSegmentContainers.TakeWhile(
                                       tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete) && tscm.TripSegContainerNumber != TripSegContainerNumber).Any()
                ? confirmationMessage
                : confirmationMessage + "\n\n" + AppResources.CompleteTrip;

            var result = await UserDialogs.Instance.ConfirmAsync(completeMessage, AppResources.ConfirmLabel);
            
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
                                ActionDesc = reason?.CodeDisp1,
                                Gross1Weight = GrossWeight,
                                Gross2Weight = SecondGrossWeight,
                                TareWeight = TareWeight
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
            else
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }
    }
}
