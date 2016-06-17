using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
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

        public TransactionConfirmationViewModel(ITripService tripService)
        {
            _tripService = tripService;
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
            using (var completeTripSegment = UserDialogs.Instance.Loading("Completing Trip Segment", maskType: MaskType.Clear))
            {
                foreach (var grouping in Containers)
                    await _tripService.CompleteTripSegmentAsync(TripNumber, grouping.Key.TripSegNumber);
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
