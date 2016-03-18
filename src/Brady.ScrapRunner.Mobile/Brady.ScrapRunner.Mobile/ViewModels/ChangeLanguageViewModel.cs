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
        private static readonly string[] supportedCultureTags = { "en-US", "es" };
        public ChangeLanguageViewModel()
        {
            Title = AppResources.ChangeLanguage;
            SelectLanguageCommand = new MvxCommand<CultureInfo>(ExecuteSelectLanguageCommand);
        }
        public override async void Start()
        { 
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            foreach (var supportedCulture in supportedCultureTags)
            {
                if (!currentCulture.Name.Equals(supportedCulture))
                {
                    CultureInfo newCulture = new CultureInfo(supportedCulture);
                    Languages.Add(newCulture);
                }
            }

            base.Start();
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
            ShowViewModel<SignInViewModel>();
            Close(this);
        }
        public MvxCommand<CultureInfo> SelectLanguageCommand { get; private set; }
     }
}