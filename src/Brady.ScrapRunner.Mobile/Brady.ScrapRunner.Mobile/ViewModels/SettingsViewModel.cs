namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;
    using Resources;

    public class SettingsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public SettingsViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = AppResources.Settings;
        }

        private string _currentLanguage;
        public string CurrentLanguage
        {
            get { return _currentLanguage; }
            set { Set(ref _currentLanguage, value); }
        }

        private RelayCommand _changeLanguageCommand;
        public RelayCommand ChangeLanguageCommand => _changeLanguageCommand ?? 
            (_changeLanguageCommand = new RelayCommand(ExecuteChangeLanguageCommand));

        private void ExecuteChangeLanguageCommand()
        {
            _navigationService.NavigateTo(Locator.ChangeLanguageView);
        }
    }
}