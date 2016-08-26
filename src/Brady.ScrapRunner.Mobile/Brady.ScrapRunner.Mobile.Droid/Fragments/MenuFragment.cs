using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.Droid.Messages;
using Brady.ScrapRunner.Mobile.Messages;
using Brady.ScrapRunner.Mobile.Resources;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.navigation_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.MenuFragment")]
    public class MenuFragment : MvxFragment<MenuViewModel>, NavigationView.IOnNavigationItemSelectedListener
    {
        private NavigationView _navigationView;
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private MvxSubscriptionToken _mvxMenuStateToken;
        private MvxSubscriptionToken _mvxDriverInfoToken;
        private IMvxMessenger _mvxMessenger;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.fragment_navigation, null);

            _navigationView = view.FindViewById<NavigationView>(Resource.Id.navigation_view);
            _navigationView.SetNavigationItemSelectedListener(this);

            _mvxMessenger = Mvx.Resolve<IMvxMessenger>();

            _mvxSubscriptionToken = _mvxMessenger.Subscribe<ForceLogoffMessage>(OnForcedLogoffMessage);
            _mvxMenuStateToken = _mvxMessenger.Subscribe<MenuStateMessage>(OnMenuStateChanged);
            //_mvxDriverInfoToken = _mvxMessenger.Subscribe<DriverInfoMessage>(OnDriverInfoChanged);

            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_mvxSubscriptionToken != null)
                _mvxMessenger.Unsubscribe<ForceLogoffMessage>(_mvxSubscriptionToken);

            if (_mvxMenuStateToken != null)
                _mvxMessenger.Unsubscribe<MenuStateMessage>(_mvxMenuStateToken);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            Navigate(menuItem.ItemId);
            return true;
        }

        private void OnForcedLogoffMessage(ForceLogoffMessage obj)
        {
            ViewModel.ForcedLogoffCommand.Execute(obj);
        }

        private void OnMenuStateChanged(MenuStateMessage msg)
        {
            switch (msg.Context)
            {
                case MenuState.Avaliable:
                    _navigationView.Menu.FindItem(Resource.Id.current_actions_nav).SetVisible(true);
                    _navigationView.Menu.FindItem(Resource.Id.nav_loadcontainer).SetVisible(true);
                    _navigationView.Menu.FindItem(Resource.Id.nav_takepicture).SetVisible(false);
                    break;
                case MenuState.OnTrip:
                    _navigationView.Menu.FindItem(Resource.Id.current_actions_nav).SetVisible(true);
                    _navigationView.Menu.FindItem(Resource.Id.nav_loadcontainer).SetVisible(false);
                    _navigationView.Menu.FindItem(Resource.Id.nav_takepicture).SetVisible(true);
                    break;
                case MenuState.OnDelay:
                    _navigationView.Menu.FindItem(Resource.Id.current_actions_nav).SetVisible(false);
                    break;
            }
        }

        private async Task Navigate(int itemId)
        {
            var baseActivity = ((MainActivity) Activity);
            baseActivity.DrawerLayout.CloseDrawers();
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            switch (itemId)
            {
                case Resource.Id.nav_routesummary:
                    ViewModel.RouteSummaryCommand.Execute();
                    break;
                case Resource.Id.nav_logout:
                    ViewModel.LogoutCommand.Execute();
                    break;
                case Resource.Id.nav_fuelentry:
                    ViewModel.FuelEntryCommand.Execute();
                    break;
                case Resource.Id.nav_messages:
                    ViewModel.MessagesCommand.Execute();
                    break;
                case Resource.Id.nav_composemessage:
                    ViewModel.NewMessageCommand.Execute();
                    break;
                case Resource.Id.nav_adddelay:
                    await ViewModel.DelayCommandAsync.ExecuteAsync();
                    break;
                case Resource.Id.nav_takepicture:
                    ViewModel.TakePictureCommand.Execute();
                    break;
                case Resource.Id.nav_loadcontainer:
                    ViewModel.LoadDropContainersCommand.Execute();
                    break;
                case Resource.Id.nav_changeodometer:
                    await ViewModel.ChangeOdometerCommand.ExecuteAsync();
                    break;
            }
        }
    }
}