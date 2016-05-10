using Android.Runtime;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.RouteSummaryFragment")]
    public class RouteSummaryFragment : BaseFragment<RouteSummaryViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_routesummary;
        protected override bool NavMenuEnabled => true;
    }
}