using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Domain;
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
        private MvxSubscriptionToken _mvxSubscriptionToken, _mvxSubscriptionToken2;
        private IMvxMessenger _mvxMessenger;
        private IDriverService _driverService;
        private ITripService _tripService;

        protected override int FragmentId => Resource.Layout.fragment_routesummary;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _mvxMessenger = Mvx.Resolve<IMvxMessenger>();
            _driverService = Mvx.Resolve<IDriverService>();
            _tripService = Mvx.Resolve<ITripService>();

            // TODO : Disabling this until polling service pulls TripSegments and TripSegmentContainers when fetching new trips
            _mvxSubscriptionToken = _mvxMessenger.SubscribeOnMainThread<TripNotificationMessage>(OnTripNotification);
            _mvxSubscriptionToken2 =
                _mvxMessenger.SubscribeOnMainThread<TripResequencedMessage>(OnTripResequenceNotification);

            var task = CheckMenuState();
        }

        private async Task CheckMenuState()
        {
            var driverStatus = await _driverService.GetCurrentDriverStatusAsync();
            
            // If driver is in the middle of a trip, do not change the menu to MenuState.Avaliable
            // We never set MenuState.OnTrip here because a user would never be taken here manually if on a trip
            if (driverStatus.Status != DriverStatusSRConstants.Enroute && driverStatus.Status != DriverStatusSRConstants.Arrive && 
                driverStatus.Status != DriverStatusSRConstants.Done)
                _mvxMessenger.Publish(new MenuStateMessage(this) { Context = MenuState.Avaliable });
            if (driverStatus.Status != DriverStatusSRConstants.Enroute && driverStatus.Status != DriverStatusSRConstants.Arrive)
                _mvxMessenger.Publish(new MenuStateMessage(this) { Context = MenuState.Avaliable });
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_mvxSubscriptionToken == null && _mvxSubscriptionToken2 == null)
                return;
            if(_mvxSubscriptionToken != null) 
                _mvxMessenger.Unsubscribe<TripNotificationMessage>(_mvxSubscriptionToken);
            if(_mvxSubscriptionToken2 != null)
                _mvxMessenger.Unsubscribe<TripResequencedMessage>(_mvxSubscriptionToken2);
        }
        private async void OnTripResequenceNotification(TripResequencedMessage msg)
        {
            ViewModel.RouteSummaryList.Clear();
            List<TripModel> trips = await _tripService.FindTripsAsync();
            foreach (var trip in trips)
                ViewModel.RouteSummaryList.Add(trip);
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
                    if (index > -1)
                        ViewModel.RouteSummaryList[index] = msg.Trip;
                    break;
                case TripNotificationContext.OnHold:
                    break;
                case TripNotificationContext.Future:
                    break;
                case TripNotificationContext.Unassigned:
                    break;
                case TripNotificationContext.Resequenced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}