using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Messages;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionConfirmationViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;
        private readonly ICodeTableService _codeTableService;
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private readonly IMvxMessenger _mvxMessenger;

        public TransactionConfirmationViewModel(ITripService tripService, IDriverService driverService, ICodeTableService codeTableService,
            IMvxMessenger mvxMessenger)
        {
            _tripService = tripService;
            _driverService = driverService;
            _codeTableService = codeTableService;
            Title = AppResources.SignatureReceipt;
            _mvxMessenger = mvxMessenger;
            _mvxSubscriptionToken = mvxMessenger.SubscribeOnMainThread<TripNotificationMessage>(OnTripNotification);
        }

        private void OnTripNotification(TripNotificationMessage msg)
        {
            switch (msg.Context)
            {
                case TripNotificationContext.Canceled:
                case TripNotificationContext.Reassigned:
                case TripNotificationContext.MarkedDone:
                    if (msg.Trip.TripNumber == TripNumber)
                        ShowViewModel<RouteSummaryViewModel>();
                    break;
                case TripNotificationContext.New:
                    break;
                case TripNotificationContext.Modified:
                    LoadData();
                    break;
                case TripNotificationContext.OnHold:
                    break;
                case TripNotificationContext.Future:
                    break;
                case TripNotificationContext.Unassigned:
                    break;
                case TripNotificationContext.Resequenced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = $"{AppResources.Trip} {tripNumber}";
        }
        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }

        private async void LoadData()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
            var list = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();
            ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);

            foreach (var tsm in segments)
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                foreach (var cont in containers)
                {
                    var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == cont.TripSegContainerType?.TrimEnd());
                    cont.TripSegContainerTypeDesc = contType != null ? contType.CodeDisp1?.TrimEnd() : cont.TripSegContainerType;
                }
                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                list.Add(grouping);
            }

            if (list.Any())
                Containers = list;


        }
        public override void Start()
        {
            LoadData();

            base.Start();
        }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _printedName;
        public string PrintedName
        {
            get { return _printedName; }
            set
            {
                SetProperty(ref _printedName, value);
                ConfirmTransactionsCommand.RaiseCanExecuteChanged();
            }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        private IMvxAsyncCommand _confirmTransactionsCommand;
        public IMvxAsyncCommand ConfirmTransactionsCommand => _confirmTransactionsCommand ?? (_confirmTransactionsCommand = new MvxAsyncCommand<byte[]>(ExecuteConfirmTransactionsCommandAsync, CanExecuteConfirmTransactionsCommandAsync));

        // Command Impl
        private async Task ExecuteConfirmTransactionsCommandAsync(byte[] image)
        {
            await FinishTripLeg(image);
        }

        private async Task FinishTripLeg(byte[] image)
        {
            var nextTripSegmentList = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            var nextTripSegment = nextTripSegmentList.Count > 0 ? nextTripSegmentList.FirstOrDefault() : null;
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var lastSegment = Containers.Any(ts => ts.Key.TripSegNumber == tripSegments.Last().TripSegNumber);

            var message = (lastSegment)
                ? AppResources.PerformTripSegmentComplete + "\n\n" + AppResources.CompleteTrip
                : AppResources.PerformTripSegmentComplete;

            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes, AppResources.No);

            if (confirm)
            {
                using ( var completeTripSegment = UserDialogs.Instance.Loading(AppResources.CompletingTripSegment, maskType: MaskType.Black))
                {
                    foreach (var segment in Containers)
                    {
                        foreach (var container in segment)
                        {
                            var containerProcess = await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                PowerId = CurrentDriver.PowerId,
                                ActionType = (container.TripSegContainerReviewFlag == TripSegStatusConstants.Exception) ? ContainerActionTypeConstants.Exception : ContainerActionTypeConstants.Done,
                                ActionCode = (container.TripSegContainerReviewFlag == TripSegStatusConstants.Exception) ? container.TripSegContainerReviewReason : null,
                                ActionDesc = container.TripSegContainerReivewReasonDesc,
                                ActionDateTime = DateTime.Now,
                                MethodOfEntry = container.MethodOfEntry,
                                TripNumber = TripNumber,
                                TripSegNumber = container.TripSegNumber,
                                ContainerNumber = container.TripSegContainerNumber,
                                ContainerLevel = container.TripSegContainerLevel
                            });

                            if (!containerProcess.WasSuccessful)
                                UserDialogs.Instance.Alert(containerProcess.Failure.Summary, AppResources.Error,
                                    AppResources.OK);
                        }

                        var tripSegmentProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                            {
                                EmployeeId = CurrentDriver.EmployeeId,
                                TripNumber = TripNumber,
                                TripSegNumber = segment.Key.TripSegNumber,
                                ActionType = TripSegmentActionTypeConstants.Done,
                                ActionDateTime = DateTime.Now,
                                PowerId = CurrentDriver.PowerId,
                                Latitude = segment.Key.TripSegEndLatitude,
                                Longitude = segment.Key.TripSegEndLongitude
                        });

                        if (!tripSegmentProcess.WasSuccessful)
                        {
                            UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                            return;
                        }

                        // We've marked the segment with an exception somewhere in the process, so don't mark it as complete
                        if (segment.Key.TripSegStatus != TripSegStatusConstants.Exception)
                            await _tripService.CompleteTripSegmentAsync(segment.Key);
                    }

                    // After segment(s) have been completed, upload the signature
                    var firstSegment = Containers.FirstOrDefault().Key.TripSegNumber;
                    var imageProcess = await _tripService.ProcessDriverImageAsync(new DriverImageProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = firstSegment,
                        ActionDateTime = DateTime.Now,
                        PrintedName = PrintedName,
                        ImageType = ImageTypeConstants.Signature,
                        ImageByteArray = image
                    });

                    if (!imageProcess.WasSuccessful)
                        UserDialogs.Instance.Alert(imageProcess.Failure.Summary, AppResources.Error);

                    // Exception processing
                    if (Containers.Any(c => c.Key.TripSegStatus == TripSegStatusConstants.Exception) && _tripService.IsTripLegScale(nextTripSegment))
                    {
                        completeTripSegment.Hide();

                        var endTrip =
                            await UserDialogs.Instance.ConfirmAsync(
                                string.Format(AppResources.ConfirmRTNeeded, "\n\n"),
                                AppResources.ConfirmLabel,
                                AppResources.Yes,
                                AppResources.No);

                        if (!endTrip)
                        {
                            completeTripSegment.Show();

                            foreach (var segment in nextTripSegmentList)
                            {
                                var tripSegmentProcess =
                                    await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                                    {
                                        EmployeeId = CurrentDriver.EmployeeId,
                                        TripNumber = TripNumber,
                                        TripSegNumber = segment.TripSegNumber,
                                        ActionType = TripSegmentActionTypeConstants.Canceled,
                                        ActionDateTime = DateTime.Now,
                                        PowerId = CurrentDriver.PowerId,
                                        Latitude = segment.TripSegEndLatitude,
                                        Longitude = segment.TripSegEndLongitude
                                    });

                                if (tripSegmentProcess.WasSuccessful) continue;

                                UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary);
                                return;
                            }

                            await CompleteTrip();
                            return;
                        }

                        completeTripSegment.Show();
                    }

                    await _tripService.PropagateContainerUpdates(TripNumber, Containers);

                    if (nextTripSegment != null && nextTripSegment.TripSegDestCustHostCode == Containers.FirstOrDefault().Key.TripSegDestCustHostCode && _tripService.IsTripLegScale(nextTripSegment))
                    {
                        var nextContainers = await _tripService.FindAllContainersForTripSegmentAsync(nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);

                        Close(this);
                        if (_tripService.IsTripLegTypePublicScale(nextTripSegment) && nextContainers.Count > 1)
                            ShowViewModel<PublicScaleSummaryViewModel>(new { tripNumber = TripNumber });
                        else if (_tripService.IsTripLegTypePublicScale(nextTripSegment) && nextContainers.Count == 1)
                            ShowViewModel<PublicScaleDetailViewModel>(
                                new
                                {
                                    tripNumber = TripNumber,
                                    tripSegNumber = nextTripSegment.TripSegNumber,
                                    tripSegContainerSeqNumber = nextContainers.SingleOrDefault().TripSegContainerSeqNumber,
                                    tripSegContainerNumber = nextContainers.SingleOrDefault().TripSegContainerNumber,
                                    methodOfEntry = ContainerMethodOfEntry.Manual
                                });
                        else if ( nextContainers.Count > 1 )
                            ShowViewModel<ScaleSummaryViewModel>(new {tripNumber = TripNumber});
                        else
                            ShowViewModel<ScaleDetailViewModel>(
                                new
                                {
                                    tripNumber = TripNumber,
                                    tripSegNumber = nextTripSegment.TripSegNumber,
                                    tripSegContainerSeqNumber = nextContainers.SingleOrDefault().TripSegContainerSeqNumber,
                                    tripSegContainerNumber = nextContainers.SingleOrDefault().TripSegContainerNumber
                                });
                    }
                    else if (nextTripSegmentList.Any())
                    {
                        CurrentDriver.Status = DriverStatusSRConstants.Done;
                        CurrentDriver.TripSegNumber = nextTripSegmentList.FirstOrDefault().TripSegNumber;
                        await _driverService.UpdateDriver(CurrentDriver);

                        Close(this);
                        ShowViewModel<RouteDetailViewModel>(new { tripNumber = TripNumber });
                    }
                    else
                    {
                        await CompleteTrip();
                    }
                }
            }
        }

        private bool CanExecuteConfirmTransactionsCommandAsync(byte[] image)
        {
            if (string.IsNullOrEmpty(PrintedName) || image == null)
            {
                UserDialogs.Instance.Alert(AppResources.SignAndPrint, AppResources.Error);
                return false;
            }

            return true;
        }

        private async Task CompleteTrip()
        {
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
    }

}