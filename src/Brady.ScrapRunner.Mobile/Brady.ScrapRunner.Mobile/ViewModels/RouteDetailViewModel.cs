using System.Collections.Generic;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Helpers;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Acr.UserDialogs;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private string _custHostCode;

        public RouteDetailViewModel(
            IRepository<TripModel> tripRepository,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            DirectionsCommand = new MvxCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new MvxCommand(ExecuteEnRouteCommand);
            ArriveCommand = new MvxCommand(ExecuteArriveCommand);
            NextStageCommand = new MvxCommand(ExecuteNextStageCommand);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }

        public override void Start()
        {
            Task.Run(async () =>
            {
                var trip = await _tripRepository.FindAsync(t => t.TripNumber == TripNumber);

                if (trip != null)
                {
                    _custHostCode = trip.TripCustHostCode;
                    Title = trip.TripTypeDesc;
                    SubTitle = $"Trip {trip.TripNumber}";
                    TripCustName = trip.TripCustName;
                    TripDriverInstructions = trip.TripDriverInstructions;
                    TripCustAddress = trip.TripCustAddress1 + trip.TripCustAddress2;
                    TripCustCityStateZip = $"{trip.TripCustCity}, {trip.TripCustState} {trip.TripCustZip}";
                    TripType = trip.TripType;

                    var containersTrip = await _tripSegmentRepository.AsQueryable()
                        .Where(ts => ts.TripNumber == TripNumber).ToListAsync();
                    var containerSegments = await _tripSegmentContainerRepository.AsQueryable()
                        .Where(tsc => tsc.TripNumber == TripNumber).ToListAsync();
                    var groupedContainers = from details in containerSegments
                                            orderby details.TripSegNumber
                                            group details by new { details.TripNumber, details.TripSegNumber }
                                            into detailsGroup
                                            select new Grouping<TripSegmentModel, TripSegmentContainerModel>(containersTrip.Find(
                                                    tsm => (tsm.TripNumber + tsm.TripSegNumber).Equals(detailsGroup.Key.TripNumber + detailsGroup.Key.TripSegNumber)
                                                ), detailsGroup);
                    if (containersTrip.Any())
                    {
                        Containers = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>(groupedContainers);
                    }
                }
            });

            base.Start();
        }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _tripType;
        public string TripType
        {
            get { return _tripType; }
            set { SetProperty(ref _tripType, value); }
        }

        private string _tripDriverInstructions;
        public string TripDriverInstructions
        {
            get { return _tripDriverInstructions; }
            set { SetProperty(ref _tripDriverInstructions, value); }
        }

        private string _tripCustName;
        public string TripCustName
        {
            get { return _tripCustName; }
            set { SetProperty(ref _tripCustName, value); }
        }

        private string _tripCustAddress;
        public string TripCustAddress
        {
            get { return _tripCustAddress; }
            set { SetProperty(ref _tripCustAddress, value); }
        }

        private string _tripCustCityStateZip;
        public string TripCustCityStateZip
        {
            get { return _tripCustCityStateZip; }
            set { SetProperty(ref _tripCustCityStateZip, value); }
        }

        private string _currentStatus;
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { SetProperty(ref _currentStatus, value); }
        }

        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers; 
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand DirectionsCommand { get; private set; }
        public MvxCommand EnRouteCommand { get; private set; }
        public MvxCommand ArriveCommand { get; private set; }
        public MvxCommand NextStageCommand { get; private set; }

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            if (!string.IsNullOrEmpty(_custHostCode))
                ShowViewModel<RouteDirectionsViewModel>(new {custHostCode = _custHostCode});
        }

        private async void ExecuteEnRouteCommand()
        {
            var message = string.Format(AppResources.ConfirmEnRouteMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmEnrouteTitle);
            if (confirm)
            {
                CurrentStatus = "EN"; //DriverStatusConstants.EnRoute;
            }
        }

        private async void ExecuteArriveCommand()
        {
            var message = string.Format(AppResources.ConfirmArrivalMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmArrivalTitle);
            if (confirm)
            {
                CurrentStatus = "AR"; //DriverStatusConstants.Arrive;
                //@TODO : REMOVE
                ExecuteNextStageCommand();
            }
        }

        private void ExecuteNextStageCommand()
        {
            if (TripType.Equals("SW"))
            {
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (TripType.Equals("RT"))
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }
    }
}