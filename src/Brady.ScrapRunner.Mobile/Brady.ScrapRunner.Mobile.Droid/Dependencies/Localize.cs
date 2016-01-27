using Brady.ScrapRunner.Mobile.Droid.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]
namespace Brady.ScrapRunner.Mobile.Droid.Dependencies
{
    using Interfaces;
    using Resources;

    public class Localize : ILocalize
    {
        public Localize()
        {
        }

        public System.Globalization.CultureInfo GetCurrentCultureInfo()
        {
            var androidLocale = Java.Util.Locale.Default;
            var netLanguage = androidLocale.ToString().Replace("_", "-"); // turns pt_BR into pt-BR
            return new System.Globalization.CultureInfo(netLanguage);
        }
    }
}