using Android.App;
using Android.OS;
using Android.Runtime;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.TransactionDetailFragment")]
    public class TransactionDetailFragment : BaseFragment<TransactionDetailViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_transactiondetail;
        protected override bool NavMenuEnabled => true;
    }
}