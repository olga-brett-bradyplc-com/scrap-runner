namespace Brady.ScrapRunner.Mobile.BarcodeDemo.Droid
{
    using System.Threading.Tasks;
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Widget;
    using ZXing.Mobile;

    [Activity (
        Label = "ScrapRunner Barcode Demo",
        MainLauncher = true, 
        Icon = "@drawable/icon", 
        Theme = "@android:style/Theme.Holo.Light", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
	public class MainActivity : global::Android.Support.V4.App.FragmentActivity
    {
        private ZXingScannerFragment _scanFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            MobileBarcodeScanner.Initialize(Application);
            _scanFragment = new ZXingScannerFragment();
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, _scanFragment)
                .Commit();
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await Task.Delay(1000);
            Scan();
        }

        protected override void OnPause()
        {
            _scanFragment.StopScanning();

            base.OnPause();
        }

        private void VibrateDevice()
        {
            var vibrateService = (Vibrator)GetSystemService(VibratorService);
            if (vibrateService == null) return;
            if (vibrateService.HasVibrator)
                vibrateService.Vibrate(250);
        }

        private void Scan()
        {
            _scanFragment.StartScanning(result => 
            {
                if (string.IsNullOrEmpty(result?.Text))
                {
                    Toast.MakeText(this, "Scanning cancelled", ToastLength.Long).Show();
                    return;
                }
                VibrateDevice();
                RunOnUiThread(() => Toast.MakeText(this, "Scanned: " + result.Text, ToastLength.Short).Show());
            }, MobileBarcodeScanningOptions.Default);
        }
    }
}