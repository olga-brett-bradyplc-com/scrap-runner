using System.Windows.Input;
using Brady.ScrapRunner.Mobile.Helpers;
using MvvmCross.Localization;
using Brady.ScrapRunner.Mobile.Resources;
using Plugin.Settings.Abstractions;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Title = AppResources.Settings;
        }

        private string _currentLanguage;
        public string CurrentLanguage
        {
            get { return _currentLanguage; }
            set { SetProperty(ref _currentLanguage, value); }
        }

        private MvxCommand _changeLanguageCommand;
        public MvxCommand ChangeLanguageCommand => _changeLanguageCommand ?? 
            (_changeLanguageCommand = new MvxCommand(ExecuteChangeLanguageCommand));

        private void ExecuteChangeLanguageCommand()
        {
            ShowViewModel<ChangeLanguageViewModel>();
        }
    }
}