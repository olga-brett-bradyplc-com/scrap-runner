namespace Brady.ScrapRunner.Mobile.Converters
{
    using System;
    using System.Globalization;
    using MvvmCross.Platform.Converters;

    public class DateTimeOffsetValueConverter : MvxValueConverter<DateTimeOffset, string>
    {
        protected override string Convert(DateTimeOffset value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return value.ToString("HH:mm", culture);
            var format = parameter as string;
            return value.ToString(format, culture);
        }
    }
}
