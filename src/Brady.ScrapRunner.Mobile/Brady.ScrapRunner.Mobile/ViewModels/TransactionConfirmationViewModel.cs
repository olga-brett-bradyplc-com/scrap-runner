using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
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
            Title = "Signature Receipt";
            ConfirmTransactionsCommand = new MvxCommand(ExecuteConfirmTransactionsCommand);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = $"Trip {tripNumber}";
        }

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
        public MvxCommand ConfirmTransactionsCommand { get; private set; }

        // Command Impl
        private async void ExecuteConfirmTransactionsCommand()
        {
            using (var completeTripSegment = UserDialogs.Instance.Loading("Completing Trip Segment", maskType: MaskType.Clear))
            {
                foreach (Grouping<TripSegmentModel, TripSegmentContainerModel> grouping in Containers)
                {
                    foreach (TripSegmentContainerModel tscm in grouping)
                    {
                        await _tripSegmentContainerRepository.DeleteAsync(tscm);
                    }

                    await _tripSegmentRepository.DeleteAsync(grouping.Key);
                }
            }
            Close(this);
            ShowViewModel<RouteDetailViewModel>(new {tripNumber = TripNumber});
        }

    }
}
