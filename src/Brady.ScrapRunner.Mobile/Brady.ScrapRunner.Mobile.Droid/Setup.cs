namespace Brady.ScrapRunner.Mobile.Droid
{
    using Android.Content;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Core.Views;
    using MvvmCross.Droid.Platform;
    using MvvmCross.Droid.Views;
    using MvvmCross.Forms.Presenter.Droid;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Platform;

    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new MvxDebugTrace();
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var presenter = new MvxFormsDroidPagePresenter();
            Mvx.RegisterSingleton<IMvxViewPresenter>(presenter);
            return presenter;
        }
    }
}