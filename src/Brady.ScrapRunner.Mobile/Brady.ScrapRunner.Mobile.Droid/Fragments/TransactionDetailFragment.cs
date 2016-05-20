using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof (MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.TransactionDetailFragment")]
    public class TransactionDetailFragment : BaseFragment<TransactionDetailViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_transactiondetail;
        protected override bool NavMenuEnabled => true;
        protected override int NavColor => Resource.Color.arrive;
    }
}