namespace Brady.ScrapRunner.Mobile.BarcodeDemo.Droid
{
    using System;
    using System.Collections.Generic;
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
        ZXingScannerFragment _scanFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            _scanFragment = new ZXingScannerFragment();
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, _scanFragment)
                .Commit();
        }

        protected override void OnResume()
        {
            base.OnResume();

            Scan();
        }

        protected override void OnPause()
        {
            _scanFragment.StopScanning();

            base.OnPause();
        }

        private void Scan()
        {
            var opts = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat> {
                    ZXing.BarcodeFormat.All_1D
                },
                CameraResolutionSelector = availableResolutions => {

                    foreach (var ar in availableResolutions)
                    {
                        Console.WriteLine("Resolution: " + ar.Width + "x" + ar.Height);
                    }
                    return null;
                }
            };

            _scanFragment.StartScanning(result => {

                // Null result means scanning was cancelled
                if (string.IsNullOrEmpty(result?.Text))
                {
                    Toast.MakeText(this, "Scanning Cancelled", ToastLength.Long).Show();
                    return;
                }

                // Otherwise, proceed with result
                RunOnUiThread(() => Toast.MakeText(this, "Scanned: " + result.Text, ToastLength.Short).Show());
            }, opts);
        }
    }
}