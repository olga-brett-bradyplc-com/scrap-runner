using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionDetailViewModel : BaseViewModel
    {
        public TransactionDetailViewModel()
        {
            Title = AppResources.PickupFull;
            //Subtitle = "Trip 615112";
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
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
            set
            {
                SetProperty(ref _location, value);
            }
        }

        private string _commodity;
        public string Commodity
        {
            get { return _commodity; }
            set
            {
                SetProperty(ref _commodity, value);
            }
        }

        private string _level;
        public string Level
        {
            get { return _level; }
            set
            {
                SetProperty(ref _level, value);
            }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set
            {
                SetProperty(ref _notes, value);
            }
        }

        private string _referenceNumber;
        public string ReferenceNumber
        {
            get { return _referenceNumber; }
            set
            {
                SetProperty(ref _referenceNumber, value);
            }
        }

    }
}