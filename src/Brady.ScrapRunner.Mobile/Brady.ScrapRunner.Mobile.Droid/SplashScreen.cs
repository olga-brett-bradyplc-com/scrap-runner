namespace Brady.ScrapRunner.Mobile.Droid
{
    using Android.App;
    using Android.Content.PM;
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
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}