namespace Brady.ScrapRunner.Mobile.SignatureDemo.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Provider;
    using Android.Widget;
    using SignaturePad;

    [Activity(
        Label = "Signature Demo",
        MainLauncher = true,
        Icon = "@drawable/icon",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private SignaturePadView _signaturePadView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _signaturePadView = FindViewById<SignaturePadView>(Resource.Id.signatureView);

            var save = FindViewById<Button>(Resource.Id.save);
            if (save != null)
            {
                save.Click += (sender, args) =>
                {
                    SaveImageToGallery();
                };
            }
        }

        private void SaveImageToGallery()
        {
            using (var bitmap = _signaturePadView.GetImage())
            {
                var savedUri = MediaStore.Images.Media.InsertImage(ContentResolver, bitmap, "Signature", "Saved Signature");
            }
        }
    }

}


