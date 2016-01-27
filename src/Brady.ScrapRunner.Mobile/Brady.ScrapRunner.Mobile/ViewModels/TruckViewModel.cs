namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using Xamarin.Forms;

    public class TruckViewModel : BaseViewModel
    {
        public TruckViewModel()
        {
            ContinueCommand = new Command(ExecuteContinueCommand);
        }

        private string _truckId;
        public string TruckId
        {
            get { return _truckId; }
            set { SetProperty(ref _truckId, value); }
        }

        private int? _odometer;
        public int? Odometer
        {
            get { return _odometer; }
            set { SetProperty(ref _odometer, value); }
        }

        public Command ContinueCommand { get; protected set; }

        protected void ExecuteContinueCommand()
        {
        }
    }
}
