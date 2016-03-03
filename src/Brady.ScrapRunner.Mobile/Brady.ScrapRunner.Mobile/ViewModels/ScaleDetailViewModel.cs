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

            ContainerSetDownCommand = new MvxCommand(ExecuteContainerSetDownCommand);
            ContainerLeftOnTruckCommand = new MvxCommand(ExecuteContainerLeftOnTruckCommand);
        }

        public void Init(string tripNumber, string tripSegNumber, string tripSegContainerNumber)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegNumber;
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

        private string _grossTime;
        public string GrossTime
        {
            get { return _grossTime; }
            set
            {
                SetProperty(ref _grossTime, value);
                SecondGrossWeightSetCommand.RaiseCanExecuteChanged();
                TareWeightSetCommand.RaiseCanExecuteChanged();
            }
        }

        private string _secondGrossTime;
        public string SecondGrossTime
        {
            get { return _secondGrossTime; }
            set { SetProperty(ref _secondGrossTime, value); }
        }

        private string _tareTime;
        public string TareTime
        {
            get { return _tareTime; }
            set { SetProperty(ref _tareTime, value); }
        }

        // Command bindings
        public MvxCommand ContainerSetDownCommand { get; private set; }
        public MvxCommand ContainerLeftOnTruckCommand { get; private set; }
        public MvxCommand GrossWeightSetCommand { get; private set; }
        public MvxCommand TareWeightSetCommand { get; private set; }
        public MvxCommand SecondGrossWeightSetCommand { get; private set; }

        // Command impl
        public async void ExecuteContainerSetDownCommand()
        {
            // @TODO : Determine if this is last leg of trip, and then give warning that this action will complete said trip
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.SetDownContainerMessage, AppResources.SetDown);
            if (result)
            {
                foreach (var grouping in Containers)
                {
                    await _tripService.CompleteTripSegmentAsync(TripNumber, grouping.Key.TripSegNumber);
                }
                // Check to see if any containers/segments exists
                // If not, delete the trip and return to route summary
                // Otherwise, we'd go to the next point in the trip
                var trip = await _tripService.FindNextTripSegmentsAsync(TripNumber);

                // @TODO : Implement logic to determine where to go from each trip
                if (!trip.Any())
                {
                    await _tripService.CompleteTripAsync(TripNumber);
                    Close(this);
                    ShowViewModel<RouteSummaryViewModel>();
                }
            }
        }

        public async void ExecuteContainerLeftOnTruckCommand()
        {
            // @TODO : Determine if this is the last set of containers for trip, then either navigate back to scale summary
            // @TODO : or finish route and navigate to route summary screen
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.LeftOnTruckContainerMessage, AppResources.LeftOnTruck);
            if (result)
            {
                Close(this);
            }
        }

        public void ExecuteGrossWeightSetCommand()
        {
            GrossTime = DateTime.Now.ToString("hh:mm tt");
        }

        public void ExecuteSecondGrossWeightSetCommand()
        {
            SecondGrossTime = DateTime.Now.ToString("hh:mm tt");
        }

        public void ExecuteTareWeightSetCommand()
        {
            TareTime = DateTime.Now.ToString("hh:mm tt");
        }

        public bool IsGrossWeightSet()
        {
            return !string.IsNullOrWhiteSpace(GrossTime);
        }
    }
}
