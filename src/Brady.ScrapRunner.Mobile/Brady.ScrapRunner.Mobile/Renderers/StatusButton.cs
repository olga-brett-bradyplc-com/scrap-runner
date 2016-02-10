using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Renderers
{
    // A simple extension of button that can have a "status" binding in order to work in conjunction with actiontriggers
    class StatusButton : Button
    {
        public static readonly BindableProperty CurrentStatusProperty =
            BindableProperty.Create<StatusButton, string>(p => p.CurrentStatus, "");

        public string CurrentStatus
        {
            get { return (string)GetValue(CurrentStatusProperty); }
            set { SetValue(CurrentStatusProperty, value); }
        }
    }
}
