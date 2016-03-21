using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Platform.Converters;

namespace Brady.ScrapRunner.Mobile.Converters
{
    public class DateTimeValueConverter : MvxValueConverter<DateTime, string>
    {
        protected override string Convert(DateTime value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value.ToString("hh:mm");
        }
    }
}
