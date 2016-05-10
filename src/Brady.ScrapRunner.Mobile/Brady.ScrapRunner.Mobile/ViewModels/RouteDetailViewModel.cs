using System;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Core.ViewModels;
using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;

        private string _custHostCode;

        public RouteDetailViewModel(ITripService tripService, IDriverService driverService)
        {
            _tripService = tripService;
            _driverService = driverService;
            DirectionsCommand = new MvxCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new MvxAsyncCommand(ExecuteEnRouteCommandAsync);
            ArriveCommand = new MvxAsyncCommand(ExecuteArriveCommandAsync);
            NextStageCommand = new MvxAsyncCommand(ExecuteNextStageCommandAsync);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }
 
        public override async void Start()
        {
            var trip = await _tripService.FindTripAsync(TripNumber);

            if (trip != null)
            {
                using (var tripDataLoad = UserDialogs.Instance.Loading(AppResources.LoadingTripData, maskType: MaskType.Clear))
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

                    _custHostCode = trip.TripCustHostCode;
                    Title = trip.TripTypeDesc;
                    SubTitle = AppResources.Trip + $" {trip.TripNumber}";
                    TripType = trip.TripType;
                    TripFor = trip.TripCustName;

                    if (list.Any())
                    {
                        Containers = list;
                        TripCustName = list.First().Key.TripSegDestCustName;
                        TripDriverInstructions = trip.TripDriverInstructions;
                        TripCustAddress = list.First().Key.TripSegDestCustAddress1 +
                                          list.First().Key.TripSegDestCustAddress2;
                        TripCustCityStateZip = $"{list.First().Key.TripSegDestCustCity}, {list.First().Key.TripSegDestCustState} {list.First().Key.TripSegDestCustZip}";
                    }
                }
            }

            MenuFilter = MenuFilterEnum.NotOnTrip; // Reset for when we start a new trip segment
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

        private string _enrouteLabel;
        public string EnrouteLabel
        {
            get { return _enrouteLabel; }
            set { SetProperty(ref _enrouteLabel, value); }
        }

        private string _arriveLabel;
        public string ArriveLabel
        {
            get { return _arriveLabel; }
            set { SetProperty(ref _arriveLabel, value); }
        }
        private string _tripFor;
        public string TripFor
        {
            get { return _tripFor; }
            set { SetProperty(ref _tripFor, value); }
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

        private string _nextActionLabel;
        public string NextActionLabel
        {
            get { return _nextActionLabel; }
            set { SetProperty(ref _nextActionLabel, value);  }
        }

        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers; 
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand DirectionsCommand { get; private set; }
        public IMvxAsyncCommand EnRouteCommand { get; private set; }
        public IMvxAsyncCommand ArriveCommand { get; private set; }
        public IMvxAsyncCommand NextStageCommand { get; private set; }

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            if (!string.IsNullOrEmpty(_custHostCode))
                ShowViewModel<RouteDirectionsViewModel>(new {custHostCode = _custHostCode});
        }

        private async Task ExecuteEnRouteCommandAsync()
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

                var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

                using (var loading = UserDialogs.Instance.Loading("Loading ...", maskType: MaskType.Black))
                {
                    var setDriverEnroute = await _driverService.SetDriverEnrouteRemoteAsync(new DriverEnrouteProcess
                    {
                        EmployeeId = currentDriver.EmployeeId,
                        PowerId = currentDriver.PowerId,
                        TripNumber = TripNumber,
                        TripSegNumber = "01",
                        ActionDateTime = DateTime.Now,
                        Odometer = currentDriver.Odometer ?? default(int),
                    });

                    if (setDriverEnroute.WasSuccessful)
                    {
                        CurrentStatus = DriverStatusConstants.Enroute;
                        MenuFilter = MenuFilterEnum.OnTrip;
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync(setDriverEnroute.Failure.Summary,
                            AppResources.Error, AppResources.OK);
                    }
                }
            }
        }

        private async Task ExecuteArriveCommandAsync()
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
                var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

                using (var loading = UserDialogs.Instance.Loading("Loading ...", maskType: MaskType.Black))
                {
                    var setDriverArrived = await _driverService.SetDriverArrivedRemoteAsync(new DriverArriveProcess
                    {
                        EmployeeId = currentDriver.EmployeeId,
                        PowerId = currentDriver.PowerId,
                        TripNumber = TripNumber,
                        TripSegNumber = "01",
                        ActionDateTime = DateTime.Now,
                        Odometer = currentDriver.Odometer ?? default(int),
                    });

                    if (setDriverArrived.WasSuccessful)
                    {
                        CurrentStatus = DriverStatusConstants.Arrive;
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync(setDriverArrived.Failure.Summary,
                            AppResources.Error, AppResources.OK);
                    }

                    if (await _tripService.IsTripLegTransactionAsync(TripNumber))
                    {
                        NextActionLabel = AppResources.Transactions;
                    }
                    else if (await _tripService.IsTripLegScaleAsync(TripNumber))
                    {
                        NextActionLabel = AppResources.YardScaleLabel;
                    }
                    else if (await _tripService.IsTripLegNoScreenAsync(TripNumber))
                    {
                        NextActionLabel = AppResources.FinishTripLabel;
                    }
                }
            }
        }

        private async Task ExecuteNextStageCommandAsync()
        {
            if (await _tripService.IsTripLegTransactionAsync(TripNumber))
            {
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (await _tripService.IsTripLegScaleAsync(TripNumber))
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (await _tripService.IsTripLegNoScreenAsync(TripNumber))
            {
                Close(this);
                foreach (var groupings in Containers)
                    await _tripService.CompleteTripSegmentAsync(TripNumber, groupings.Key.TripSegNumber);

                await _tripService.CompleteTripAsync(TripNumber);
                ShowViewModel<RouteSummaryViewModel>();
            }
        }
    }
}