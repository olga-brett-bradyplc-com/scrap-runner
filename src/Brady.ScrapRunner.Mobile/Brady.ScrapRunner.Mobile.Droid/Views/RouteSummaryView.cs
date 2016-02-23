using Android.Content.PM;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.App;
    using Android.OS;
    using ViewModels;

    [Activity(Label = "Route Summary",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Locale)]
    public class RouteSummaryView : BaseActivity<RouteSummaryViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_routesummary);
        }
    }
}