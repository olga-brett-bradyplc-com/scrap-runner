using System.ComponentModel;
using System.Resources;
using Android.Content.PM;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Binding.BindingContext;

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
        private Button _signInButton;
        private EditText _usernameTextEdit;
        private EditText _passwordTextEdit;

        public Button SignInButton
        {
            get
            {
                return _signInButton ??
                       (_signInButton = FindViewById<Button>(
                           Resource.Id.SignInButton));
            }
        }

        public EditText UsernameEditText
        {
            get
            {
                return _usernameTextEdit ??
                       (_usernameTextEdit = FindViewById<EditText>(
                           Resource.Id.UsernameTextEdit));
            }
        }

        public EditText PasswordEditText
        {
            get
            {
                return _passwordTextEdit ??
                       (_passwordTextEdit = FindViewById<EditText>(
                           Resource.Id.PasswordTextEdit));
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_signin);
            SignInButton.SetText(AppResources.SignIn, TextView.BufferType.Spannable);
            UsernameEditText.SetHint(Resource.String.username);
            PasswordEditText.SetHint(Resource.String.password);
        }
    }
}
