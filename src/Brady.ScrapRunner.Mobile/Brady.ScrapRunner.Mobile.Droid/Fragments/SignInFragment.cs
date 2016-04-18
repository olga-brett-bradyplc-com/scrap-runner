using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.SignInFragment")]
    public class SignInFragment : BaseFragment<SignInViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_signin;
        protected override bool NavMenuEnabled => false;
    }
}
