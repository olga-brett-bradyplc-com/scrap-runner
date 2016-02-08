namespace Brady.ScrapRunner.Mobile.Views
{
    using ViewModels;
    using Xamarin.Forms;

    public partial class RouteDirectionsView : ContentPage
    {
        public RouteDirectionsView(string tripCustHostCode)
        {
            InitializeComponent();
            BindingContext = App.Locator.RouteDirections;
            ((RouteDirectionsViewModel)BindingContext).LoadAsync(tripCustHostCode);
        }
    }
}