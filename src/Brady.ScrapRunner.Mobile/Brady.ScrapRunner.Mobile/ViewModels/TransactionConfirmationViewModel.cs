using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionConfirmationViewModel : BaseViewModel
    {
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

        public TransactionConfirmationViewModel(
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            Title = AppResources.SignatureReceipt;
            ConfirmTransactionsCommand = new MvxCommand(ExecuteConfirmTransactionsCommand);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = AppResources.Trip + $" {TripNumber}";
        }

        public override async void Start()
        {
            var containersTrip = await _tripSegmentRepository.AsQueryable()
                .Where(ts => ts.TripNumber == TripNumber).ToListAsync();
            var containerSegments = await _tripSegmentContainerRepository.AsQueryable()
                .Where(tsc => tsc.TripNumber == TripNumber).ToListAsync();
            var groupedContainers = from details in containerSegments
                orderby details.TripSegNumber
                group details by new {details.TripNumber, details.TripSegNumber}
                into detailsGroup
                select new Grouping<TripSegmentModel, TripSegmentContainerModel>(containersTrip.Find(
                    tsm =>
                        (tsm.TripNumber + tsm.TripSegNumber).Equals(detailsGroup.Key.TripNumber +
                                                                    detailsGroup.Key.TripSegNumber)
                    ), detailsGroup);
            if (containersTrip.Any())
            {
                TransactionList =
                    new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>(
                        groupedContainers);
            }
            
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
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _transactionList;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> TransactionList
        {
            get { return _transactionList; }
            set { SetProperty(ref _transactionList, value); }
        }


        // Command bindings
        public MvxCommand ConfirmTransactionsCommand { get; private set; }

        // Command Impl
        private void ExecuteConfirmTransactionsCommand()
        {
            // @TODO : Impement TripMaster logic so we know what a particular trip is composed of
            // @TODO : That is, Switch Trip => (Drop Empty, Pickup Full) then (Return To Yard)
            // This is hardcoded until the above is resolved
            Close(this);
            ShowViewModel<RouteDetailViewModel>(new {tripNumber = "615113"});
        }

    }
}
