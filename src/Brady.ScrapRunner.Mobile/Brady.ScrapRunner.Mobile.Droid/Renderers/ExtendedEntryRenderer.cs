using Android.Graphics.Drawables;
using Brady.ScrapRunner.Mobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Brady.ScrapRunner.Mobile.Renderers;

[assembly: ExportRenderer(typeof(ExtendedEntry), typeof(ExtendedEntryRenderer))]
namespace Brady.ScrapRunner.Mobile.Droid.Renderers
{
    class ExtendedEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            ExtendedEntry be = (ExtendedEntry) Element;

            if (Control != null)
            {
                GradientDrawable gd = new GradientDrawable();
                gd.SetColor(be.BgColor.ToAndroid());
                gd.SetCornerRadius((float)be.CornerRadius);
                gd.SetStroke((int)be.StrokeThickness, be.StrokeColor.ToAndroid());
                Control.SetBackground(gd);
            }
            
        }
    }
}