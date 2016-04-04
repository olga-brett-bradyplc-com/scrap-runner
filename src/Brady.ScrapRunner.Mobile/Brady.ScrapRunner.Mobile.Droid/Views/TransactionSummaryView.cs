using System.Linq;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Core.Platform;
using ZXing.Mobile;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using System;
    using System.ComponentModel;
    using Android.App;
    using Android.OS;
    using ViewModels;
    using MvvmCross.Binding.Droid.Views;
    using MvvmCross.Platform.WeakSubscription;

    [Activity(Label = "Transaction Summary View")]
    public class TransactionSummaryView : BaseActivity<TransactionSummaryViewModel>
    {
        private IDisposable _containersToken;
        private IDisposable _currentTransactionToken;
        private ZXingScannerFragment _scannerFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_transactionsummary);
            MobileBarcodeScanner.Initialize(Application);

            _scannerFragment = new ZXingScannerFragment();

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.camera_fragment, _scannerFragment)
                .Commit();

            var listGrouping = FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);

            if (ViewModel.Containers != null)
            {
                listGrouping.ItemsSource = ViewModel.Containers;
            }

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
        }

        public override void OnDetachedFromWindow()
        {
            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;

            if (_currentTransactionToken == null) return;
            _currentTransactionToken.Dispose();
            _currentTransactionToken = null;
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await Task.Delay(1000);
            
            Scan();
        }

        protected override void OnPause()
        {
            _scannerFragment.StopScanning();
            base.OnPause();
        }

        private void VibrateDevice()
        {
            var vibrateService = (Vibrator) GetSystemService(VibratorService);
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
                    Toast.MakeText(this, "Could not read bar code", ToastLength.Long).Show();
                    return;
                }

                var currentActionDateTime = ViewModel.CurrentTransaction.TripSegContainerActionDateTime;

                ViewModel.TransactionScannedCommand.Execute(result.Text);
                _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
                VibrateDevice();

                // Assume transaction did not complete
                if (ViewModel.CurrentTransaction.TripSegContainerActionDateTime == currentActionDateTime)
                {
                    // @TODO : Implement dialog with appropiate messaging
                    Log.Error("scraprunner", "Could not scan label");
                }
                // Assume the transaction was successfully entered
                else
                {
                    RunOnUiThread(() =>
                    {
                        var listGrouping = FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
                        var listItem = listGrouping.FindViewById<TextView>(Resource.Id.tripContainerInfo);
                        listItem.SetText(listItem.Text.Replace("<NO NUMBER>", result.Text), TextView.BufferType.Normal);

                        var listImage = listGrouping.FindViewById<ImageView>(Resource.Id.arrow_image);
                        listImage.SetImageResource(Resource.Drawable.ic_check_circle_green_36dp);

                        Toast.MakeText(this, "Scanned: " + result.Text, ToastLength.Short).Show();
                    });
                }

            }, MobileBarcodeScanningOptions.Default);
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
            if (ViewModel.Containers != null)
            {
                listGrouping.ItemsSource = ViewModel.Containers;
            }
        }
    }
}