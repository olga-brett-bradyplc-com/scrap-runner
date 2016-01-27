namespace Brady.ScrapRunner.Mobile.Resources
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using Interfaces;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        private readonly CultureInfo _cultureInfo;
        const string ResourceId = "Brady.ScrapRunner.Mobile.Resources.AppResources";

        public TranslateExtension()
        {
            _cultureInfo = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
        }

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null) return string.Empty;
            var resmgr = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
            var translation = resmgr.GetString(Text, _cultureInfo);
            if (translation == null)
            {
#if DEBUG
                throw new ArgumentException(
                    $"Key '{Text}' was not found in resources '{ResourceId}' for culture '{_cultureInfo.Name}'.", 
                    "Text");
#else
                translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }
    }
}
