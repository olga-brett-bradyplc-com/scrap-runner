using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Views
{
    public partial class RouteDetailView
    {
        public RouteDetailView()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
        }

        void OnClick(object sender, EventArgs e)
        {
            DisplayAlert("Not implemented yet!", "This has still yet to be implemented", "OK");
        }
    }
}
