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
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

        public ScaleDetailViewModel(
            IRepository<TripModel> tripRepository,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;

            Title = "Yard/Scale Detail";

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
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == TripNumber);

            if (trip != null)
            {
                using (var tripDataLoad = UserDialogs.Instance.Loading("Loading Trip Data", maskType: MaskType.Clear))
                {
                    var containersForSegment =
                        await ContainerHelper.ContainersForSegment(TripNumber, _tripSegmentRepository,
                            _tripSegmentContainerRepository);
                    if (containersForSegment.Any())
                    {
                        Containers =
                            new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>(
                                containersForSegment);
                    }
                }
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
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.SetDownContainerMessage, AppResources.SetDown);
            if (result)
            {
                foreach (var grouping in Containers)
                {
                    foreach (var tscm in grouping)
                    {
                        await _tripSegmentContainerRepository.DeleteAsync(tscm);
                    }
                    await _tripSegmentRepository.DeleteAsync(grouping.Key);
                }
                // Check to see if any containers/segments exists
                // If not, delete the trip and return to route summary
                // Otherwise, we'd go to the next point in the trip
                var trip = await _tripRepository.FindAsync(t => t.TripNumber == TripNumber);
                var tripContainersExist = await ContainerHelper.ContainersExist(TripNumber, _tripSegmentRepository,
                    _tripSegmentContainerRepository);

                // @TODO : Implement logic to determine where to go from each trip
                if (!tripContainersExist)
                {
                    await _tripRepository.DeleteAsync(trip);
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
