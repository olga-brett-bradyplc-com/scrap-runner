using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionConfirmationViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;
        private readonly ICodeTableService _codeTableService;

        public TransactionConfirmationViewModel(ITripService tripService, IDriverService driverService, ICodeTableService codeTableService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _codeTableService = codeTableService;
            Title = AppResources.SignatureReceipt;
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = $"{AppResources.Trip} {tripNumber}";
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
            var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
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

                        if (tripSegmentProcess.WasSuccessful)
                            await _tripService.CompleteTripSegmentAsync(segment.Key);
                        else
                            UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
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

    }
}
