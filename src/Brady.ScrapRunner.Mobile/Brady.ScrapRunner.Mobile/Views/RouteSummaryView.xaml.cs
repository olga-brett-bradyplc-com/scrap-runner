namespace Brady.ScrapRunner.Mobile.Views
{
    using System;
    using Xamarin.Forms;

    public partial class RouteSummaryView : ContentPage
    {
        public RouteSummaryView()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            BindingContext = App.Locator.RouteSummary;
        }

        void OnClick(object sender, EventArgs e)
        {
            DisplayAlert("Not implemented yet!", "This has still yet to be implemented", "OK");
        }
    }
}