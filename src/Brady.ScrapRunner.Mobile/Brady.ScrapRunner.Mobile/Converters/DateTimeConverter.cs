namespace Brady.ScrapRunner.Mobile.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = value as DateTime?;
            if (parameter == null) return dateTime?.ToString("HHmm");
            var stringFormat = parameter as string;
            return dateTime?.ToString(stringFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}