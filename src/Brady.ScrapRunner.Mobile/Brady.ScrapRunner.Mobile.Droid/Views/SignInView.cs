namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.App;
    using Android.OS;
    using ViewModels;

    [Activity(
        Name = "com.bradyplc.scraprunner.SignInView",
        Label = "Sign In",
        Theme = "@style/ScrapRunnerTheme.SignIn")]
    public class SignInView : BaseActivity<SignInViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_signin);
        }
    }
}
