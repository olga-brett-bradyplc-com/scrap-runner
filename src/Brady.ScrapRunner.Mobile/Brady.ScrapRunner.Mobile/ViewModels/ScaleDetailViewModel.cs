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
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

        public ScaleDetailViewModel(
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;

            Title = AppResources.RouteSummary;

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
            var containersTrip = await _tripSegmentRepository.AsQueryable()
                .Where(ts => ts.TripNumber == TripNumber).ToListAsync();
            var containerSegments = await _tripSegmentContainerRepository.AsQueryable()
                .Where(
                    tsc =>
                        tsc.TripNumber == TripNumber && tsc.TripSegNumber == TripSegNumber &&
                        tsc.TripSegContainerNumber == TripSegContainerNumber).ToListAsync();
            var groupedContainers = from details in containerSegments
                orderby details.TripSegNumber
                group details by new {details.TripNumber, details.TripSegNumber}
                into detailsGroup
                select new Grouping<TripSegmentModel, TripSegmentContainerModel>(containersTrip.Find(
                    tsm =>
                        (tsm.TripNumber + tsm.TripSegNumber).Equals(detailsGroup.Key.TripNumber +
                                                                    detailsGroup.Key.TripSegNumber)
                    ), detailsGroup);
            if (containersTrip.Any())
            {
                TransactionList =
                    new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>(
                        groupedContainers);
            }
            
            base.Start();
        }

        // Listview bindings
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> TransactionList { get; private set; }

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
            // @TODO : Determine if this is the last set of containers for trip, then either navigate back to scale summary
            // @TODO : or finish route and navigate to route summary screen
            var result = await UserDialogs.Instance.ConfirmAsync(AppResources.SetDownContainerMessage, AppResources.SetDown);
            if (result)
            {
                Close(this);
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
