using System;
using System.Collections.Generic;
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
        private readonly ICustomerService _customerService;
        private readonly ITerminalService _terminalService;

        public RouteDetailViewModel(
            ITripService tripService, 
            IDriverService driverService, 
            IPreferenceService preferenceService, 
            ICustomerService customerService,
            ITerminalService terminalService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _preferenceService = preferenceService;
            _customerService = customerService;
            _terminalService = terminalService;
            
            EnRouteCommand = new MvxAsyncCommand(ExecuteEnRouteCommandAsync);
            ArriveCommand = new MvxAsyncCommand(ExecuteArriveCommandAsync);
            NextStageCommand = new MvxAsyncCommand(ExecuteNextStageCommandAsync);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }

        public void Init(string tripNumber, string status)
        {
            TripNumber = tripNumber;
            CurrentStatus = status;
        }
 
        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

            // We grab all the trips to validate driver is on correct trip if DEFEnforceSeqProcess = Y
            var trips = await _tripService.FindTripsAsync();
            var trip = trips.FirstOrDefault(ts => ts.TripNumber == TripNumber);
            
            Title = trip.TripTypeDesc;
            SubTitle = $"{AppResources.Trip} {trip.TripNumber}";
            TripDriverInstructions = trip.TripDriverInstructions;
            TripType = trip.TripType;
            TripFor = trip.TripCustName;

            var fullTripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var templist = new List<TripLegWrapper>();

            var customerDirections =
                await _customerService.FindCustomerDirections(fullTripSegments.FirstOrDefault().TripSegDestCustHostCode);

            if(customerDirections.Count > 0)
                CustomerDirections = new CustomerDirectionsWrapper
                {
                    TripCustName = fullTripSegments.FirstOrDefault().TripSegDestCustName,
                    TripCustAddress = fullTripSegments.FirstOrDefault().TripSegDestCustAddress1,
                    TripCustCityStateZip = fullTripSegments.FirstOrDefault().DestCustCityStateZip,
                    Directions = customerDirections
                };

            foreach (var segment in fullTripSegments.Where(sg => sg.TripSegStatus == TripSegStatusConstants.Pending || sg.TripSegStatus == TripSegStatusConstants.Missed))
            {
                var currentContainers = await _tripService.FindAllContainersForTripSegmentAsync(TripNumber, segment.TripSegNumber);
                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(segment, currentContainers);
                if (templist.LastOrDefault()?.TripCustHostCode == segment.TripSegDestCustHostCode)
                    templist.LastOrDefault().TripSegments.Add(grouping);
                else
                {
                    templist.Add(new TripLegWrapper
                    {
                        TripCustName = segment.TripSegDestCustName,
                        TripCustHostCode = segment.TripSegDestCustHostCode,
                        TripCustAddress = segment.TripSegDestCustAddress1,
                        TripCustCityStateZip = segment.DestCustCityStateZip,
                        Notes = trip.TripDriverInstructions,
                        TripSegments = new List<Grouping<TripSegmentModel, TripSegmentContainerModel>>{ grouping }
                    });
                }
            }

            TripLegs = new List<TripLegWrapper>(templist);

            var firstSegment = TripLegs.FirstOrDefault().TripSegments.FirstOrDefault().Key;
            TripCustName = firstSegment.TripSegDestCustName;
            TripDriverInstructions = trip.TripDriverInstructions;
            TripCustAddress = firstSegment.TripSegDestCustAddress1 +
                              firstSegment.TripSegDestCustAddress2;
            TripCustCityStateZip = $"{firstSegment.TripSegDestCustCity}, {firstSegment.TripSegDestCustState} {firstSegment.TripSegDestCustZip}";


            // Is the user allowed to edit a rty segment?; RT/RN must already exist as a segment
            var doesRtnSegExist = fullTripSegments.Any(sg => sg.TripSegType == BasicTripTypeConstants.ReturnYard || sg.TripSegType == BasicTripTypeConstants.ReturnYardNC);
            // User preference DEFAllowAddRT must be set to 'Y'
            var defAllowChangeRt = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFAllowChangeRT);

            AllowRtnEdit = doesRtnSegExist && Constants.Yes.Equals(defAllowChangeRt);

            // If the user is already on a trip, or DEFEnforceSeqProcess = Y and they're not on their first avaliable trip, mark the trip as read-only
            var seqEnforced = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFEnforceSeqProcess);
            if (!string.IsNullOrEmpty(CurrentDriver.TripNumber) && 
                CurrentDriver.Status != DriverStatusSRConstants.LoggedIn && 
                CurrentDriver.Status != DriverStatusSRConstants.Available && 
                TripNumber != CurrentDriver.TripNumber)
            {
                ReadOnlyTrip = true;
                UserDialogs.Instance.Toast(AppResources.ReadOnlyTrip);
            }
            else if (seqEnforced == Constants.Yes && trips.FirstOrDefault().TripNumber != TripNumber)
            {
                ReadOnlyTrip = true;
                UserDialogs.Instance.Toast(AppResources.ReadOnlyTrip);
            }

            // If CurrentStatus is null but they're enroute/arrived, assume they've navigated back to this screen
            // from another, and reset CurrentStatus as appropiate
            if (CurrentStatus == null &&
                CurrentDriver.TripNumber == TripNumber &&
                (CurrentDriver.Status == DriverStatusSRConstants.Arrive ||
                 CurrentDriver.Status == DriverStatusSRConstants.Enroute))
                CurrentStatus = CurrentDriver.Status;

            // Set the appropiate button text if user is resuming a trip in "arrived" status
            if (CurrentDriver.TripNumber == TripNumber && CurrentStatus == DriverStatusSRConstants.Arrive)
                SetNextStageLabel(fullTripSegments.FirstOrDefault(sg => sg.TripSegStatus == TripSegStatusConstants.Pending || sg.TripSegStatus == TripSegStatusConstants.Missed));

            base.Start();
        }


        #region Field/Command bindings and impls
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

        private bool _readOnlyTrip;
        public bool ReadOnlyTrip
        {
            get { return _readOnlyTrip; }
            set { SetProperty(ref _readOnlyTrip, value); }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        private List<TripLegWrapper> _tripLegs;
        public List<TripLegWrapper> TripLegs
        {
            get { return _tripLegs; }
            set { SetProperty(ref _tripLegs, value); }
        }

        private CustomerDirectionsWrapper _customerDirections;
        public CustomerDirectionsWrapper CustomerDirections
        {
            get { return _customerDirections; }
            set { SetProperty(ref _customerDirections, value); }
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
        private void ExecuteAddReturnToYardCommand()
        {
            ShowViewModel<ModifyReturnToYardViewModel>(new {changeType = TerminalChangeEnum.Edit, tripNumber = TripNumber});
        }

        private async Task ExecuteEnRouteCommandAsync()
        {
            var firstSegment = TripLegs.FirstOrDefault().TripSegments.FirstOrDefault().Key;
            var message = string.Format(AppResources.ConfirmEnRouteMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmEnrouteTitle);
            if (confirm)
            {
                using (var loading = UserDialogs.Instance.Loading("Loading ...", maskType: MaskType.Black))
                {
                    var setDriverEnroute = await _driverService.ProcessDriverEnrouteAsync(new DriverEnrouteProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        PowerId = CurrentDriver.PowerId,
                        TripNumber = TripNumber,
                        TripSegNumber = firstSegment.TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        Odometer = CurrentDriver.Odometer ?? default(int),
                    });

                    if (!setDriverEnroute.WasSuccessful)
                    {
                        await UserDialogs.Instance.AlertAsync(setDriverEnroute.Failure.Summary,
                            AppResources.Error, AppResources.OK);
                        return;
                    }

                    CurrentDriver.TripNumber = TripNumber;
                    CurrentDriver.TripSegNumber = firstSegment.TripSegNumber;
                    CurrentDriver.Status = DriverStatusSRConstants.Enroute;
                    await _driverService.UpdateDriver(CurrentDriver);

                    CurrentStatus = DriverStatusConstants.Enroute;
                }
            }
        }

        private async Task ExecuteArriveCommandAsync()
        {
            var firstSegment = TripLegs.FirstOrDefault().TripSegments.FirstOrDefault().Key;
            var message = string.Format(AppResources.ConfirmArrivalMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmArrivalTitle);
            if (confirm)
            {
                using (var loading = UserDialogs.Instance.Loading(AppResources.Loading, maskType: MaskType.Black))
                {
                    // Used for RouteSummary view to know what "state" it's in
                    // TODO: Replace with messaging similiar to the menu state?
                    CurrentStatus = DriverStatusSRConstants.Arrive;

                    CurrentDriver.Status = CurrentStatus;
                    await _driverService.UpdateDriver(CurrentDriver);

                    //GPS Capture dialog appears here if the current system doesn't have lat/lon set for a yard after arrival
                    //commented out temporarely for the demo by OIB 8/9/16
                    var currentSegment = await _tripService.FindTripSegmentInfoAsync(TripNumber,
                        TripLegs.FirstOrDefault().TripSegments.FirstOrDefault().Key.TripSegNumber);

                    var tripInfo = await _tripService.FindTripAsync(TripNumber);

                    if (currentSegment.TripSegEndLatitude == null || currentSegment.TripSegEndLongitude == null ||
                        currentSegment.TripSegEndLatitude == 0 || currentSegment.TripSegEndLongitude == 0)
                    {
                        //first check Destination Customer record for lat/lon
                        var customer = await _customerService.FindCustomerMaster(currentSegment.TripSegDestCustHostCode);

                        if (customer.CustLatitude != null && customer.CustLongitude != null &&
                            customer.CustLatitude != 0 && customer.CustLongitude != 0)
                        {
                            currentSegment.TripSegEndLatitude = customer.CustLatitude;
                            currentSegment.TripSegEndLongitude = customer.CustLongitude;

                            await _tripService.UpdateGpsCustomerSegments(currentSegment);
                            await SetDriverArrive();
                        }
                        else
                        {
                            if (currentSegment.TripSegType == BasicTripTypeConstants.ReturnYard ||
                                currentSegment.TripSegType == BasicTripTypeConstants.ReturnYardNC ||
                                currentSegment.TripSegType == BasicTripTypeConstants.YardWork)
                            {
                                //condition to check for lat/lon
                                var terminal = await _terminalService.FindTerminalMasterAsync(tripInfo.TripTerminalId);

                                if (terminal.Latitude == null || terminal.Longitude == null || terminal.Latitude == 0 ||
                                    terminal.Longitude == 0)
                                {
                                    var gpsCaptureDialog = await UserDialogs.Instance.ConfirmAsync(
                                        AppResources.GPSCaptureMessage, AppResources.GPSCapture, AppResources.Yes,
                                        AppResources.No);

                                    if (gpsCaptureDialog)
                                    {
                                        //routine to capture current log/lat
                                        string address = terminal.Address1?.TrimEnd();
                                        if (terminal.Address2?.TrimEnd() != "")
                                            address += terminal.Address2?.TrimEnd();
                                        string termInfoText = terminal.TerminalName?.TrimEnd() + "\n" + address + "\n" +
                                                                terminal.City?.TrimEnd() + " " + terminal.State?.TrimEnd() + " " +
                                                                terminal.Zip?.TrimEnd() + " " + terminal.Country?.TrimEnd();

                                        ShowViewModel<GpsCaptureViewModel>(new
                                        {
                                            custHostCode = currentSegment.TripSegDestCustHostCode,
                                            customerInfo = termInfoText,
                                            currentDriver = CurrentDriver
                                        });
                                    }
                                    else
                                        await SetDriverArrive();
                                }
                                else
                                {
                                    currentSegment.TripSegEndLatitude = terminal.Latitude;
                                    currentSegment.TripSegEndLongitude = terminal.Longitude;

                                    await _tripService.UpdateGpsCustomerSegments(currentSegment);
                                    await SetDriverArrive();
                                }
                            }
                            else
                            {
                                var gpsCaptureDialog = await UserDialogs.Instance.ConfirmAsync(
                                    AppResources.GPSCaptureMessage, AppResources.GPSCapture, AppResources.Yes,
                                    AppResources.No);

                                if (gpsCaptureDialog)
                                {
                                    //routine to capture current log/lat
                                    string address = customer.CustAddress1?.TrimEnd();
                                    if (customer.CustAddress2?.TrimEnd() != "")
                                        address += customer.CustAddress2?.TrimEnd();
                                    string termInfoText = customer.CustName?.TrimEnd() + "\n" + address + "\n" +
                                                            customer.CustCity?.TrimEnd() + " " + customer.CustState?.TrimEnd() + " " +
                                                            customer.CustZip?.TrimEnd() + " " + customer.CustCountry?.TrimEnd();

                                    ShowViewModel<GpsCaptureViewModel>(new
                                    {
                                        custHostCode = currentSegment.TripSegDestCustHostCode,
                                        customerInfo = termInfoText,
                                        currentDriver = CurrentDriver
                                    });
                                }
                                else
                                    await SetDriverArrive();
                            }
                        }
                        SetNextStageLabel(firstSegment);
                    }
                    else
                        await SetDriverArrive();
                }
            }
        }

        private async Task SetDriverArrive()
        {
            var currentSegment = TripLegs.FirstOrDefault().TripSegments.FirstOrDefault().Key;
            var setDriverArrived = await _driverService.ProcessDriverArrivedAsync(new DriverArriveProcess
            {
                EmployeeId = CurrentDriver.EmployeeId,
                PowerId = CurrentDriver.PowerId,
                TripNumber = TripNumber,
                TripSegNumber = currentSegment.TripSegNumber,
                ActionDateTime = DateTime.Now,
                Odometer = CurrentDriver.Odometer ?? default(int),
            });

            if(!setDriverArrived.WasSuccessful)
            {
                await UserDialogs.Instance.AlertAsync(setDriverArrived.Failure.Summary,
                    AppResources.Error, AppResources.OK);
            }
        }

        private void SetNextStageLabel(TripSegmentModel tripSegment)
        {
            if (_tripService.IsTripLegTransaction(tripSegment))
                NextActionLabel = AppResources.Transactions;
            else if (_tripService.IsTripLegScale(tripSegment))
                NextActionLabel = AppResources.YardScaleLabel;
            else if (_tripService.IsTripLegNoScreen(tripSegment))
                NextActionLabel = AppResources.FinishTripLabel;
        }

        private async Task ExecuteNextStageCommandAsync()
        {
            var firstSegment = TripLegs.FirstOrDefault().TripSegments.FirstOrDefault().Key;
            if (_tripService.IsTripLegTransaction(firstSegment))
            {
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (_tripService.IsTripLegScale(firstSegment))
            {
                var segmentContainers = TripLegs.FirstOrDefault().TripSegments.FirstOrDefault();
                Close(this);
                if (_tripService.IsTripLegTypePublicScale(firstSegment) && segmentContainers.Count > 1)
                    ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
                else if (_tripService.IsTripLegTypePublicScale(firstSegment) && segmentContainers.Count == 1)
                    ShowViewModel<PublicScaleDetailViewModel>(
                        new
                        {
                            tripNumber = TripNumber,
                            tripSegNumber = segmentContainers.Key.TripSegNumber,
                            tripSegContainerSeqNumber = segmentContainers.SingleOrDefault().TripSegContainerSeqNumber,
                            tripSegContainerNumber = segmentContainers.SingleOrDefault().TripSegContainerNumber,
                            methodOfEntry = ContainerMethodOfEntry.Manual
                        });
                else if (segmentContainers.Count > 1)
                    ShowViewModel<ScaleSummaryViewModel>(new {tripNumber = TripNumber});
                else
                    ShowViewModel<ScaleDetailViewModel>(
                        new
                        {
                            tripNumber = TripNumber,
                            tripSegNumber = segmentContainers.Key.TripSegNumber,
                            tripSegContainerSeqNumber = segmentContainers.SingleOrDefault().TripSegContainerSeqNumber,
                            tripSegContainerNumber = segmentContainers.SingleOrDefault().TripSegContainerNumber
                        });
            }
            else if (_tripService.IsTripLegNoScreen(firstSegment))
            {
                var message = string.Format(AppResources.PerformActionLabel, "\n\n");
                var confirm =
                    await
                        UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes,
                            AppResources.No);

                if (confirm)
                {
                    foreach (var segment in TripLegs.FirstOrDefault().TripSegments)
                    {
                        foreach (var container in segment)
                        {
                            var containerAction =
                                await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                                {
                                    EmployeeId = CurrentDriver.EmployeeId,
                                    PowerId = CurrentDriver.PowerId,
                                    ActionType = ContainerActionTypeConstants.Done,
                                    ActionDateTime = DateTime.Now,
                                    MethodOfEntry = TripMethodOfCompletionConstants.Driver,
                                    TripNumber = TripNumber,
                                    TripSegNumber = container.TripSegNumber,
                                    ContainerNumber = container.TripSegContainerNumber,
                                    ContainerLevel = container.TripSegContainerLevel
                                });

                            if (containerAction.WasSuccessful) continue;

                            UserDialogs.Instance.Alert(containerAction.Failure.Summary, AppResources.Error);
                            return;
                        }

                        var tripSegmentProcess =
                            await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                TripNumber = TripNumber,
                                ActionType = TripSegmentActionTypeConstants.Done,
                                ActionDateTime = DateTime.Now,
                                PowerId = CurrentDriver.PowerId,
                                Latitude = firstSegment.TripSegEndLatitude,
                                Longitude = firstSegment.TripSegEndLongitude
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

                    //Close(this);
                    ShowViewModel<RouteSummaryViewModel>();
                }
            }
        }

        #endregion
    }

    public class TripLegWrapper
    {
        public string TripCustName { get; set; }
        public string TripCustHostCode { get; set; }
        public string TripCustAddress { get; set; }
        public string TripCustCityStateZip { get; set; }
        public string Notes { get; set; }
        public ICollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> TripSegments { get; set; }
    }

    public class CustomerDirectionsWrapper
    {
        public string TripCustName { get; set; }
        public string TripCustAddress { get; set; }
        public string TripCustCityStateZip { get; set; }
        public List<CustomerDirectionsModel> Directions { get; set; }
    }
}