using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class PublicScaleSummaryViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;

        public PublicScaleSummaryViewModel(ITripService tripService)
        {
            _tripService = tripService;

            Title = AppResources.PublicScaleSummary;
            ContainerSelectedCommand = new MvxCommand<TripSegmentContainerModel>(ExecuteContainerSelectedCommand);
        }
        public void Init(string tripNumber, string methodOfEntry)
        {
            TripNumber = tripNumber;
            MethodOfEntry = methodOfEntry;
            SubTitle = $"{AppResources.Trip} {TripNumber}";
        }
        private string _methodOfEntry;

        public string MethodOfEntry
        {
            get { return _methodOfEntry; }
            set { SetProperty(ref _methodOfEntry, value); }
        }
        public override async void Start()
        {
            using (var tripDataLoad = UserDialogs.Instance.Loading(AppResources.LoadingTripData, maskType: MaskType.Clear))
            {
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
            }

            base.Start();
        }
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
            ShowViewModel<PublicScaleDetailViewModel>(new
            {
                tripNumber = selectedSegment.TripNumber,
                tripSegNumber = selectedSegment.TripSegNumber,
                tripSegContainerSeqNumber = selectedSegment.TripSegContainerSeqNumber,
                tripSegContainerNumber = selectedSegment.TripSegContainerNumber,
                methodOfEntry = MethodOfEntry
            });
        }
    }
}
