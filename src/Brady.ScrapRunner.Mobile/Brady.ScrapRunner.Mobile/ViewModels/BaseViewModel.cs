namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get {  return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _subTitle;

        public string SubTitle
        {
            get { return _subTitle; }
            set { SetProperty(ref _subTitle, value); }
        }

        protected void SetProperty<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingField, newValue)) return;
            backingField = newValue;
            if (propertyName != null) OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
