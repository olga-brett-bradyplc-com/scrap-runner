using System.Collections.Generic;
using Android.Views;
using MvvmCross.Binding.Parse.Binding.Lang;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.IoC;

namespace Brady.ScrapRunner.Mobile.Droid
{
    using System.Net;
    using System.Reflection;
    using Android.Content;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Platform;

    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
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
        protected override IEnumerable<Assembly> AndroidViewAssemblies => new List<Assembly>(base.AndroidViewAssemblies)
        {
            typeof(Android.Support.V7.Widget.Toolbar).Assembly,
            typeof(Android.Support.V7.Widget.RecyclerView).Assembly,
            typeof(Android.Support.V4.Widget.DrawerLayout).Assembly,
            typeof(Android.Support.V4.View.ViewPager).Assembly,
            typeof(Android.Support.Design.Widget.FloatingActionButton).Assembly
        };
        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
#if DEBUG
            // Ignore SSL certificate errors while debugging.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif
        }
    }
}