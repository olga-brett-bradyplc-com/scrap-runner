using System.Net;
using System.Reflection;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using MvvmCross.Binding.Parse.Binding.Lang;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.IoC;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using Plugin.Settings.Abstractions;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Droid.Services;

namespace Brady.ScrapRunner.Mobile.Droid
{
    public class Setup : MvxAppCompatSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IEnumerable<Assembly> AndroidViewAssemblies => new List<Assembly>(base.AndroidViewAssemblies)
        {
            typeof(Android.Support.V7.Widget.RecyclerView).Assembly,
            typeof(Android.Support.Design.Widget.FloatingActionButton).Assembly,
            typeof(Android.Support.Design.Widget.NavigationView).Assembly
        };

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

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
//#if DEBUG
            // Ignore SSL certificate errors while debugging.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
//#endif
            // This plugin doesn't use MvvmCross Plugin Bootstrapper.
            Mvx.RegisterType<ISettings, Plugin.Settings.SettingsImplementation>();

            Mvx.RegisterType<IMvxLanguageBindingParser, CustomLanguageBindingParser>();
            Mvx.RegisterType<INotification, AndroidNotification>();

            Mvx.LazyConstructAndRegisterSingleton<INetworkAvailabilityService>(() => new AndroidNetworkAvailabilityService());
            Mvx.LazyConstructAndRegisterSingleton<IBackgroundScheduler>(() => new AndroidBackgroundScheduler());
            Mvx.LazyConstructAndRegisterSingleton<INotificationService>(() => new AndroidNotificationService());
            Mvx.LazyConstructAndRegisterSingleton<IScrapRunnerNotificationService, AndroidScrapRunnerNotificationService>();
        }
    }
}