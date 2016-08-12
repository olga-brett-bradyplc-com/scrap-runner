using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Caching;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platform;

namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    [Activity(
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.MainActivity"
    )]
    public class MainActivity : MvxCachingFragmentCompatActivity<MainViewModel>
    {
        public DrawerLayout DrawerLayout;

        private Type _previousFragmentType;
        private Type _currentFragmentType;

        private const string MenuViewModelKey = "MenuViewModel";
        private const string DelayViewModelKey = "DelayViewModel";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);
            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            if (bundle == null)
                ViewModel.ShowMenu();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    DrawerLayout.OpenDrawer(GravityCompat.Start);
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        // Forcing a fragment replace solves some caching issues we were running in to. This may not be the best long term option.
        // As a reminder, we're inheriting from MvxCachingFragmentCompatActivity instead of MvxFragmentCompatActivity because the latter
        // doesn't implement IFragmentHost, which causes the app to crash. @TODO : Look into alternatives
        protected override void ShowFragment(string tag, int contentId, Bundle bundle, bool forceAddToBackStack = false,
            bool forceReplaceFragment = false)
        {
            base.ShowFragment(tag, contentId, bundle, forceAddToBackStack, true);
        }

        private void ShowBackButton()
        {
            DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
        }

        private void ShowHamburgerMenu()
        {
            DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
        }

        public override async void OnBackPressed()
        {
            UpdateBackStackValues();

            if (DrawerLayout != null && DrawerLayout.IsDrawerOpen(GravityCompat.Start))
                DrawerLayout.CloseDrawers();

            // Assume if previous = SignIn, and current = Settings, they haven't actually logged in yet
            else if (_previousFragmentType == typeof(SignInViewModel) && _currentFragmentType != typeof(SettingsViewModel))
            {
                var viewmodel = FindViewModelOnBackStack(MenuViewModelKey);
                var command = viewmodel?.ViewModel as MenuViewModel;
                var executeAsync = command?.LogoutCommand.ExecuteAsync();
                if (executeAsync != null) await executeAsync;
            }
            else if (_currentFragmentType == typeof(DelayViewModel))
            {
                var viewmodel = FindViewModelOnBackStack(DelayViewModelKey);
                var command = viewmodel?.ViewModel as DelayViewModel;
                var executeAsync = command?.BackOnDutyCommand.ExecuteAsync();
                if (executeAsync != null) await executeAsync;
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override void OnBeforeFragmentChanging(IMvxCachedFragmentInfo fragmentInfo, Android.Support.V4.App.FragmentTransaction transaction)
        {
            // Instead of relying on fragmentInfo paramter to set _currentFragmentType, we use UpdateBackStackValues because OnBeforeFragmentChanging isn't
            // called when the back button is pressed.
            UpdateBackStackValues();

            // Disabling for now until some quirks can be worked out
            //if (!fragmentInfo.Tag.Contains(MenuViewModel))
            //{
            //    transaction.SetCustomAnimations(Resource.Animation.custom_enter_anim,
            //        Resource.Animation.custom_leave_anim);
            //}
        }

        public void HideSoftKeyboard()
        {
            if (CurrentFocus == null) return;

            var inputMethodManager = (InputMethodManager) GetSystemService(InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);

            CurrentFocus.ClearFocus();
        }

        private void UpdateBackStackValues()
        {
            var backstack = SupportFragmentManager.Fragments?.Where(t => t?.Tag != null && !t.Tag.Contains(MenuViewModelKey)).ToList();
            var currentItem = backstack?.LastOrDefault() as MvxFragment;
            _currentFragmentType = currentItem?.ViewModel.GetType();

            if (backstack?.Count > 1)
            {
                var previousItem = backstack[backstack.Count - 2] as MvxFragment;
                _previousFragmentType = previousItem?.ViewModel.GetType();
            }
            else
            {
                _previousFragmentType = null;
            }
        }

        private MvxFragment FindViewModelOnBackStack(string viewModelKey)
        {
            return
                SupportFragmentManager.Fragments.Where(t => t?.Tag != null)
                    .ToList()
                    .FirstOrDefault(f => f.Tag.Contains(viewModelKey)) as MvxFragment;
        }
    }
}