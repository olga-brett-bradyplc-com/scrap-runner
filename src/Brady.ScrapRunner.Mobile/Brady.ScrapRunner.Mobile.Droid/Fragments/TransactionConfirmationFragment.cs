using System;
using System.ComponentModel;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Brady.ScrapRunner.Mobile.ViewModels;
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
            //signaturePad.BackgroundColor = new Color(ContextCompat.GetColor(Activity, Resource.Color.material_gray_300));
            signaturePad.StrokeColor = Color.Black;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = View.FindViewById<MvxListView>(Resource.Id.TransactionConfirmationListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;
        }
    }
}