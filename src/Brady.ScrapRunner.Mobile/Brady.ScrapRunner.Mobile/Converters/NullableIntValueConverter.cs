using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Platform.Converters;

namespace Brady.ScrapRunner.Mobile.Converters
{
    public class NullableIntValueConverter : MvxValueConverter<int?, string>
    {
        protected override string Convert(int? value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value.ToString();
        }

        protected override int? ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return int.Parse(value);
        }
    }
}
