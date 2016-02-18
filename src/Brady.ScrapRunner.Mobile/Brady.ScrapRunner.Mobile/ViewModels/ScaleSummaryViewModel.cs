using System.Linq;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class ScaleSummaryViewModel : BaseViewModel
    {
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;
          
        public ScaleSummaryViewModel(
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository )
        {
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;

            Title = "Yard/Scale Summary";

            ContainerSelectedCommand = new MvxCommand<TripSegmentContainerModel>(ExecuteContainerSelectedCommand);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = $"Trip {TripNumber}";
        }

        public override void Start()
        {
            Task.Run(async () =>
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
            });
            base.Start();
        }

        // Listview bindings
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> TransactionList { get; private set; }

        // Command bindings
        public MvxCommand<TripSegmentContainerModel> ContainerSelectedCommand { get; private set; }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }
        private ContainerMasterModel _containerSelected;
        public ContainerMasterModel ContainerSelected
        {
            get { return _containerSelected; }
            set { SetProperty(ref _containerSelected, value); }
        }

        // Command impl
        public void ExecuteContainerSelectedCommand(TripSegmentContainerModel selectedSegment)
        {
            Close(this);
            ShowViewModel<ScaleDetailViewModel>(new
            {
                tripNumber = selectedSegment.TripNumber,
                tripSegNumber = selectedSegment.TripSegNumber,
                tripSegContainerNumber = selectedSegment.TripSegContainerNumber
            });
        }
    }
}
