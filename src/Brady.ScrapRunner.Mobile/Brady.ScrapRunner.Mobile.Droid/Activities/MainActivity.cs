using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Caching;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    [Activity(
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.MainActivity"
    )]
    public class MainActivity : MvxCachingFragmentCompatActivity<MainViewModel>
    {
        public DrawerLayout DrawerLayout;

        private const string MenuViewModel = "MenuViewModel";

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

        public override void OnBackPressed()
        {
            if (DrawerLayout != null && DrawerLayout.IsDrawerOpen(GravityCompat.Start))
                DrawerLayout.CloseDrawers();
            else
                base.OnBackPressed();
        }

        public override void OnBeforeFragmentChanging(IMvxCachedFragmentInfo fragmentInfo, Android.Support.V4.App.FragmentTransaction transaction)
        {
            if (!fragmentInfo.Tag.Contains(MenuViewModel))
            {
                transaction.SetCustomAnimations(Resource.Animation.custom_enter_anim,
                    Resource.Animation.custom_leave_anim);
            }
        }

        public void HideSoftKeyboard()
        {
            if (CurrentFocus == null) return;

            InputMethodManager inputMethodManager = (InputMethodManager) GetSystemService(InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);

            CurrentFocus.ClearFocus();
        }
    }
}