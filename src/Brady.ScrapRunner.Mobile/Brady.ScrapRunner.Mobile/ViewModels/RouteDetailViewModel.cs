using System;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        private readonly IPreferenceService _preferenceService;

        private string _custHostCode;

        public RouteDetailViewModel(ITripService tripService, IDriverService driverService, IPreferenceService preferenceService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _preferenceService = preferenceService;

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
                    SubTitle = $"{AppResources.Trip} {trip.TripNumber}";
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

            // Is the user allowed to edit a rty segment?
            // Rtn must already exist as a segment
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var doesRtnSegExist = tripSegments.Any(sg => sg.TripSegType == BasicTripTypeConstants.ReturnYard || sg.TripSegType == BasicTripTypeConstants.ReturnYardNC);
            // User preference DEFAllowAddRT must be set to 'Y'
            var defAllowChangeRt = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFAllowChangeRT);

            AllowRtnEdit = doesRtnSegExist && Constants.Yes.Equals(defAllowChangeRt);

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

        private bool? _allowRtnEdit;
        public bool? AllowRtnEdit
        {
            get { return _allowRtnEdit; }
            set { SetProperty(ref _allowRtnEdit, value); }
        }

        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers; 
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public IMvxCommand DirectionsCommand { get; private set; }
        public IMvxAsyncCommand EnRouteCommand { get; private set; }
        public IMvxAsyncCommand ArriveCommand { get; private set; }
        public IMvxAsyncCommand NextStageCommand { get; private set; }

        private IMvxCommand _addReturnToYardCommand;

        public IMvxCommand AddReturnToYardCommand
            => _addReturnToYardCommand ?? (_addReturnToYardCommand = new MvxCommand(ExecuteAddReturnToYardCommand));

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            if (!string.IsNullOrEmpty(_custHostCode))
                ShowViewModel<RouteDirectionsViewModel>(new {custHostCode = _custHostCode});
        }

        private void ExecuteAddReturnToYardCommand()
        {
            ShowViewModel<ModifyReturnToYardViewModel>(new {changeType = TerminalChangeEnum.Edit, tripNumber = TripNumber});
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
                    foreach (var segment in Containers)
                    {
                        var setDriverEnroute = await _driverService.SetDriverEnrouteRemoteAsync(new DriverEnrouteProcess
                        {
                            EmployeeId = currentDriver.EmployeeId,
                            PowerId = currentDriver.PowerId,
                            TripNumber = TripNumber,
                            TripSegNumber = segment.Key.TripSegNumber,
                            ActionDateTime = DateTime.Now,
                            Odometer = currentDriver.Odometer ?? default(int),
                        });

                        if (!setDriverEnroute.WasSuccessful)
                        {
                            await UserDialogs.Instance.AlertAsync(setDriverEnroute.Failure.Summary,
                                AppResources.Error, AppResources.OK);
                            return;
                        }
                    }

                    CurrentStatus = DriverStatusConstants.Enroute;
                    MenuFilter = MenuFilterEnum.OnTrip;
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
                        TripSegNumber = "01", //TODO: why it's hardcoded? we need to pass segment number into route detail
                        ActionDateTime = DateTime.Now,
                        Odometer = currentDriver.Odometer ?? default(int),
                    });

                    if (setDriverArrived.WasSuccessful)
                    {
                        CurrentStatus = DriverStatusConstants.Arrive;

                        //TODO: GPS Capture dialog appears here if the current system doesn't have lat/lon set for a yard after arrival
                        var tripInfo = await _tripService.FindTripAsync(TripNumber);
                        {
                            //condition to check for lat/lon
                            var yardInfo = await _tripService.FindYardInfo(tripInfo.TripTerminalId);

                            if (yardInfo != null)
                            {
                                if (yardInfo.CustLatitude == "0" || yardInfo.CustLongitude == "0")
                                {
                                    var gpsCaptureDialog = await UserDialogs.Instance.ConfirmAsync(
                                        AppResources.GPSCaptureMessage, AppResources.GPSCapture, AppResources.Yes,
                                        AppResources.No);

                                    if (gpsCaptureDialog)
                                    {
                                        //TODO: add saving routine to capture current log/lat
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync(setDriverArrived.Failure.Summary,
                            AppResources.Error, AppResources.OK);
                    }

                    if (_tripService.IsTripLegTransaction(Containers.First().Key))
                        NextActionLabel = AppResources.Transactions;
                    else if (_tripService.IsTripLegScale(Containers.First().Key))
                        NextActionLabel = AppResources.YardScaleLabel;
                    else if (_tripService.IsTripLegNoScreen(Containers.First().Key))
                        NextActionLabel = AppResources.FinishTripLabel;
                }
            }
        }

        private async Task ExecuteNextStageCommandAsync()
        {
            if (_tripService.IsTripLegTransaction(Containers.First().Key))
            {
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (_tripService.IsTripLegScale(Containers.First().Key))
            {
                Close(this);

                if (_tripService.IsTripLegTypePublicScale(Containers.First().Key))
                    ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
                else
                    ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (_tripService.IsTripLegNoScreen(Containers.First().Key))
            {
                var message = string.Format(AppResources.PerformActionLabel, "\n\n");
                var confirm =
                    await
                        UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes,
                            AppResources.No);

                if (confirm)
                {
                    foreach (var segment in Containers)
                        await _tripService.CompleteTripSegmentAsync(segment.Key);

                    await _tripService.CompleteTripAsync(TripNumber);

                    Close(this);
                    ShowViewModel<RouteSummaryViewModel>();
                }
            }
        }
    }
}