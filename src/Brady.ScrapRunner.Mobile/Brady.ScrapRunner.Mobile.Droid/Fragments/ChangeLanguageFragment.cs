using Android.OS;
using Android.Runtime;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.ChangeLanguageFragment")]
    public class ChangeLanguageFragment : BaseFragment<ChangeLanguageViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_changelanguage;
        // @TODO : Determine if logged in, then show menu, otherwise, hide?
        protected override bool NavMenuEnabled => false;
    }
}