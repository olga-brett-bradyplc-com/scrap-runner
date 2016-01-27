using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Renderers
{
    /// <summary>
    /// An extended Xamarin.Forms.Entry control that includes properties for a border width, color, radius, etc.
    /// Currently there's only an android implementation as of 1/26/16
    /// </summary>
    public class ExtendedEntry : Entry
    {
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create<ExtendedEntry, double>(p => p.CornerRadius, 0);

        public double CornerRadius
        {
            get {  return (double) GetValue(CornerRadiusProperty); }
            set {  SetValue(CornerRadiusProperty, value); }
        }

        public static readonly BindableProperty StrokeColorProperty = 
            BindableProperty.Create<ExtendedEntry, Color>(p => p.StrokeColor, Color.Transparent);

        public Color StrokeColor
        {
            get { return (Color) GetValue(StrokeColorProperty); }
            set {  SetValue(StrokeColorProperty, value); }
        }

        public static readonly BindableProperty StrokeThicknessProperty =
            BindableProperty.Create<ExtendedEntry, double>(p => p.StrokeThickness, 0);

        public double StrokeThickness
        {
            get { return (double) GetValue(StrokeThicknessProperty); }
            set {  SetValue(StrokeThicknessProperty, value); }
        }

        // I don't like the idea of doing this, but having problems getting the renderer
        // to set the background color of the parent Entry field to transparent, so as to let the 
        // GradientDrawable be the default background
        public static readonly BindableProperty BgColorProperty =
            BindableProperty.Create<ExtendedEntry, Color>(p => p.BgColor, Color.White);

        public Color BgColor
        {
            get { return (Color) GetValue(BgColorProperty); }
            set {  SetValue(BgColorProperty, value); }
        }
    }
}
