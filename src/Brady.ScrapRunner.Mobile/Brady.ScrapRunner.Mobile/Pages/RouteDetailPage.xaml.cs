namespace Brady.ScrapRunner.Mobile.Pages
{
    using Renderers;
    using Xamarin.Forms;

    public partial class RouteDetailPage : ExtendedContentPage
    {
        public RouteDetailPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
        }
    }
}