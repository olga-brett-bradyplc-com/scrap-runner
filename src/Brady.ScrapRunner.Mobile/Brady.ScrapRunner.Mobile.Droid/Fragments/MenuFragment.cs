using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Support.V7.Fragging.Fragments;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.navigation_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.MenuFragment")]
    public class MenuFragment : MvxFragment<MenuViewModel>, NavigationView.IOnNavigationItemSelectedListener
    {
        private NavigationView _navigationView;
        private IMenuItem _previousMenuItem;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.fragment_navigation, null);
            _navigationView = view.FindViewById<NavigationView>(Resource.Id.navigation_view);
            _navigationView.SetNavigationItemSelectedListener(this);

            return view;
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            if (menuItem != _previousMenuItem)
            {
                _previousMenuItem?.SetChecked(false);
            }

            menuItem.SetCheckable(true);
            menuItem.SetChecked(true);

            _previousMenuItem = menuItem;

            Navigate(menuItem.ItemId);

            return true;
        }

        private async Task Navigate(int itemId)
        {
            var baseActivity = ((MainActivity) Activity);
            baseActivity.DrawerLayout.CloseDrawers();
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            switch (itemId)
            {
                case Resource.Id.nav_logout:
                    ViewModel.LogoutCommand.Execute();
                    break;
            }
        }
    }
}