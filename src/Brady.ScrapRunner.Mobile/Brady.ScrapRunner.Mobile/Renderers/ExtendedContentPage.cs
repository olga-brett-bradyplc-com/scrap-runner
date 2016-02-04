using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Renderers
{
    public class ExtendedContentPage : ContentPage
    {
        public static readonly BindableProperty SubTitleProperty =
            BindableProperty.Create<ExtendedContentPage, string>(p => p.SubTitle, "");

        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }

        public static readonly BindableProperty AppBarColorProperty =
            BindableProperty.Create<ExtendedContentPage, Color>(p => p.AppBarColor, Color.Gray);

        public Color AppBarColor
        {
            get { return (Color) GetValue(AppBarColorProperty); }
            set { SetValue(AppBarColorProperty, value); }
        }
    }
}
