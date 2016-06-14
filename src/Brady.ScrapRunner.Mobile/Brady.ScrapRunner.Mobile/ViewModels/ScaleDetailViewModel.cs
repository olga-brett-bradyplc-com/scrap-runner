using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public ScaleDetailViewModel(ITripService tripService)
        {
            _tripService = tripService;
            Title = AppResources.YardScaleDetail;

            GrossWeightSetCommand = new MvxCommand(ExecuteGrossWeightSetCommand);
            SecondGrossWeightSetCommand = new MvxCommand(ExecuteSecondGrossWeightSetCommand, IsGrossWeightSet);
            TareWeightSetCommand = new MvxCommand(ExecuteTareWeightSetCommand, IsGrossWeightSet);

            ContainerSetDownCommand = new MvxAsyncCommand(ExecuteContainerSetDownCommandAsync);
            ContainerLeftOnTruckCommand = new MvxAsyncCommand(ExecuteContainerLeftOnTruckCommandAsync);
        }

        public void Init(string tripNumber, string tripSegNumber, short tripSegContainerSeqNumber, string tripSegContainerNumber)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegNumber;
            TripSegContainerSeqNumber = tripSegContainerSeqNumber;
            TripSegContainerNumber = tripSegContainerNumber;
            SubTitle = $"Trip {TripNumber}";
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

        // Command bindings
        public IMvxAsyncCommand ContainerSetDownCommand { get; private set; }
        public IMvxAsyncCommand ContainerLeftOnTruckCommand { get; private set; }
        public MvxCommand GrossWeightSetCommand { get; private set; }
        public MvxCommand TareWeightSetCommand { get; private set; }
        public MvxCommand SecondGrossWeightSetCommand { get; private set; }

        // Command impl
        private async Task ExecuteContainerSetDownCommandAsync()
        {
            var nextTripSegment = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            var tripSegmentContainers = await _tripService.FindNextTripSegmentContainersAsync(TripNumber, TripSegNumber);

            var completeMessage = (nextTripSegment.TakeWhile( ts => ts.TripSegNumber != TripSegNumber).Any() ||
                                   tripSegmentContainers.TakeWhile(
                                       tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete) && tscm.TripSegContainerNumber != TripSegContainerNumber).Any())
                ? AppResources.SetDownContainerMessage
                : AppResources.SetDownContainerMessage + "\n\n" + AppResources.CompleteTrip;

            // @TODO : Determine if this is last leg of trip, and then give warning that this action will complete said trip
            var result = await UserDialogs.Instance.ConfirmAsync(completeMessage, AppResources.SetDown);
            if (result)
            {
                // @TODO : This won't work with bulk processing
                await
                    _tripService.UpdateTripSegmentContainerWeightTimesAsync(TripNumber, TripSegNumber,
                        TripSegContainerNumber, GrossTime, SecondGrossTime, TareTime);
                // @TODO : Implement once we have our location service working
                //await
                //    _tripService.UpdateTripSegmentContainerLongLatAsync(TripNumber, TripSegNumber,
                //        TripSegContainerNumber, Latitude, Longitude);
                await _tripService.ProcessTripSegmentContainerAsync(TripNumber, TripSegNumber, TripSegContainerSeqNumber, TripSegContainerNumber, true);
                await ExecuteNextStage();
            }
        }

        private async Task ExecuteContainerLeftOnTruckCommandAsync()
        {
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.LeftOnTruckContainerMessage, AppResources.LeftOnTruck);
            if (result)
            {
                Close(this);
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
                await _tripService.CompleteTripAsync(TripNumber);
                await _tripService.CompleteTripSegmentAsync(TripNumber, TripSegNumber);
                Close(this);
                ShowViewModel<RouteSummaryViewModel>();
            }
            else
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }

        private void ExecuteGrossWeightSetCommand()
        {
            GrossTime = DateTime.Now;
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
            return GrossTime == null;
        }
    }
}
