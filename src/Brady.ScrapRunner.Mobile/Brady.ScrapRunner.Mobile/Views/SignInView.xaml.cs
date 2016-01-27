using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Views
{
    public partial class SignInView
    {
        public SignInView()
        {
            InitializeComponent();

            // Hide the navigation title bar
            NavigationPage.SetHasNavigationBar(this, false);

            // @TODO: This is just temporary!
            SignInButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(new PowerUnitView(), false);
            };
        }
    }
}
