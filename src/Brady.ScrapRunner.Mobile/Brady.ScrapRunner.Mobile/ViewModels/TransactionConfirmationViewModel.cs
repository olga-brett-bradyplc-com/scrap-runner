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
            ConfirmTransactionsCommand = new MvxAsyncCommand(ExecuteConfirmTransactionsCommandAsync);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = AppResources.Trip + $" {tripNumber}";
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

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public IMvxAsyncCommand ConfirmTransactionsCommand { get; private set; }

        // Command Impl
        private async Task ExecuteConfirmTransactionsCommandAsync()
        {
            using (var completeTripSegment = UserDialogs.Instance.Loading("Completing Trip Segment", maskType: MaskType.Clear))
            {
                foreach (var grouping in Containers)
                {
                    await _tripService.CompleteTripSegmentAsync(TripNumber, grouping.Key.TripSegNumber);
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
                ShowViewModel<RouteSummaryViewModel>();
            }
        }

    }
}
