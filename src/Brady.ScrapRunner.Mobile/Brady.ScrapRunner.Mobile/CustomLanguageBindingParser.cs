using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Binding;
using MvvmCross.Binding.Parse.Binding;
using MvvmCross.Binding.Parse.Binding.Lang;

namespace Brady.ScrapRunner.Mobile
{
    public class CustomLanguageBindingParser : MvxBindingParser, IMvxLanguageBindingParser
    {
        protected override MvxSerializableBindingDescription ParseBindingDescription()
        {
            this.SkipWhitespace();

            string resourceName = (string)this.ReadValue();

            AppResources.Culture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = AppResources.Culture;

            // Pass the resource name in as the parameter on the StringResourceConverter.
            return new MvxSerializableBindingDescription
            {
                Converter = "StringResource",
                ConverterParameter = resourceName,
                Path = null,
                Mode = MvxBindingMode.OneTime
            };
        }

        public string DefaultConverterName { get; set; }

        public string DefaultTextSourceName { get; set; }
    }
}
