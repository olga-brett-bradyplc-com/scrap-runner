namespace Brady.ScrapRunner.Mobile.Views
{
    public partial class SignInView
    {
        public SignInView()
        {
            InitializeComponent();
            // @TODO: This is just temporary!
            SignInButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(new PowerUnitView(), false);
            };
        }
    }
}
