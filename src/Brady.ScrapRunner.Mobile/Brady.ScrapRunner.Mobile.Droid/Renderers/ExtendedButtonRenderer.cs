using Brady.ScrapRunner.Mobile.Droid.Renderers;
using Brady.ScrapRunner.Mobile.Renderers;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(ExtendedButton), typeof(ExtendedButtonRenderer))]
namespace Brady.ScrapRunner.Mobile.Droid.Renderers
{
    using Xamarin.Forms.Platform.Android.AppCompat;

    public class ExtendedButtonRenderer : ButtonRenderer
    {
    }
}