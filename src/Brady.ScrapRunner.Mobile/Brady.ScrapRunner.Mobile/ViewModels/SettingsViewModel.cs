﻿using MvvmCross.Localization;

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
            if (ShowViewModel<ChangeLanguageViewModel>())
                Close(this);
        }

        private string _changeLanguageLabel;

        public string ChangeLanguageLabel
        {
            get { return _changeLanguageLabel; }
            set { SetProperty(ref _changeLanguageLabel, value); }
        }
        public override async void Start()
        {
            ChangeLanguageLabel = AppResources.ChangeLanguage;

            base.Start();
        }

    }
}