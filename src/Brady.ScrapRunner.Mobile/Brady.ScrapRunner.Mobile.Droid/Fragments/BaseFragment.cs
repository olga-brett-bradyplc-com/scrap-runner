using System;
using System.ComponentModel;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platform.WeakSubscription;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    public abstract class BaseFragment<T> : MvxFragment<T> where T : BaseViewModel
    {
        private Toolbar _toolbar;
        private MvxActionBarDrawerToggle _drawerToggle;
        private IDisposable _titleToken;
        private IDisposable _subTitleToken;

        protected BaseFragment()
        {
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(FragmentId, null);

            var baseActivity = ((MainActivity)Activity);
            _toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);

            // By default, set drawerlayout as locked so a user can't swipe the menu when it shouldn't be accessible
            baseActivity.DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);

            if (_toolbar != null)
            {
                baseActivity.SetSupportActionBar(_toolbar);
                baseActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(NavMenuEnabled);
                baseActivity.SupportActionBar.SetDisplayShowHomeEnabled(NavMenuEnabled);

                _toolbar.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, NavColor)));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    baseActivity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    baseActivity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    baseActivity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Activity, NavColor)));
                }

                if (NavMenuEnabled)
                {
                    _drawerToggle = new MvxActionBarDrawerToggle(
                        Activity,
                        baseActivity.DrawerLayout,
                        _toolbar,
                        Resource.String.drawer_open,
                        Resource.String.drawer_close);

                    baseActivity.DrawerLayout.AddDrawerListener(_drawerToggle);
                    baseActivity.DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
                }

                if (!string.IsNullOrEmpty(ViewModel.Title))
                    baseActivity.SupportActionBar.Title = ViewModel.Title;
                if (!string.IsNullOrEmpty(ViewModel.SubTitle))
                    baseActivity.SupportActionBar.Subtitle = ViewModel.SubTitle;
                _titleToken = ViewModel.WeakSubscribe(() => ViewModel.Title, OnTitleChanged);
                _subTitleToken = ViewModel.WeakSubscribe(() => ViewModel.SubTitle, OnSubTitleChanged);
            }

            return view;
        }

        private void OnTitleChanged(object sender, PropertyChangedEventArgs args)
        {
            var baseActivity = ((MainActivity)Activity);
            if (baseActivity.SupportActionBar == null) return;
            baseActivity.SupportActionBar.Title = ViewModel.Title;
        }

        private void OnSubTitleChanged(object sender, PropertyChangedEventArgs args)
        {
            var baseActivity = ((MainActivity)Activity);
            if (baseActivity.SupportActionBar == null) return;
            baseActivity.SupportActionBar.Subtitle = ViewModel.SubTitle;
        }

        protected abstract int FragmentId { get; }
        protected abstract bool NavMenuEnabled { get; }
        protected virtual int NavColor { get; set; } = Resource.Color.material_gray_900;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            if (_toolbar != null)
            {
                _drawerToggle?.SyncState();
            }
        }
    }
}