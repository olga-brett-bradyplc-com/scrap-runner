using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
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
        private static readonly string[] SupportedCultureTags = { "en-US", "es" };

        public SettingsViewModel()
        {
            Title = AppResources.Settings;
        }
        
        public override void Start()
        {
            LanguageSettingLabel = AppResources.LanguageLabel;
            LanguageSettingSubLabel = CultureInfo.CurrentCulture.DisplayName;

            // @TODO : Put in a mechanism to either disable/hide server settings when logged in, or make them re-log in after changing
            ServerSettingLabel = AppResources.ServerLabel;
            ServerSettingSubLabel = PhoneSettings.ServerSettings;
        }

        private string _languageSettingLabel;
        public string LanguageSettingLabel
        {
            get { return _languageSettingLabel; }
            set { SetProperty(ref _languageSettingLabel, value); }
        }

        private string _languageSettingSubLabel;
        public string LanguageSettingSubLabel
        {
            get { return _languageSettingSubLabel; }
            set { SetProperty(ref _languageSettingSubLabel, value); }
        }

        private string _serverSettingLabel;
        public string ServerSettingLabel
        {
            get { return _serverSettingLabel; }
            set { SetProperty(ref _serverSettingLabel, value); }
        }

        private string _serverSettingSubLabel;
        public string ServerSettingSubLabel
        {
            get { return _serverSettingSubLabel; }
            set { SetProperty(ref _serverSettingSubLabel, value); }
        }

        
        private IMvxAsyncCommand _changeLanguageCommand;
        public IMvxAsyncCommand ChangeLanguageCommand => _changeLanguageCommand ?? 
            (_changeLanguageCommand = new MvxAsyncCommand(ExecuteChangeLanguageCommand));

        private async Task ExecuteChangeLanguageCommand()
        {
            var languages = SupportedCultureTags.Select(sc => new CultureInfo(sc));
            var languageDialogAsync =
                await
                    UserDialogs.Instance.ActionSheetAsync(AppResources.SelectLanguage, "", AppResources.Cancel, null,
                        languages.Select(c => c.DisplayName).ToArray());

            if (languageDialogAsync != AppResources.Cancel)
            {
                var language = languages.FirstOrDefault(ct => ct.DisplayName == languageDialogAsync);
                AppResources.Culture = language;
                CultureInfo.DefaultThreadCurrentCulture = language;
                LanguageSettingSubLabel = language.DisplayName;
            }
        }

        private IMvxAsyncCommand _changeServerSettings;
        public IMvxAsyncCommand ChangeServerSettings => _changeServerSettings ?? (_changeServerSettings = new MvxAsyncCommand(ExecuteChangeServerSettings));

        private async Task ExecuteChangeServerSettings()
        {
            var prompt = await UserDialogs.Instance.PromptAsync(AppResources.ServerInput, AppResources.ServerLabel, AppResources.Save, AppResources.Cancel, PhoneSettings.ServerSettings, InputType.Url);
            if (!string.IsNullOrEmpty(prompt.Text))
            {
                PhoneSettings.ServerSettings = prompt.Text.Replace("https://", "");
                ServerSettingSubLabel = PhoneSettings.ServerSettings;
            }
        }
    }

    //public class SettingsWrapper
    //{
    //    public SettingsWrapper(string key, string title, string subTitle, IMvxAsyncCommand command)
    //    {
    //        Key = key;
    //        Title = title;
    //        SubTitle = subTitle;
    //        ParentCommand = command;
    //    }

    //    public string Key { get; set; }
    //    public string SubTitle { get; set; }
    //    public string Title { get; set; }

    //    public IMvxAsyncCommand ParentCommand { get; set; }

    //    private IMvxAsyncCommand _localCommand;
    //    public IMvxAsyncCommand LocalCommand => _localCommand ?? (_localCommand = new MvxAsyncCommand(ExecuteParentCommand));

    //    private async Task ExecuteParentCommand()
    //    {
    //        await ParentCommand.ExecuteAsync();
    //    }
    //}
}