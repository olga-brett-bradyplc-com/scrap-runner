using Android.Content.PM;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.App;
    using Android.OS;
    using ViewModels;

    [Activity(Label = "Settings")]
    public class SettingsView : BaseActivity<SettingsViewModel>
    {
        private Button _changeLanguageButton;

        public Button ChangeLanguageButton
        {
            get
            {
                return _changeLanguageButton ??
                 (_changeLanguageButton = FindViewById<Button>(
                  Resource.Id.ChangeLanguageButton));
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
            ChangeLanguageButton.SetText(AppResources.ChangeLanguage, TextView.BufferType.Spannable);
        }
    }
}