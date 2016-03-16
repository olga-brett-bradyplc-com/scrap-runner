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
    [Activity(Label = "ScaleDetailView")]
    public class ScaleDetailView : BaseActivity<ScaleDetailViewModel>
    {
        private IDisposable _containersToken;
        private IDisposable _grossTimeToken;
        private IDisposable _tareTimeToken;
        private IDisposable _secondGrossTimeToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_scaledetail);

            var listGrouping = FindViewById<MvxListView>(Resource.Id.ScaleDetailListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
            _grossTimeToken = ViewModel.WeakSubscribe(() => ViewModel.GrossTime, OnGrossTimeChanged);
            _tareTimeToken = ViewModel.WeakSubscribe(() => ViewModel.TareTime, OnTareTimeChanged);
            _secondGrossTimeToken = ViewModel.WeakSubscribe(() => ViewModel.SecondGrossTime, OnSecondGrossTimeChanged);
        }

        public override void OnDetachedFromWindow()
        {
            if (_containersToken == null) return;
            _containersToken.Dispose();
            _containersToken = null;

            if (_grossTimeToken == null) return;
            _grossTimeToken.Dispose();
            _grossTimeToken = null;

            if (_tareTimeToken == null) return;
            _tareTimeToken.Dispose();
            _tareTimeToken = null;

            if (_secondGrossTimeToken == null) return;
            _secondGrossTimeToken.Dispose();
            _secondGrossTimeToken = null;
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            var listGrouping = FindViewById<MvxListView>(Resource.Id.ScaleDetailListView);
            if (ViewModel.Containers != null)
                listGrouping.ItemsSource = ViewModel.Containers;
        }

        private void OnGrossTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            var button = FindViewById<Button>(Resource.Id.grossButton);
            button.Text = "Gross : " + ViewModel.GrossTime;
        }

        private void OnTareTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            var button = FindViewById<Button>(Resource.Id.grossButton);
            button.Text = "Tare : " + ViewModel.GrossTime;
        }

        private void OnSecondGrossTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            var button = FindViewById<Button>(Resource.Id.grossButton);
            button.Text = "Second Gross : " + ViewModel.GrossTime;
        }
    }
}