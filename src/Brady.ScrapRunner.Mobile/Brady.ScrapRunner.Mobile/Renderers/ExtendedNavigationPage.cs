using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Renderers
{
    class ExtendedNavigationPage : NavigationPage
    {
        public static readonly BindableProperty SubTitleProperty =
            BindableProperty.Create<ExtendedNavigationPage, string>(p => p.SubTitle, "");

        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }
    }
}
