using Android.Content.PM;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.App;
    using Android.OS;
    using ViewModels;

    [Activity(Label = "Settings",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Locale)]
    public class SettingsView : BaseActivity<SettingsViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
        }
    }
}