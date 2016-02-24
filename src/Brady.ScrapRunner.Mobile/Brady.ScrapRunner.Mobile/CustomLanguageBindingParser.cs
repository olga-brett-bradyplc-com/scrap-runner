using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Binding;
using MvvmCross.Binding.Parse.Binding;
using MvvmCross.Binding.Parse.Binding.Lang;
using MvvmCross.Localization;

namespace Brady.ScrapRunner.Mobile
{
    public class CustomLanguageBindingParser : MvxBindingParser, IMvxLanguageBindingParser
    {
        protected override MvxSerializableBindingDescription ParseBindingDescription()
        {
            this.SkipWhitespace();

            string resourceName = (string)this.ReadValue();

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
