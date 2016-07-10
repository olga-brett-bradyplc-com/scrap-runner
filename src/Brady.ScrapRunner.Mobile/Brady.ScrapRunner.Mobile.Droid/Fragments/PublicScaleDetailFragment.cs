using System;
using System.ComponentModel;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;


namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.PublicScaleDetailFragment")]
    public class PublicScaleDetailFragment : BaseFragment<PublicScaleDetailViewModel>
    {
        private IDisposable _containersToken;
        private IDisposable _grossTimeToken;
        private IDisposable _tareTimeToken;
        private IDisposable _secondGrossTimeToken;
        protected override int FragmentId => Resource.Layout.fragment_publicscaledetail;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.PublicScaleDetailListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
            _grossTimeToken = ViewModel.WeakSubscribe(() => ViewModel.GrossTime, OnGrossTimeChanged);
            _tareTimeToken = ViewModel.WeakSubscribe(() => ViewModel.TareTime, OnTareTimeChanged);
            _secondGrossTimeToken = ViewModel.WeakSubscribe(() => ViewModel.SecondGrossTime, OnSecondGrossTimeChanged);
        }
        public override void OnDestroyView()
        {
            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;

            if (_grossTimeToken == null) return;
            _grossTimeToken.Dispose();
            _grossTimeToken = null;

            if (_tareTimeToken == null) return;
            _tareTimeToken.Dispose();
            _tareTimeToken = null;

            if (_secondGrossTimeToken == null) return;
            _secondGrossTimeToken.Dispose();
            _secondGrossTimeToken = null;

            base.OnDestroyView();
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.PublicScaleDetailListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;
        }

        private void OnGrossTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            var button = View.FindViewById<Button>(Resource.Id.grossButton);
            button.Text = "Gross : " + ViewModel.GrossTime;
        }

        private void OnTareTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            var button = View.FindViewById<Button>(Resource.Id.tareButton);
            button.Text = "Tare : " + ViewModel.GrossTime;
        }

        private void OnSecondGrossTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            var button = View.FindViewById<Button>(Resource.Id.secondGrossButton);
            button.Text = "Second Gross : " + ViewModel.GrossTime;
        }
    }
}