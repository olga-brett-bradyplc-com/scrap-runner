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
                listGrouping.ItemsSource = ViewModel.Containers;

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
                
                ViewModel.TransactionScannedCommand.Execute(result.Text);

                VibrateDevice();

                Activity.RunOnUiThread(() =>
                {
                    var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
                    var ct = ViewModel.CurrentTransaction;
                    var okey = ViewModel.CurrentTransaction.TripNumber + ViewModel.CurrentTransaction.TripSegNumber +
                               ViewModel.CurrentTransaction.TripSegContainerSeqNumber;

                    for (var i = 0; i < listGrouping.Count; i++)
                    {
                        var currentListItem = listGrouping.GetChildAt(i);
                        var tripContainerKey = currentListItem.FindViewById<TextView>(Resource.Id.tripContainerKey)?.Text ?? "";

                        if (tripContainerKey == okey)
                        {
                            var listItem = currentListItem.FindViewById<TextView>(Resource.Id.tripContainerInfo);
                            listItem.SetText(listItem.Text.Replace("<NO NUMBER>", result.Text), TextView.BufferType.Normal);

                            var listImage = currentListItem.FindViewById<ImageView>(Resource.Id.arrow_image);
                            listImage.SetImageResource(Resource.Drawable.ic_check_circle_green_36dp);

                            ViewModel.SelectNextTransactionCommand.Execute();

                            break;
                        }
                    }

                    Toast.MakeText(Activity, "Scanned: " + result.Text, ToastLength.Short).Show();
                });

            }, MobileBarcodeScanningOptions.Default);
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
            if (ViewModel.Containers != null)
            {
                listGrouping.ItemsSource = ViewModel.Containers;
            }
        }

    }
}