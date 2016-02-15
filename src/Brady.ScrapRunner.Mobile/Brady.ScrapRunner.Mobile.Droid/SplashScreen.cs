namespace Brady.ScrapRunner.Mobile.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using MvvmCross.Droid.Views;

    [Activity(
        Label = "ScrapRunner"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , NoHistory = true
        , ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.activity_splash)
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Acr.UserDialogs.UserDialogs.Init(this);
        }
    }
}