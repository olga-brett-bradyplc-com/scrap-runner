using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionDetailViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;

        public TransactionDetailViewModel(ITripService tripService)
        {
            _tripService = tripService;
            Title = AppResources.PickupFull;
        }

        // Initialize parameters passed from transaction summary screen
        public void Init(string tripNumber, string tripSegNumber, string tripSegContainerNumber)
        {
            TripNumber = tripNumber;
            TripSegNumber = tripSegNumber;
            TripSegContainerNumber = tripSegContainerNumber;
            SubTitle = TripNumber;
        }

        public override async void Start()
        {
            var container =
                await _tripService.FindTripSegmentContainer(TripNumber, TripSegNumber, TripSegContainerSeqNumber);

            //ContainerId = container.TripSegContainerNumber ?? "";
            //Location = container.TripSegContainerLocation ?? "";
            //Commodity = container.TripSegContainerCommodityDesc ?? "";
            //Level = container.TripSegContainerLevel;
            //Notes = container.TripSegContainerComment ?? "";
            //ReferenceNumber = container.TripSegContainerReferenceNumber;

            MenuFilter = MenuFilterEnum.OnTransaction;

            base.Start();
        }

        // Command bindings
        public MvxCommand TransactionSavedCommand { get; private set; }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _tripSegNumber;
        public string TripSegNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripSegNumber, value); }
        }

        private int? _tripSegContainerSeqNumber;

        public int? TripSegContainerSeqNumber
        {
            get { return _tripSegContainerSeqNumber; }
            set { SetProperty(ref _tripSegContainerSeqNumber, value); }
        }

        private string _tripSegContainerNumber;
        public string TripSegContainerNumber
        {
            get { return _tripSegContainerNumber; }
            set { SetProperty(ref _tripSegContainerNumber, value); }
        }

        private string _containerId;
        public string ContainerId
        {
            get { return _containerId; }
            set
            {
                SetProperty(ref _containerId, value);
            }
        }

        private string _location;
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }

        private string _commodity;
        public string Commodity
        {
            get { return _commodity; }
            set { SetProperty(ref _commodity, value); }
        }

        private int? _level;
        public int? Level
        {
            get { return _level; }
            set{ SetProperty(ref _level, value); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value); }
        }

        private string _referenceNumber;
        public string ReferenceNumber
        {
            get { return _referenceNumber; }
            set { SetProperty(ref _referenceNumber, value); }
        }

    }
}