using System;
using System.ComponentModel;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
using MvvmCross.Droid.Support.V7.AppCompat.Widget;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof (MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.TransactionDetailFragment")]
    public class TransactionDetailFragment : BaseFragment<TransactionDetailViewModel>
    {
        private IDisposable _commoditySelectionEnabledToken;

        protected override int FragmentId => Resource.Layout.fragment_transactiondetail;
        protected override bool NavMenuEnabled => true;
        protected override int NavColor => Resource.Color.arrive;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var commoditySpinner = View.FindViewById<MvxAppCompatSpinner>(Resource.Id.customer_commodity_spinner);
            commoditySpinner.Enabled = ViewModel.CommoditySelectionEnabled;
            commoditySpinner.Clickable = ViewModel.CommoditySelectionEnabled;

            _commoditySelectionEnabledToken = ViewModel.WeakSubscribe(() => ViewModel.CommoditySelectionEnabled,
                OnCommoditySelectionEnabledChanged);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_commoditySelectionEnabledToken != null)
            {
                _commoditySelectionEnabledToken.Dispose();
                _commoditySelectionEnabledToken = null;
            }
        }

        private void OnCommoditySelectionEnabledChanged(object sender, PropertyChangedEventArgs args)
        {
            var commoditySpinner = View.FindViewById<MvxAppCompatSpinner>(Resource.Id.customer_commodity_spinner);
            commoditySpinner.Enabled = ViewModel.CommoditySelectionEnabled;
            commoditySpinner.Clickable = ViewModel.CommoditySelectionEnabled;
        }
    }
}