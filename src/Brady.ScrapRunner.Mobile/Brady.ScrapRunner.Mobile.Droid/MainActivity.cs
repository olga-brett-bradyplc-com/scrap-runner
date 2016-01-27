namespace Brady.ScrapRunner.Mobile.Droid
{
    using Acr.UserDialogs;
    using Android.App;
    using Android.Content.PM;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Views;
    [Activity(
        Label = "ScrapRunner", 
        Icon = "@drawable/icon", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            UserDialogs.Init(this);

            Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());

            // We want the system tray to be the same color as our UI's primary color
            // for the given state ( in route, stopped, arrived, etc. )
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            if((int)Android.OS.Build.VERSION.SdkInt >= 21)
            {
                ActionBar.SetIcon(
                    new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent)));
            }
        }
    }
}

