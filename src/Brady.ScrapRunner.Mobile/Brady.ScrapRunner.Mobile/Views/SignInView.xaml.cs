namespace Brady.ScrapRunner.Mobile.Views
{
    using Xamarin.Forms;

    public partial class SignInView : ContentPage
    {
        public SignInView()
        {
            InitializeComponent();
            // Hide the navigation title bar
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = App.Locator.SignIn;
        }
    }
}
