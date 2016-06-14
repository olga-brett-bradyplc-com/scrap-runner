using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Bindings;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
using ZXing.Mobile;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.TransactionSummaryFragment")]
    public class TransactionSummaryFragment : BaseFragment<TransactionSummaryViewModel>
    {
        private IDisposable _containersToken;
        private IDisposable _currentTransactionToken;
        private ZXingScannerFragment _scannerFragment;

        protected override int FragmentId => Resource.Layout.fragment_transactionsummary;
        protected override bool NavMenuEnabled => true;
        protected override int NavColor => Resource.Color.arrive;

        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            MobileBarcodeScanner.Initialize(Activity.Application);

            _scannerFragment = new ZXingScannerFragment();

            Activity.SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.camera_fragment, _scannerFragment)
                .Commit();

            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);

            if (ViewModel.Containers != null)
                listGrouping.Adapter.ItemsSource = ViewModel.Containers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);

            await Task.Delay(1000);
            Scan();

        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;

            if (_currentTransactionToken == null) return;
            _currentTransactionToken.Dispose();
            _currentTransactionToken = null;


            _scannerFragment.StopScanning();
        }

        public override async void OnResume()
        {
            base.OnResume();

            await Task.Delay(1000);
            Scan();
        }

        public override void OnPause()
        {
            _scannerFragment.StopScanning();
            base.OnPause();
        }

        private void VibrateDevice()
        {
            var vibrateService = (Vibrator) ((MainActivity) Activity)?.GetSystemService(Context.VibratorService);
            if (vibrateService == null) return;
            if (vibrateService.HasVibrator)
                vibrateService.Vibrate(300);
        }

        private void Scan()
        {
            _scannerFragment.StartScanning(result =>
            {
                if (string.IsNullOrEmpty(result?.Text))
                {
                    Toast.MakeText(Activity, "Could not read bar code", ToastLength.Long).Show();
                    return;
                }
                
                ViewModel.TransactionScannedCommandAsync.Execute(result.Text);
                var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
                //ViewModel.SelectNextTransactionCommand.Execute();

                VibrateDevice();

                Activity.RunOnUiThread(() =>
                {
                    // Update adapters item source
                    listGrouping.Adapter.ItemsSource = ViewModel.Containers;
                    listGrouping.InvalidateViews();
                    Toast.MakeText(Activity, "Scanned: " + result.Text, ToastLength.Short).Show();
                });

            }, MobileBarcodeScanningOptions.Default);
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
            if (ViewModel.Containers != null)
            {
                listGrouping.Adapter.ItemsSource = ViewModel.Containers;
            }
        }

    }
}