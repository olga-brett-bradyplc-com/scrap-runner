using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Helpers;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class TransactionSummaryViewModel : BaseViewModel
    {
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository; 

        public TransactionSummaryViewModel(
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository )
        {
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            Title = "Transactions";
        }

        // Initialize parameter passed from Route Detail Screen
        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = TripNumber;
            ConfirmationSelectedCommand = new MvxCommand(ExecuteConfirmationSelectedCommand);
        }

        // Grab all relevant data
        public override async void Start()
        {
            var containersForSegment = await ContainerHelper.ContainersForSegment(TripNumber, _tripSegmentRepository,
                _tripSegmentContainerRepository);
            if (containersForSegment.Any())
            {
                Containers =
                    new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>(
                        containersForSegment);
            }

            base.Start();
        }

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand TransactionSelectedCommand { get; private set; }
        public MvxCommand TransactionScannedCommand { get; private set; }
        public MvxCommand ConfirmationSelectedCommand { get; private set; }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        // Command impl
        public void ExecuteTransactionSelectedCommand()
        {
            ShowViewModel<TransactionDetailViewModel>();
        }

        public void ExecuteConfirmationSelectedCommand()
        {
            Close(this);
            ShowViewModel<TransactionConfirmationViewModel>(new {tripNumber = TripNumber});
        }
    }
}