namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Models;

    public class SettingsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public SettingsViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ChangeLanguageCommand = new RelayCommand(ExecuteChangeLanguageCommand);
        }

        private string _currentLanguage;
        public string CurrentLanguage
        {
            get { return _currentLanguage; }
            set { Set(ref _currentLanguage, value); }
        }

        public RelayCommand ChangeLanguageCommand { get; private set; }
        private void ExecuteChangeLanguageCommand()
        {
            _navigationService.NavigateTo(Locator.ChangeLanguageView);
        }
    }
}