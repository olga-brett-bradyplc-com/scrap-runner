namespace Brady.ScrapRunner.Mobile.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Views;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Core.Views;
    using MvvmCross.Forms.Presenter.Core;
    using MvvmCross.Forms.Presenter.Droid;
    using MvvmCross.Platform;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.Android;

    [Activity(Label = "MvxFormsApplicationActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MvxFormsApplicationActivity
        : FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // We want the system tray to be the same color as our UI's primary color
            // for the given state ( in route, stopped, arrived, etc. )
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            if ((int)Android.OS.Build.VERSION.SdkInt >= 21)
            {
                ActionBar.SetIcon(
                    new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent)));
            }

            Forms.Init(this, bundle);
            var mvxFormsApp = new MvxFormsApp();
            LoadApplication(mvxFormsApp);

            var presenter = Mvx.Resolve<IMvxViewPresenter>() as MvxFormsDroidPagePresenter;
            if (presenter != null) presenter.MvxFormsApp = mvxFormsApp;

            Mvx.Resolve<IMvxAppStart>().Start();
        }
    }
}