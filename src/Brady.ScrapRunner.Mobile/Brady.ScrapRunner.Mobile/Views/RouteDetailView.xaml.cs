namespace Brady.ScrapRunner.Mobile.Views
{
    using System;
    using Xamarin.Forms;

    public partial class RouteDetailView : ContentPage
    {
        public RouteDetailView()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            BindingContext = App.Locator.RouteDetail;
        }

        void OnClick(object sender, EventArgs e)
        {
            DisplayAlert("Not implemented yet!", "This has still yet to be implemented", "OK");
        }
    }
}
