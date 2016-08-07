using System;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Messages;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Messages;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.RouteSummaryFragment")]
    public class RouteSummaryFragment : BaseFragment<RouteSummaryViewModel>
    {
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private IMvxMessenger _mvxMessenger;
        private IDriverService _driverService;

        protected override int FragmentId => Resource.Layout.fragment_routesummary;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _mvxMessenger = Mvx.Resolve<IMvxMessenger>();
            _driverService = Mvx.Resolve<IDriverService>();

            // TODO : Disabling this until polling service pulls TripSegments and TripSegmentContainers when fetching new trips
            //_mvxSubscriptionToken = _mvxMessenger.SubscribeOnMainThread<TripNotificationMessage>(OnTripNotification);

            var task = CheckMenuState();
        }

        private async Task CheckMenuState()
        {
            var driverStatus = await _driverService.GetCurrentDriverStatusAsync();
            
            // If driver is in the middle of a trip, do not change the menu to MenuState.Avaliable
            // We never set MenuState.OnTrip here because a user would never be taken here manually if on a trip
            if (driverStatus.Status != "E" && driverStatus.Status != "A" && driverStatus.Status != "D")
                _mvxMessenger.Publish(new MenuStateMessage(this) { Context = MenuState.Avaliable });
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_mvxSubscriptionToken == null)
                return;

            _mvxMessenger.Unsubscribe<TripNotificationMessage>(_mvxSubscriptionToken);
        }

        private void OnTripNotification(TripNotificationMessage msg)
        {
            switch (msg.Context)
            {
                case TripNotificationContext.Canceled:
                case TripNotificationContext.Reassigned:
                case TripNotificationContext.MarkedDone:
                    var remove = ViewModel.RouteSummaryList.SingleOrDefault(t => t.TripNumber == msg.Trip.TripNumber);
                    ViewModel.RouteSummaryList.Remove(remove);
                    break;
                case TripNotificationContext.New:
                    ViewModel.RouteSummaryList.Add(msg.Trip);
                    break;
                case TripNotificationContext.Modified:
                    var index = ViewModel.RouteSummaryList.IndexOf(ViewModel.RouteSummaryList.SingleOrDefault(t => t.TripNumber == msg.Trip.TripNumber));
                    ViewModel.RouteSummaryList[index] = msg.Trip;
                    break;
                case TripNotificationContext.OnHold:
                    break;
                case TripNotificationContext.Future:
                    break;
                case TripNotificationContext.Unassigned:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}