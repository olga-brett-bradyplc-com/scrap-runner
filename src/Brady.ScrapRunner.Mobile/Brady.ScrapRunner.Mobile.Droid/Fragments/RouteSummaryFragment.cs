using System;
using System.Linq;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Messages;
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

        protected override int FragmentId => Resource.Layout.fragment_routesummary;
        protected override bool NavMenuEnabled => true;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _mvxMessenger = Mvx.Resolve<IMvxMessenger>();
            _mvxSubscriptionToken = _mvxMessenger.SubscribeOnMainThread<TripNotificationMessage>(OnTripNotification);
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