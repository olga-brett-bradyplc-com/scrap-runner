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

        public RouteDetailViewModel(
            ITripService tripService, 
            IDriverService driverService, 
            IPreferenceService preferenceService, 
            ICustomerService customerService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _preferenceService = preferenceService;
            _customerService = customerService;
            
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

            MenuFilter = MenuFilterEnum.NotOnTrip; // Reset for when we start a new trip segment

            // If the user is already on a trip, or DEFEnforceSeqProcess = Y and they're not on their first avaliable trip, mark the trip as read-only
            var seqEnforced = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFEnforceSeqProcess);
            if (!string.IsNullOrEmpty(CurrentDriver.TripNumber) && 
                CurrentDriver.Status != DriverStatusSRConstants.LoggedIn && 
                CurrentDriver.Status != DriverStatusSRConstants.Available && 
                TripNumber != CurrentDriver.TripNumber)
            {
                ReadOnlyTrip = true;
                UserDialogs.Instance.WarnToast(AppResources.ReadOnlyTrip, AppResources.ReadOnlyTripGenMessage, 10000);
            }
            else if (seqEnforced == Constants.Yes && trips.FirstOrDefault().TripNumber != TripNumber)
            {
                ReadOnlyTrip = true;
                UserDialogs.Instance.WarnToast(AppResources.ReadOnlyTrip, AppResources.ReadOnlyTripSeqMessage, 10000);
            }

            // If CurrentStatus isn't null, that means we're restoring a previous user session on login
            // Otherwise, check driver status to see if they're already in the midst of a trip, and set the status appropiately if so
            if (CurrentDriver.TripNumber == TripNumber && 
                CurrentStatus == null && 
                (CurrentDriver.Status != DriverStatusSRConstants.Available || CurrentDriver.Status != DriverStatusSRConstants.LoggedIn))
            {
                CurrentStatus = CurrentDriver.Status;
                if( CurrentStatus == DriverStatusSRConstants.Arrive )
                    SetNextStageLabel(fullTripSegments.FirstOrDefault(sg => sg.TripSegStatus == TripSegStatusConstants.Pending || sg.TripSegStatus == TripSegStatusConstants.Missed));
            }

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
                    MenuFilter = MenuFilterEnum.OnTrip;
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
                    var setDriverArrived = await _driverService.ProcessDriverArrivedAsync(new DriverArriveProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        PowerId = CurrentDriver.PowerId,
                        TripNumber = TripNumber,
                        TripSegNumber = firstSegment.TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        Odometer = CurrentDriver.Odometer ?? default(int),
                    });

                    if (setDriverArrived.WasSuccessful)
                    {
                        CurrentStatus = DriverStatusConstants.Arrive;

                        CurrentDriver.Status = DriverStatusSRConstants.Arrive;
                        await _driverService.UpdateDriver(CurrentDriver);

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

                    SetNextStageLabel(firstSegment);
                }
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
                Close(this);

                if (_tripService.IsTripLegTypePublicScale(firstSegment))
                    ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
                else
                    ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
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
                                PowerId = CurrentDriver.PowerId
                            });

                        if (tripSegmentProcess.WasSuccessful)
                            await _tripService.CompleteTripSegmentAsync(segment.Key);
                        else
                            UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                    }

                    await _driverService.ClearDriverStatus(CurrentDriver, true);
                    await _tripService.CompleteTripAsync(TripNumber);

                    Close(this);
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