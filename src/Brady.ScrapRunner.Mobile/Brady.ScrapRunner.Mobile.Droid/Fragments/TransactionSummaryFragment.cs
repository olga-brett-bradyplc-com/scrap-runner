using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Acr.UserDialogs;
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
using Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
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
        private const int AddReturnToYardNav = Menu.First;
        private const int SimulateScanNav = AddReturnToYardNav + 1;

        private IDisposable _containersToken;
        private IDisposable _currentTransactionToken;
        private IDisposable _allowRtnAddToken;
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

            HasOptionsMenu = true;

            if (ViewModel.Containers != null)
                listGrouping.Adapter.ItemsSource = ViewModel.Containers;

            //_allowRtnAddToken = ViewModel.WeakSubscribe(() => ViewModel.AllowRtnAdd, OnAllowRtnAddChanged);
            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var ignore = OnOptionsItemSelectedAsync(item);
            return true;
        }

        private async Task OnOptionsItemSelectedAsync(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case AddReturnToYardNav:
                    ViewModel.AddRtnYardCommand.Execute();
                    break;
                case SimulateScanNav:
                    var listview = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
                    var number = await UserDialogs.Instance.PromptAsync("Enter barcode number", "Simulate", "OK");
                    await ViewModel.TransactionScannedCommandAsync.ExecuteAsync(number.Text);
                    Activity.RunOnUiThread(() => { ((BaseAdapter) listview.Adapter).NotifyDataSetChanged(); });
                    VibrateDevice();
                    break;
            }
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();
            if (ViewModel.AllowRtnAdd.HasValue && (bool) ViewModel.AllowRtnAdd)
                menu.Add(0, AddReturnToYardNav, Menu.None, AppResources.ReturnToYardAdd).SetShowAsAction(ShowAsAction.Never);

#if DEBUG
    menu.Add(0, SimulateScanNav, Menu.None, "Simulate Scan (DEV)").SetShowAsAction(ShowAsAction.Never);
#endif
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

            _scannerFragment?.StopScanning();
        }

        public override async void OnResume()
        {
            base.OnResume();

            await Task.Delay(3000);
            Scan();
        }

        public override void OnPause()
        {
            _scannerFragment?.StopScanning();
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
            var barcodeScanningOptions = new MobileBarcodeScanningOptions
            {
                DelayBetweenContinuousScans = 3000
            };
            
            _scannerFragment.StartScanning(async result =>
            {
                VibrateDevice();
                var listview = View.FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
                await ViewModel.TransactionScannedCommandAsync.ExecuteAsync(result?.Text);
                Activity.RunOnUiThread(() => { ((BaseAdapter)listview.Adapter).NotifyDataSetChanged(); });
            }, barcodeScanningOptions);
        }

        private void OnAllowRtnAddChanged(object sender, PropertyChangedEventArgs args)
        {
            if (ViewModel.AllowRtnAdd.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnAdd.Value;
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