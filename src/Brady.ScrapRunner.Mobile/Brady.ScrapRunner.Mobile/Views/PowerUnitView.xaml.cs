using Brady.ScrapRunner.Mobile.Resources;
using Brady.ScrapRunner.Mobile.ViewModels;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Views
{
    public partial class PowerUnitView
    {
        public PowerUnitView()
        {
            InitializeComponent();

            // Hide the navigation title bar
            NavigationPage.SetHasNavigationBar(this, false);

            // @TODO: Remove once proper navigation system is developed
            SetPowerUnitIdButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(new RouteSummaryView(), false);
                Navigation.RemovePage(this);
            };
        }
    }
}
