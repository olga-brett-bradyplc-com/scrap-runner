using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Converters
{
    // There's supposedly a built-in Xamarin ColorTypeConverter, but I was having problems getting that to work.
    // Simple enough to write a custom converter.
    class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? value : (string.IsNullOrEmpty(value.ToString()) ?
                value : Color.FromHex(value.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
