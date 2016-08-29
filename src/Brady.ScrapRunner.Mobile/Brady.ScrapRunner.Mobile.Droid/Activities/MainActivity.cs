using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using MvvmCross.Droid.Shared.Fragments;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views;
using SupportV4 = Android.Support.V4.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Exceptions;

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

        private static MainActivity _instance;

        private const string MenuViewModelKey = "MenuViewModel";
        private const string DelayViewModelKey = "DelayViewModel";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _instance = this;

            SetContentView(Resource.Layout.activity_main);
            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
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

        protected override void ShowFragment(string tag, int contentId, Bundle bundle, bool forceAddToBackStack = false, bool forceReplaceFragment = false)
        {
            base.ShowFragment(tag, contentId, bundle, forceAddToBackStack, true);
        }

        public override async void OnBackPressed()
        {
            UpdateBackStackValues();

            if (DrawerLayout != null && DrawerLayout.IsDrawerOpen(GravityCompat.Start))
                DrawerLayout.CloseDrawers();
            
            if (_currentFragmentType == typeof(DelayViewModel))
            {
                var viewmodel = FindViewModelOnBackStack(DelayViewModelKey);
                var command = viewmodel?.ViewModel as DelayViewModel;
                var executeAsync = command?.BackOnDutyCommand.ExecuteAsync();
                if (executeAsync != null) await executeAsync;
            }
            else if (_previousFragmentType == null)
            {
                MoveTaskToBack(true); // Or should we just swallow this call?
            }
            else
            {
                base.OnBackPressed();
            }
        }

        // Overriding this method to remove a check that calls base.OnBackPressed if SupportFragmentManager.BackStackEntryCount == 0
        // This was causing problems throughout our navigation.
        // If this mod causes problems, we may have to rethink how we navigate from page to page
        public override bool Close(IMvxViewModel viewModel)
        {
            var frag = GetCurrentCacheableFragmentsInfo().FirstOrDefault(x => x.ViewModelType == viewModel.GetType());
            if (frag == null)
            {
                return false;
            }

            CloseFragment(frag.Tag, frag.ContentId);
            return true;
        }

        //public override void OnBeforeFragmentChanging(IMvxCachedFragmentInfo fragmentInfo, Android.Support.V4.App.FragmentTransaction transaction)
        //{
        //    if (!fragmentInfo.Tag.Contains(MenuViewModelKey))
        //        transaction.SetCustomAnimations(Resource.Animation.custom_enter_anim, Resource.Animation.custom_leave_anim);
        //}
        
        public override void OnFragmentChanged(IMvxCachedFragmentInfo fragmentInfo)
        {
            UpdateBackStackValues();
        }

        public void HideSoftKeyboard()
        {
            if (CurrentFocus == null) return;

            var inputMethodManager = (InputMethodManager) GetSystemService(InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);

            CurrentFocus.ClearFocus();
        }

        // This will allow us to finish this activity if end-user logs out. We don't want MainActivity+Fragments on the backstack in that case
        public static MainActivity GetInstance()
        {
            return _instance;
        }

        public static void ResetInstance()
        {
            _instance = null;
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