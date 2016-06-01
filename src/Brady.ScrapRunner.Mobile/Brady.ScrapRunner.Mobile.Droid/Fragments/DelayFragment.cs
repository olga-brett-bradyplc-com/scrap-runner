using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.DelayFragment")]
    public class DelayFragment : BaseFragment<DelayViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_delay;
        protected override bool NavMenuEnabled => false;
        protected override int NavColor => Resource.Color.delayed;
    }
}