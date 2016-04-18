using System;
using System.ComponentModel;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.ScaleSummaryFragment")]
    public class ScaleSummaryFragment : BaseFragment<ScaleSummaryViewModel>
    {
        private IDisposable _containersToken;

        protected override int FragmentId => Resource.Layout.fragment_scalesummary;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.ScaleSummaryListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.ScaleSummaryListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;
        }

    }
}