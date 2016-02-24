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
        public ChangeLanguageViewModel()
        {
            Title = AppResources.ChangeLanguage;
            SelectLanguageCommand = new MvxCommand<CultureInfo>(ExecuteSelectLanguageCommand);
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            CultureInfo newCulture;
            if (currentCulture.Name.Equals("es"))
                newCulture = new CultureInfo("en-US");
            else if (currentCulture.Name.Equals("en-US") || currentCulture.Name.Equals("en"))
                newCulture = new CultureInfo("es");
            else
                newCulture = new CultureInfo("en-US");
            Languages.Add(newCulture);
        }

        private ObservableCollection<CultureInfo> _languages = new ObservableCollection<CultureInfo>();
        public ObservableCollection<CultureInfo> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        private void ExecuteSelectLanguageCommand(CultureInfo cultureInfo)
        {
            AppResources.Culture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            Close(this);
        }
         public MvxCommand<CultureInfo> SelectLanguageCommand { get; private set; }
        public IMvxLanguageBinder TextSource
        {
            get { return new MvxLanguageBinder("", GetType().Name); }
        }
    }
}