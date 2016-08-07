using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.Droid.Controls;
using Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView;
using Brady.ScrapRunner.Mobile.Droid.Messages;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform;
using MvvmCross.Platform.WeakSubscription;
using MvvmCross.Plugins.Messenger;
using Object = Java.Lang.Object;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.RouteDetailFragment")]
    public class RouteDetailFragment : BaseFragment<RouteDetailViewModel>
    {
        private IDisposable _currentStatusToken;
        private IDisposable _allowRtnEditToken;
        private IDisposable _tripLegToken;
        private IDisposable _readOnlyToken;

        private IMvxMessenger _mvxMessenger;
        private IDriverService _driverService;

        private string _currentStatus;
        private MvxExpandableExListView _listview;
        private ViewPager _pager;

        protected override int FragmentId => Resource.Layout.fragment_routedetail;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _mvxMessenger = Mvx.Resolve<IMvxMessenger>();
            _driverService = Mvx.Resolve<IDriverService>();

            if (ViewModel.AllowRtnEdit.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnEdit.Value;

            if (_currentStatus != null || ViewModel.CurrentStatus != null)
                OnCurrentStatusChanged(this, null);
            
            _pager = View.FindViewById<ViewPager>(Resource.Id.TripViewPager);

            if (ViewModel.TripLegs != null)
                OnTripLegChanged(this, null);

            var directionsButton = View.FindViewById<Button>(Resource.Id.DirectionsButton);
            directionsButton.Click += delegate
            {
                _pager.CurrentItem = _pager.Adapter.Count + 2;
            };

            var task = CheckMenuState();

            _readOnlyToken = ViewModel.WeakSubscribe(() => ViewModel.ReadOnlyTrip, OnReadOnlyTripChanged);
            _tripLegToken = ViewModel.WeakSubscribe(() => ViewModel.TripLegs, OnTripLegChanged);
            _allowRtnEditToken = ViewModel.WeakSubscribe(() => ViewModel.AllowRtnEdit, OnAllowRtnEditChanged);
            _currentStatusToken = ViewModel.WeakSubscribe(() => ViewModel.CurrentStatus, OnCurrentStatusChanged);
        }

        private async Task CheckMenuState()
        {
            var driverStatus = await _driverService.GetCurrentDriverStatusAsync();

            // If we're resuming a trip, set the menuing as "OnTrip"
            // TODO: Do we need to make this smarter? E.g., if they're on a trip, but previewing another trip should we enable/disable menu options?
            if (driverStatus.Status == "E" || driverStatus.Status == "A" || driverStatus.Status == "D")
                _mvxMessenger.Publish(new MenuStateMessage(this) { Context = MenuState.OnTrip });
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.routedetail_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var baseActivity = ((MainActivity) Activity);
            switch (item.ItemId)
            {
                case Resource.Id.edit_rtn_nav:
                    baseActivity.CloseOptionsMenu();
                    ViewModel.AddReturnToYardCommand.Execute();
                    return true;
                default:
                    baseActivity.CloseOptionsMenu();
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

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

            if (_tripLegToken != null)
            {
                _tripLegToken.Dispose();
                _tripLegToken = null;
            }

            if (_readOnlyToken != null)
            {
                _readOnlyToken.Dispose();
                _readOnlyToken = null;
            }
        }

        private void OnAllowRtnEditChanged(object sender, PropertyChangedEventArgs args)
        {
            if (ViewModel.AllowRtnEdit.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnEdit.Value;
        }

        private void OnTripLegChanged(object sender, PropertyChangedEventArgs args)
        {
            if (ViewModel.TripLegs != null)
            {
                var pagerAdapter = new TripPagerAdapter(Activity, BindingContext, ViewModel.TripLegs, ViewModel.CustomerDirections);
                _pager.Adapter = pagerAdapter;
            }

            if (ViewModel.CustomerDirections != null)
            {
                var directionsButton = View.FindViewById<Button>(Resource.Id.DirectionsButton);
                directionsButton.Visibility = ViewStates.Visible;
            }
        }

        private void OnReadOnlyTripChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!ViewModel.ReadOnlyTrip) return;

            // Hide all buttons and show warning message
            var enrouteButton = View.FindViewById<Button>(Resource.Id.EnrouteButton);
            var arriveButton = View.FindViewById<Button>(Resource.Id.ArriveButton);

            enrouteButton.Visibility = ViewStates.Gone;
            arriveButton.Visibility = ViewStates.Gone;
        }

        private void OnCurrentStatusChanged(object sender, PropertyChangedEventArgs args)
        {
            _currentStatus = ViewModel.CurrentStatus;

            var layout = View.FindViewById<TextView>(Resource.Id.TripCompanyName);
            var toolbar = View.FindViewById<Toolbar>(Resource.Id.toolbar);

            var directionsButton = View.FindViewById<Button>(Resource.Id.DirectionsButton);
            var enrouteButton = View.FindViewById<Button>(Resource.Id.EnrouteButton);
            var arriveButton = View.FindViewById<Button>(Resource.Id.ArriveButton);
            var buttonLayout = View.FindViewById<LinearLayout>(Resource.Id.transactionButtonLayout);

            // @TODO: Add animations, etc., 
            switch (_currentStatus)
            {
                case "EN":
                case "E":
                    layout.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.enroute)));
                    toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.enroute)));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.enroute)));
                    }

                    //var move = new TranslateAnimation(Dimension.RelativeToSelf, 0, Dimension.RelativeToSelf, 500,
                    //    Dimension.RelativeToSelf, 0, Dimension.RelativeToSelf, 300)
                    //{
                    //    Interpolator = new LinearInterpolator(),
                    //    Duration = 1000
                    //};
                    //enrouteButton.StartAnimation(move);

                    enrouteButton.Visibility = ViewStates.Invisible;
                    arriveButton.Visibility = ViewStates.Visible;

                    _mvxMessenger.Publish(new MenuStateMessage(this) { Context = MenuState.OnTrip });

                    break;
                case "AR":
                case "A":
                    layout.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.arrive)));
                    toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.arrive)));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.arrive)));
                    }

                    enrouteButton.Visibility = ViewStates.Invisible;
                    arriveButton.Visibility = ViewStates.Invisible;
                    directionsButton.SetX(directionsButton.GetX() + 135);
                    buttonLayout.Visibility = ViewStates.Visible;
                    break;
            }
        }
    }
}