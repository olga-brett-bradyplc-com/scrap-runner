using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
using ZXing.Mobile;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.LoadDropContainerFragment")]
    public class LoadDropContainerFragment : BaseFragment<LoadDropContainerViewModel>, View.IOnTouchListener, GestureDetector.IOnGestureListener
    {
        private ZXingScannerFragment _scannerFragment;
        private GestureDetector _gestureDetector;
        private MvxListView _listView;
        private IDisposable _containersToken;

        private const int SwipeMinDistance = 120;
        private const int SwipeMaxOffPath = 250;
        private const int SwipeThresholdVelocity = 200;

        protected override int FragmentId => Resource.Layout.fragment_loaddropcontainer;
        protected override bool NavMenuEnabled => false;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _listView = View.FindViewById<MvxListView>(Resource.Id.CurrentContainerList);

            if (ViewModel.CurrentContainers != null)
                _listView.ItemsSource = ViewModel.CurrentContainers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.CurrentContainers, OnContainersChanged);

            _gestureDetector = new GestureDetector(this);
            _listView.SetOnTouchListener(this);
            HasOptionsMenu = true;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.loaddropcontainer_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.add_new_container_loaddrop_nav:
                    ViewModel.AddContainerCommand.ExecuteAsync();
                    return true;
                case Resource.Id.load_scanner_nav:
                    LoadScanner();
                    return true;
                case Resource.Id.developer_nb_scan_nav:
                    ViewModel.ScanContainerCommand.ExecuteAsync("89997");
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.ScaleSummaryListView);
            if (ViewModel.CurrentContainers != null)
                _listView.ItemsSource = ViewModel.CurrentContainers;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_containersToken != null)
            {
                _containersToken.Dispose();
                _containersToken = null;
            }

            _scannerFragment?.StopScanning();
        }

        private async void LoadScanner()
        {
            var scannerLayout = View.FindViewById<FrameLayout>(Resource.Id.camera_fragment_loaddrop);
            scannerLayout.Visibility = ViewStates.Visible;

            MobileBarcodeScanner.Initialize(Activity.Application);
            _scannerFragment = new ZXingScannerFragment();

            Activity.SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.camera_fragment_loaddrop, _scannerFragment)
                .Commit();

            await Task.Delay(1000);
            Scan();
        }

        private void VibrateDevice()
        {
            var vibrateService = (Vibrator)((MainActivity)Activity)?.GetSystemService(Context.VibratorService);
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

                VibrateDevice();
                ViewModel.ScanContainerCommand.ExecuteAsync(result.Text);
                Toast.MakeText(Activity, "Scanned: " + result.Text, ToastLength.Short).Show();

            }, MobileBarcodeScanningOptions.Default);
        }

        public bool OnDown(MotionEvent e)
        {
            return false;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            var pos = _listView.PointToPosition((int) e1.GetX(), (int) e1.GetY());
            var temp = _listView.GetItemAtPosition(pos);
            var child = _listView.GetChildAt(pos);
            var button = child?.FindViewById<RelativeLayout>(Resource.Id.ContainerItemButtons);

            try
            {
                if (Math.Abs(e1.GetY() - e2.GetY()) > SwipeMaxOffPath)
                    return false;

                if (e1.GetX() - e2.GetX() > SwipeMinDistance && Math.Abs(velocityX) > SwipeThresholdVelocity)
                {
                    button.Visibility = ViewStates.Visible;
                }
                else if (e2.GetX() - e1.GetX() > SwipeMinDistance && Math.Abs(velocityX) > SwipeThresholdVelocity)
                {
                    button.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                //
            }
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);
            return false;
        }
    }
}