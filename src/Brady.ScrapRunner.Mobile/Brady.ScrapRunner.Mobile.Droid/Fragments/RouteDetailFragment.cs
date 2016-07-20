using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
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
        private string _currentStatus;
        private MvxExpandableExListView _listview;

        protected override int FragmentId => Resource.Layout.fragment_routedetail;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if(ViewModel.AllowRtnEdit.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnEdit.Value;

            _listview = View.FindViewById<MvxExpandableExListView>(Resource.Id.TempExpandListView);

            if (_currentStatus != null)
                OnCurrentStatusChanged(this, null);

            _allowRtnEditToken = ViewModel.WeakSubscribe(() => ViewModel.AllowRtnEdit, OnAllowRtnEditChanged);
            _currentStatusToken = ViewModel.WeakSubscribe(() => ViewModel.CurrentStatus, OnCurrentStatusChanged);
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
        }

        private void OnAllowRtnEditChanged(object sender, PropertyChangedEventArgs args)
        {
            if (ViewModel.AllowRtnEdit.HasValue)
                HasOptionsMenu = ViewModel.AllowRtnEdit.Value;
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

                    enrouteButton.Visibility = ViewStates.Invisible;
                    arriveButton.Visibility = ViewStates.Invisible;
                    directionsButton.SetX(directionsButton.GetX() + 135);
                    buttonLayout.Visibility = ViewStates.Visible;
                    break;
            }
        }
    }
}