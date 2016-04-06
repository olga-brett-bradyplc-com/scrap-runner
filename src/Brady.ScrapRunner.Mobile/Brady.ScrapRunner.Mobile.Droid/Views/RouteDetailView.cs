using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.Binders;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Platform.WeakSubscription;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    [Activity(Label = "Route Detail")]
    public class RouteDetailView : BaseActivity<RouteDetailViewModel>
    {

        private IDisposable _containersToken;
        private IDisposable _currentStatusToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_routedetail);
            
            if(ViewModel.Containers != null)
                LoadContainers(ViewModel.Containers);

            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
            _currentStatusToken = ViewModel.WeakSubscribe(() => ViewModel.CurrentStatus, OnCurrentStatusChanged);
        }

        protected override void OnDestroy()
        {
            if (_containersToken != null)
            {
                _containersToken.Dispose();
                _containersToken = null;
            }

            if (_currentStatusToken != null)
            {
                _currentStatusToken.Dispose();
                _currentStatusToken = null;
            }

            base.OnDestroy();
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            LoadContainers(ViewModel.Containers);
        }

        private void OnCurrentStatusChanged(object sender, PropertyChangedEventArgs args)
        {
            var layout = FindViewById<TextView>(Resource.Id.TripCompanyName);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            var directionsButton = FindViewById<Button>(Resource.Id.DirectionsButton);
            var enrouteButton = FindViewById<Button>(Resource.Id.EnrouteButton);
            var arriveButton = FindViewById<Button>(Resource.Id.ArriveButton);
            var buttonLayout = FindViewById<LinearLayout>(Resource.Id.transactionButtonLayout);

            // @TODO: Add animations, etc., 
            switch (ViewModel.CurrentStatus)
            {
                case "EN":
                    layout.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.enroute)));
                    toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.enroute)));
                    enrouteButton.Visibility = ViewStates.Invisible;
                    arriveButton.Visibility = ViewStates.Visible;
                    break;
                case "AR":
                    layout.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.arrive)));
                    toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.arrive)));
                    arriveButton.Visibility = ViewStates.Invisible;
                    directionsButton.SetX(directionsButton.GetX() + 135);
                    buttonLayout.Visibility = ViewStates.Visible;
                    break;
            }
        }

        private void LoadContainers(ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> data)
        {
            foreach (var element in data)
            {
                var mainLayout = FindViewById<LinearLayout>(Resource.Id.content_layout);
                LayoutInflater inflatorService = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                var tripSegmentLayout = inflatorService.Inflate(Resource.Layout.item_tripsegment, mainLayout) as LinearLayout;

                var tempTitle = tripSegmentLayout.FindViewById<TextView>(Resource.Id.cardViewTitle);
                tempTitle.Text = element.Key.TripSegTypeDesc;
                tempTitle.Id = 1; // Kind of a hacky way to do this.

                // We'd use a listview for this usually, but since we're mocking our collapsable lists ( not implemented in prototype ),
                // only show the first TripSegmentContainer for each TripSegment
                var firstTripSegmentContainer = element.First();

                var tempType = tripSegmentLayout.FindViewById<TextView>(Resource.Id.TripSegmentContainerTypeText);
                tempType.Text = firstTripSegmentContainer.DefaultTripSegContainerNumber +
                    " " + firstTripSegmentContainer.TripSegContainerType +
                    "-" + firstTripSegmentContainer.TripSegContainerSize;
                tempType.Id = 2;

                var tempCommodity = tripSegmentLayout.FindViewById<TextView>(Resource.Id.TripSegmentContainerCommodityDescText);
                tempCommodity.Text = firstTripSegmentContainer.TripSegContainerCommodityDesc;
                tempCommodity.Id = 3;

                var tempLocation = tripSegmentLayout.FindViewById<TextView>(Resource.Id.TripSegmentContianerLocationText);
                tempLocation.Text = firstTripSegmentContainer.TripSegContainerLocation;
                tempLocation.Id = 4;
            }
        }
    }
}