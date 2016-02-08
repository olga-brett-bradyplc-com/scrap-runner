using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Renderers
{
    class NavigationButton : Button
    {
        public static readonly BindableProperty CurrentStatusProperty =
            BindableProperty.Create<NavigationButton, string>(p => p.CurrentStatus, "");

        public string CurrentStatus
        {
            get { return (string)GetValue(CurrentStatusProperty); }
            set { SetValue(CurrentStatusProperty, value); }
        }
    }
}
