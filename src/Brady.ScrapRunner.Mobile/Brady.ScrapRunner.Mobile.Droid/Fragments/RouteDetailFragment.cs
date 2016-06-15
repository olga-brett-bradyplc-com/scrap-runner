using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.RouteDetailFragment")]
    public class RouteDetailFragment : BaseFragment<RouteDetailViewModel>
    {
        private IDisposable _containersToken;
        private IDisposable _currentStatusToken;
        private IDisposable _allowRtnEditToken;

        protected override int FragmentId => Resource.Layout.fragment_routedetail;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // Right now, we only have one context menu option avaliable for this view
            // We'll have to change this if we add more options in the future
            if(ViewModel.AllowRtnEdit.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnEdit.Value; 

            if (ViewModel.Containers != null)
                LoadContainers(ViewModel.Containers);

            _allowRtnEditToken = ViewModel.WeakSubscribe(() => ViewModel.AllowRtnEdit, OnAllowRtnEditChanged);
            _containersToken = ViewModel.WeakSubscribe(() => ViewModel.Containers, OnContainersChanged);
            _currentStatusToken = ViewModel.WeakSubscribe(() => ViewModel.CurrentStatus, OnCurrentStatusChanged);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.routedetail_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.edit_rtn_nav:
                    ViewModel.AddReturnToYardCommand.Execute();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

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

            if (_allowRtnEditToken != null)
            {
                _allowRtnEditToken.Dispose();
                _allowRtnEditToken = null;
            }
        }

        private void OnAllowRtnEditChanged(object sender, PropertyChangedEventArgs args)
        {
            if (ViewModel.AllowRtnEdit.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnEdit.Value;
        }

        private void OnContainersChanged(object sender, PropertyChangedEventArgs args)
        {
            LoadContainers(ViewModel.Containers);
        }

        private void OnCurrentStatusChanged(object sender, PropertyChangedEventArgs args)
        {
            var layout = View.FindViewById<TextView>(Resource.Id.TripCompanyName);
            var toolbar = View.FindViewById<Toolbar>(Resource.Id.toolbar);

            var directionsButton = View.FindViewById<Button>(Resource.Id.DirectionsButton);
            var enrouteButton = View.FindViewById<Button>(Resource.Id.EnrouteButton);
            var arriveButton = View.FindViewById<Button>(Resource.Id.ArriveButton);
            var buttonLayout = View.FindViewById<LinearLayout>(Resource.Id.transactionButtonLayout);

            // @TODO: Add animations, etc., 
            switch (ViewModel.CurrentStatus)
            {
                case "EN":
                    layout.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.enroute)));
                    toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.enroute)));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.enroute)));
                    }

                    enrouteButton.Visibility = ViewStates.Invisible;
                    arriveButton.Visibility = ViewStates.Visible;
                    break;
                case "AR":
                    layout.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.arrive)));
                    toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.arrive)));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.arrive)));
                    }

                    arriveButton.Visibility = ViewStates.Invisible;
                    directionsButton.SetX(directionsButton.GetX() + 135);
                    buttonLayout.Visibility = ViewStates.Visible;
                    break;
            }
        }

        private void LoadContainers(ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> data)
        {
            var inflatorService = (LayoutInflater)((MainActivity)Activity)?.GetSystemService(Context.LayoutInflaterService);
            foreach (var element in data)
            {
                var mainLayout = View.FindViewById<LinearLayout>(Resource.Id.content_layout);
                var tripSegmentLayout = inflatorService?.Inflate(Resource.Layout.item_tripsegment, mainLayout) as LinearLayout;

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