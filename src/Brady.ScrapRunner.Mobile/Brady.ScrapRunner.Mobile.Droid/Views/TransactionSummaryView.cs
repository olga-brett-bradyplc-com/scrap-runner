using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Platform.WeakSubscription;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    [Activity(Label = "Transaction Summary View")]
    public class TransactionSummaryView : BaseActivity<TransactionSummaryViewModel>
    {
        private IDisposable _containersToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_transactionsummary);

            var listGrouping = FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
        }

        public override void OnDetachedFromWindow()
        {
            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = FindViewById<MvxListView>(Resource.Id.TransactionSummaryListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;
        }
    }
}