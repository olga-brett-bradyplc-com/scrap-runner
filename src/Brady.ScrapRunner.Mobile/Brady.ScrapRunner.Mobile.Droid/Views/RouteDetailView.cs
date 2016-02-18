using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_routedetail);

            // Having a race-condition of sorts where information isn't always being loaded
            ViewModel.WeakSubscribe(() => ViewModel.Containers, (s, e) =>
            {
                LoadContainers(ViewModel.Containers);
            });

            // Track current status ( enroute, arrived ) and change elements as needed
            ViewModel.WeakSubscribe(() => ViewModel.CurrentStatus, (s, e) =>
            {
                var layout = FindViewById<TextView>(Resource.Id.TripCompanyName);
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

                var directionsButton = FindViewById<Button>(Resource.Id.DirectionsButton);
                var enrouteButton = FindViewById<Button>(Resource.Id.EnrouteButton);
                var arriveButton = FindViewById<Button>(Resource.Id.ArriveButton);

                // @TODO: Add animations, etc., during UI polish phase
                switch (ViewModel.CurrentStatus)
                {
                    case "EN":
                        layout.SetBackgroundColor(Color.ParseColor("#43b517"));
                        toolbar.SetBackgroundColor(Color.ParseColor("#43b517"));
                        enrouteButton.Visibility = ViewStates.Invisible;
                        arriveButton.Visibility = ViewStates.Visible;
                        break;
                    case "AR":
                        layout.SetBackgroundColor(Color.ParseColor("#b51717"));
                        toolbar.SetBackgroundColor(Color.ParseColor("#b51717"));
                        arriveButton.Visibility = ViewStates.Invisible;
                        directionsButton.SetX(directionsButton.GetX() + 135);
                        break;
                }
            });
        }

        private void LoadContainers(ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> data)
        {
            foreach (var element in data)
            {
                var mainLayout = FindViewById<LinearLayout>(Resource.Id.content_layout);
                LayoutInflater inflatorService = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                var tempLayout = inflatorService.Inflate(Resource.Layout.item_tripsegment, mainLayout) as LinearLayout;
                var tempTitle = tempLayout.FindViewById<TextView>(Resource.Id.cardViewTitle);
                tempTitle.Text = element.Key.TripSegTypeDesc;
                tempTitle.Id = 1; // Kind of a hacky way to do this.
            }
        }
    }
}