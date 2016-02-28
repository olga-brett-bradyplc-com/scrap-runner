using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
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

        public override async void Start()
        {
            using (var tripDataLoad = UserDialogs.Instance.Loading("Loading Trip Data", maskType: MaskType.Clear))
            {
                var containersForSegment =
                    await ContainerHelper.ContainersForSegment(TripNumber, _tripSegmentRepository,
                        _tripSegmentContainerRepository);

                if (containersForSegment.Any())
                {
                    Containers =
                        new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>(
                            containersForSegment);
                }
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
