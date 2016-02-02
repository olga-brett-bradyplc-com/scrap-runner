namespace Brady.ScrapRunner.Mobile.Views
{
    using Xamarin.Forms;

    public partial class SettingsView : ContentPage
    {
        public SettingsView()
        {
            InitializeComponent();
            BindingContext = App.Locator.Settings;
        }
    }
}
