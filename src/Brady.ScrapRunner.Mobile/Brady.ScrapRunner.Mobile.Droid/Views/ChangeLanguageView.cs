using Android.Content.PM;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.App;
    using Android.OS;
    using ViewModels;

    [Activity(Label = "Change Language",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Locale)]
    public class ChangeLanguageView : BaseActivity<ChangeLanguageViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_changelanguage);
        }
    }
}