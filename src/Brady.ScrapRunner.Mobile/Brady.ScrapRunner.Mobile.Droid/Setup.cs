using System.Collections.Generic;
using Android.Views;
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
    }
}
