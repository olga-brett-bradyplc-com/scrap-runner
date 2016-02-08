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

        // Subtitle property
        public static readonly BindableProperty SubTitleProperty =
            BindableProperty.Create<ExtendedContentPage, string>(p => p.SubTitle, "");

        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }

        // Navigation bar property
        public static readonly BindableProperty BarBgColorProperty =
            BindableProperty.Create<ExtendedContentPage, string>(p => p.BarBgColor, "");

        public string BarBgColor
        {
            get { return (string)GetValue(BarBgColorProperty); }
            set { SetValue(BarBgColorProperty, value); }
        }
    }
}
