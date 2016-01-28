using Android.Graphics.Drawables;
using Brady.ScrapRunner.Mobile.Droid.Renderers;
using Brady.ScrapRunner.Mobile.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedContentPage), typeof(ExtendedContentPageRenderer))]
namespace Brady.ScrapRunner.Mobile.Droid.Renderers
{
    class ExtendedContentPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            var ecp = (ExtendedContentPage) Element;
            var appBar = ((FormsApplicationActivity) Context).ActionBar;
            
            appBar.SetBackgroundDrawable(new ColorDrawable(Color.Maroon.ToAndroid()));
            appBar.Subtitle = ecp.SubTitle;
        }
    }
}