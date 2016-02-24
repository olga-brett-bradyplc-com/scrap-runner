using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Platform.Converters;

namespace Brady.ScrapRunner.Mobile
{
    public class StringResourceConverter : IMvxValueConverter
    {
        private static ResourceManager manager = new ResourceManager(typeof(AppResources));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ignore value. We are using parameter only.
            return manager.GetString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
