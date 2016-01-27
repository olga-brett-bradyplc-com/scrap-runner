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
            NavigationPage.SetHasNavigationBar(this, false);

            // TODO: Refactor out once proper navigation system is developed
            SetPowerUnitIdButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(new RouteSummaryView(), false);
            };
        }
    }
}
