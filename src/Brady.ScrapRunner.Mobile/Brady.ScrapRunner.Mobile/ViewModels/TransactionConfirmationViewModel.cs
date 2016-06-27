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

        public TransactionConfirmationViewModel(ITripService tripService, IDriverService driverService)
        {
            _tripService = tripService;
            _driverService = driverService;
            Title = AppResources.SignatureReceipt;
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = $"{AppResources.Trip} {tripNumber}";
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

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        private IMvxAsyncCommand _confirmTransactionsCommand;
        public IMvxAsyncCommand ConfirmTransactionsCommand => _confirmTransactionsCommand ?? (_confirmTransactionsCommand = new MvxAsyncCommand(ExecuteConfirmTransactionsCommandAsync, CanExecuteConfirmTransactionsCommandAsync));

        // Command Impl
        private async Task ExecuteConfirmTransactionsCommandAsync()
        {
            // Check to see if this is the last leg of the trip, and if so, warn them.
            // We can't use FindNextTripSegment like we normally do because we haven't
            // marked the segment as complete yet.
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var lastSegment = tripSegments.Last();

            if (Containers.Any(ts => ts.Key.TripSegNumber == lastSegment.TripSegNumber))
            {
                var message = string.Format(AppResources.PerformActionLabel, "\n\n");
                var confirm =
                    await
                        UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes,
                            AppResources.No);
                if (confirm)
                    await FinishTripLeg();
            }
            else
            {
                await FinishTripLeg();
            }
        }

        private async Task FinishTripLeg()
        {
            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

            using (var completeTripSegment = UserDialogs.Instance.Loading(AppResources.CompletingTripSegment, maskType: MaskType.Clear))
            {
                foreach (var segment in Containers)
                {
                    var tripSegmentProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                    {
                        EmployeeId = currentDriver.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = segment.Key.TripSegNumber,
                        ActionType = TripSegmentActionTypeConstants.Done,
                        ActionDateTime = DateTime.Now,
                        PowerId = currentDriver.PowerId
                    });

                    if (tripSegmentProcess.WasSuccessful)
                    {
                        await _tripService.CompleteTripSegmentAsync(segment.Key);
                        foreach (var container in segment)
                        {
                            var containerAction =
                                await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                                {
                                    EmployeeId = currentDriver.EmployeeId,
                                    PowerId = currentDriver.PowerId,
                                    ActionType = ContainerActionTypeConstants.Done,
                                    ActionDateTime = DateTime.Now,
                                    MethodOfEntry = TripMethodOfCompletionConstants.Manual,
                                    TripNumber = TripNumber,
                                    TripSegNumber = container.TripSegNumber,
                                    ContainerNumber = container.TripSegContainerNumber,
                                    ContainerLevel = container.TripSegContainerLevel
                                });

                            if (containerAction.WasSuccessful) continue;

                            UserDialogs.Instance.Alert(containerAction.Failure.Summary, AppResources.Error);
                            return;
                        }
                    }
                    else
                    {
                        UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                        return;
                    }
                }
            }

            var nextTripSegment = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            Close(this);

            if (nextTripSegment.Any())
            {
                ShowViewModel<RouteDetailViewModel>(new { tripNumber = TripNumber });
            }
            else
            {
                await _tripService.CompleteTripAsync(TripNumber);
                ShowViewModel<RouteSummaryViewModel>();
            }
        }

        private bool CanExecuteConfirmTransactionsCommandAsync()
        {
            return !string.IsNullOrEmpty(PrintedName);
        }

    }
}
