using System;
using System.ComponentModel;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.GpsCaptureFragment")]
    public class GpsCaptureFragment : BaseFragment<GpsCaptureViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_gpscapture;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
        }
    }
}