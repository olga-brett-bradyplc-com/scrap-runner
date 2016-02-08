namespace Brady.ScrapRunner.Mobile.Views
{
    using System;
    using ViewModels;
    using Xamarin.Forms;

    public partial class RouteDetailView : ContentPage
    {
        public RouteDetailView(string tripNumber)
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            // @TODO: This isn't right.
            BindingContext = App.Locator.RouteDetail;
            ((RouteDetailViewModel) BindingContext).LoadAsync(tripNumber);
        }

        void OnClick(object sender, EventArgs e)
        {
            DisplayAlert("Not implemented yet!", "This has still yet to be implemented", "OK");
        }
    }
}
