using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Renderers;
using Brady.ScrapRunner.Mobile.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedContentPage), typeof(ExtendedPageRenderer))]
namespace Brady.ScrapRunner.Mobile.Droid.Renderers
{
    class ExtendedPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            var element = (ExtendedContentPage) Element;
            var appBar = ((Activity) Context).ActionBar;
            appBar.Subtitle = element.SubTitle;
        }
    }
}