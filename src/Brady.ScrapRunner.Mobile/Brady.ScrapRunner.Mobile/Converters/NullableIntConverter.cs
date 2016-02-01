namespace Brady.ScrapRunner.Mobile.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = value as int?;
            return i == null ? null : string.Format(culture, "{0}", i.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i;
            if (int.TryParse((string)value, out i)) return i;
            return null;
        }
    }
}
