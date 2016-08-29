using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.ViewModels;

namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    [Activity(
        Label = "Sign In",
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.SignInActivity")]
    public class SignInActivity : BaseActivity<SignInViewModel>
    {
        protected override int ActivityId => Resource.Layout.activity_signin;

        protected override void OnResume()
        {
            base.OnResume();

            // Make sure MainActivity has been cleared
            if (MainActivity.GetInstance() == null) return;

            MainActivity.GetInstance().Finish();
            MainActivity.ResetInstance();
        }
    }
}