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
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.ModifyReturnToYardFragment")]
    public class ModifyReturnToYardFragment : BaseFragment<ModifyReturnToYardViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_modifyreturntoyard;

        protected override bool NavMenuEnabled => false;
    }
}