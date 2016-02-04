namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Resources;

    public class ChangeLanguageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public ChangeLanguageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ChangeLanguageCommand = new RelayCommand<CultureInfo>(ExecuteChangeLanguageCommand);
        }

        private ObservableCollection<CultureInfo> _languages = new ObservableCollection<CultureInfo>();
        public ObservableCollection<CultureInfo> Languages
        {
            get { return _languages; }
            set { Set(ref _languages, value); }
        }

        private void ExecuteChangeLanguageCommand(CultureInfo cultureInfo)
        {
            AppResources.Culture = cultureInfo;
            _navigationService.GoBack();
        }

        public RelayCommand<CultureInfo> ChangeLanguageCommand { get; private set; }
    }
}