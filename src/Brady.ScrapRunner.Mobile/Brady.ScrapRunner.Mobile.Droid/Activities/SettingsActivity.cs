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
        Label = "Settings",
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.SettingsActivity")]
    public class SettingsActivity : BaseActivity<SettingsViewModel>
    {
        protected override int ActivityId => Resource.Layout.fragment_settings;
    }
}