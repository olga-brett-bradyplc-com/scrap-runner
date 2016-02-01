namespace Brady.ScrapRunner.Mobile.Views
{
    using Xamarin.Forms;

    public partial class PowerUnitView : ContentPage
    {
        public PowerUnitView()
        {
            InitializeComponent();
            // Hide the navigation title bar
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = App.Locator.PowerUnit;
        }
    }
}
