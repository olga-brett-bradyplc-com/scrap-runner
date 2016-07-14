using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Channels;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.ViewModels;
using Java.IO;
using Java.Nio;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
using SignaturePad;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.TransactionConfirmationFragment")]
    public class TransactionConfirmationFragment : BaseFragment<TransactionConfirmationViewModel>
    {
        private IDisposable _containersToken;

        protected override int FragmentId => Resource.Layout.fragment_transactionconfirmation;
        protected override bool NavMenuEnabled => true;
        protected override int NavColor => Resource.Color.arrive;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionConfirmationListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);

            var signaturePad = View.FindViewById<SignaturePadView>(Resource.Id.SignatureView);
            signaturePad.BackgroundColor = Color.ParseColor("#f3f3f3");
            signaturePad.StrokeColor = Color.Black;

            var confirmButton = View.FindViewById<Button>(Resource.Id.confirm_transactions_button);

            // Manually set button handler so we can send the signature pad image to the viewmodel
            confirmButton.Click += (sender, args) =>
            {
                if (signaturePad.Points.Length > 0)
                {
                    /*
                        Due to limitations in the dispatch image viewer, we need to make sure that signature pad image is transposed
                        on top of an opaque background, otherwise the image is sent with a transparent background, which causes the 
                        image to appear as a solid, black image due to the dispatch's image viewer not supporting transparency
                    */
                    var signatureBitmap = signaturePad.GetImage();
                    var opaqueBitmap = Bitmap.CreateBitmap(signatureBitmap.Width, signatureBitmap.Height, signatureBitmap.GetConfig());
                    var canvas = new Canvas(opaqueBitmap);
                    canvas.DrawColor(Color.White);
                    canvas.DrawBitmap(signatureBitmap, 0, 0, null);
                    using (var ms = new MemoryStream())
                    {
                        opaqueBitmap.Compress(Bitmap.CompressFormat.Png, 80, ms);
                        ViewModel.ConfirmTransactionsCommand.ExecuteAsync(ms.ToArray());
                    }
                }
                else
                {
                    ViewModel.ConfirmTransactionsCommand.ExecuteAsync(null);
                }
            };
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_containersToken != null)
            {
                _containersToken.Dispose();
                _containersToken = null;
            }
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionConfirmationListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;
        }
    }
}