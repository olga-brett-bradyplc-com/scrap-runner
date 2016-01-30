namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using GalaSoft.MvvmLight;

    public class BaseViewModel : ViewModelBase
    {
        public BaseViewModel()
        {
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
    }
}