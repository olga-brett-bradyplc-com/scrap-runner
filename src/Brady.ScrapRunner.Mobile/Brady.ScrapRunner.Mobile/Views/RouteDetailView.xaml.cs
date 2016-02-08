namespace Brady.ScrapRunner.Mobile.Views
{
    using Renderers;
    using System;
    using ViewModels;
    using Xamarin.Forms;

    public partial class RouteDetailView : ExtendedContentPage
    {
        public RouteDetailView(string tripNumber)
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            BindingContext = App.Locator.RouteDetail;
            ((RouteDetailViewModel) BindingContext).TripNumber = tripNumber;
        }
    }
}
