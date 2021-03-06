using System;
using System.ComponentModel;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.ScaleSummaryFragment")]
    public class ScaleSummaryFragment : BaseFragment<ScaleSummaryViewModel>
    {
        private IDisposable _containersToken;

        protected override int FragmentId => Resource.Layout.fragment_scalesummary;
        protected override bool NavMenuEnabled => true;
        protected override int NavColor => Resource.Color.arrive;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var listGrouping = View.FindViewById<BindableGroupListView>(Resource.Id.ScaleSummaryListView);
            var groupLayout = Resource.Layout.item_scalesummary;
            listGrouping.Adapter = new BindableGroupListScaleAdapter(Activity, (MvxAndroidBindingContext)BindingContext, groupLayout);

            if (ViewModel.ContainersOnPowerId != null)
                listGrouping.ItemsSource = ViewModel.ContainersOnPowerId;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.ContainersOnPowerId, OnContainersChanged);
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
            if (ViewModel.ContainersOnPowerId != null)
                listGrouping.ItemsSource = ViewModel.ContainersOnPowerId;
        }

    }
}