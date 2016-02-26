using System.Collections.Generic;
using Android.Views;
using MvvmCross.Binding.Parse.Binding.Lang;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.IoC;

namespace Brady.ScrapRunner.Mobile.Droid
{
    using Android.Content;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Platform;

    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
            #if DEBUG
            // Note since self-signed, we set server certificate validation callback to not complain.
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });
            #endif
        }
        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            Mvx.RegisterType<IMvxLanguageBindingParser, CustomLanguageBindingParser>();
        }
        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IDictionary<string, string> ViewNamespaceAbbreviations
        {
            get
            {
                var abbreviations = base.ViewNamespaceAbbreviations;
                abbreviations["MvxExt"] = "Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView";
                return abbreviations;
            }
        }

        protected override void FillViewTypes(IMvxTypeCache<View> cache)
        {
            base.FillViewTypes(cache);
            cache.AddAssembly(typeof(Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView.BindableGroupListView).Assembly);
        }
        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);
            registry.AddOrOverwrite("Language", new MvxLanguageConverter());
        }
    }
}
