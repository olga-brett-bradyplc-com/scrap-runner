using System;
using Android.Runtime;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.PhotosFragment")]
    public class PhotosFragment : BaseFragment<PhotosViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_photos;
        protected override bool NavMenuEnabled => true;
    }
}