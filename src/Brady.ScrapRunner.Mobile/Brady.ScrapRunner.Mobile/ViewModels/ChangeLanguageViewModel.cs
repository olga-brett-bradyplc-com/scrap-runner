using System.Linq;
using System.Windows.Input;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Localization;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class ChangeLanguageViewModel : BaseViewModel
    {
        private static readonly string[] SupportedCultureTags = { "en-US", "es" };

        public ChangeLanguageViewModel()
        {
            Title = AppResources.ChangeLanguage;
            SelectLanguageCommand = new MvxCommand<CultureInfo>(ExecuteSelectLanguageCommand);
        }

        public override void Start()
        {
            CurrentCulture = CultureInfo.CurrentCulture;
            foreach (var newCulture in SupportedCultureTags.Select(supportedCulture => new CultureInfo(supportedCulture)))
                Languages.Add(newCulture);

            base.Start();
        }

        private ObservableCollection<CultureInfo> _languages = new ObservableCollection<CultureInfo>();
        public ObservableCollection<CultureInfo> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        private CultureInfo _currentCulture;
        public CultureInfo CurrentCulture
        {
            get { return _currentCulture; }
            set { SetProperty(ref _currentCulture, value); }
        }

        private void ExecuteSelectLanguageCommand(CultureInfo cultureInfo)
        {
            AppResources.Culture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            Close(this);
            ShowViewModel<SettingsViewModel>();
        }

        public MvxCommand<CultureInfo> SelectLanguageCommand { get; private set; }
     }
}