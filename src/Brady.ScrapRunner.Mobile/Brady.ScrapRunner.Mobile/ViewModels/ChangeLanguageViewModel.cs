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
            ChangeLanguageCommand = new MvxCommand<CultureInfo>(ExecuteChangeLanguageCommand);
        }

        private ObservableCollection<CultureInfo> _languages = new ObservableCollection<CultureInfo>();
        public ObservableCollection<CultureInfo> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        private void ExecuteChangeLanguageCommand(CultureInfo cultureInfo)
        {
            AppResources.Culture = cultureInfo;
            Close(this);
        }

        public MvxCommand<CultureInfo> ChangeLanguageCommand { get; private set; }
    }
}