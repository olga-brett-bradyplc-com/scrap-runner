using Android.Content.PM;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.App;
    using Android.OS;
    using ViewModels;

    [Activity(Label = "Power Unit", Theme = "@style/ScrapRunnerTheme.SignIn")]
    public class PowerUnitView : BaseActivity<PowerUnitViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_powerunit);
        }
    }
}