namespace Brady.ScrapRunner.Mobile.Views
{
    using ViewModels;
    using System.Collections.Generic;
    using Xamarin.Forms;
    using System.Collections.ObjectModel;
    using System;
    using Models;
    public partial class RouteSummaryView
    {
        public RouteSummaryView()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);

            // TODO: Refactor out once proper navigation system is developed
            RouteSummaryListView.ItemSelected += async (sender, e) =>
            {
                await Navigation.PushAsync(new RouteDetailView(), false);
            };
        }

        void OnClick(object sender, EventArgs e)
        {
            DisplayAlert("Not implemented yet!", "This has still yet to be implemented", "OK");
        }
    }
}