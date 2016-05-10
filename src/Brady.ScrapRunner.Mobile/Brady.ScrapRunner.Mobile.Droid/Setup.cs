using System.Collections.Generic;
using Android.Views;
using MvvmCross.Binding.Parse.Binding.Lang;
using MvvmCross.Droid.Shared.Presenter;
using MvvmCross.Droid.Views;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.IoC;
using System.Net;
using System.Reflection;
using Android.Content;
using Android.Widget;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace Brady.ScrapRunner.Mobile.Droid
{
    public class Setup : MvxAndroidSetup
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
            typeof(Android.Support.V7.Widget.Toolbar).Assembly,
            typeof(Android.Support.V7.Widget.RecyclerView).Assembly,
            typeof(Android.Support.V4.Widget.DrawerLayout).Assembly,
            typeof(Android.Support.V4.View.ViewPager).Assembly,
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

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var mvxFragmentsPresenter = new MvxFragmentsPresenter(AndroidViewAssemblies);
            Mvx.RegisterSingleton<IMvxAndroidViewPresenter>(mvxFragmentsPresenter);
            return mvxFragmentsPresenter;
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

        // Implemented to fix issues with SelectedItem binding not working with MvxSpinner
        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            MvxAppCompatSetupHelper.FillTargetFactories(registry);
            base.FillTargetFactories(registry);
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
#if DEBUG
            // Ignore SSL certificate errors while debugging.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif
            Mvx.RegisterType<IMvxLanguageBindingParser, CustomLanguageBindingParser>();
        }
    }
}