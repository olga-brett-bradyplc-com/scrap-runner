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
    }
}
