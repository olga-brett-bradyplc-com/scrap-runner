using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Views
{
    public partial class ScaleDetailView : ContentPage
    {
        public ScaleDetailView()
        {
            InitializeComponent();
            BindingContext = App.Locator.ScaleDetail;
        }
    }
}
