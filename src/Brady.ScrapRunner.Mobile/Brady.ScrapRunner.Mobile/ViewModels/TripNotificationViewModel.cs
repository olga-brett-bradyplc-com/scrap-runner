namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using BWF.DataServices.Metadata.Models;
    using Domain;
    using Domain.Process;
    using Interfaces;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Platform;
    using Resources;

    public class TripNotificationViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;
        private readonly INotificationService _notificationService;
        private string _tripNumber;
        private TripNotificationContext _notificationContext;
        private int _notificationId;

        public TripNotificationViewModel(
            ITripService tripService, 
            IDriverService driverService, 
            INotificationService notificationService)
        {
            _tripService = tripService;
            _driverService = driverService;
            _notificationService = notificationService;
        }

        private string _notificationMessage;
        public string NotificationMessage
        {
            get { return _notificationMessage;}
            set { SetProperty(ref _notificationMessage, value); }
        }

        public void Init(string tripNumber, TripNotificationContext notificationContext, int notificationId)
        {
            _tripNumber = tripNumber;
            _notificationContext = notificationContext;
            _notificationId = notificationId;
        }

        public override async void Start()
        {
            try
            {
                base.Start();
                await StartAsync();
            }
            catch (Exception e)
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Error starting TripNotificationViewModel: {e.Message}");
            }
        }

        private async Task StartAsync()
        {
            if (_notificationContext == TripNotificationContext.Resequenced)
            {
                // Resequence notification doesn't have a specific trip associated with it.
                Title = AppResources.NotificationTripResequenceTitle;
                NotificationMessage = AppResources.NotificationTripResequenceText;
                return;
            }
            var trip = await _tripService.FindTripAsync(_tripNumber);
            if (trip == null)
            {
                Mvx.TaggedWarning(Constants.ScrapRunner, $"Failed to find trip {_tripNumber}");
                return;
            }
            var tripCustomerName = trip.TripCustName;
            switch (_notificationContext)
            {
                case TripNotificationContext.New:
                    Title = AppResources.NotificationNewTripTitle;
                    NotificationMessage = string.Format(AppResources.NotificationNewTripText, _tripNumber, tripCustomerName); ;
                    break;
                case TripNotificationContext.Modified:
                    Title = AppResources.NotificationTripModifiedTitle;
                    NotificationMessage = string.Format(AppResources.NotificationTripModifiedText, _tripNumber, tripCustomerName);
                    break;
                case TripNotificationContext.Canceled:
                case TripNotificationContext.Future:
                case TripNotificationContext.Reassigned:
                case TripNotificationContext.Unassigned:
                    Title = AppResources.NotificationTripCanceledTitle;
                    NotificationMessage = string.Format(AppResources.NotificationTripCanceledText, tripCustomerName);
                    break;
                case TripNotificationContext.OnHold:
                    Title = AppResources.NotificationTripOnHoldTitle;
                    NotificationMessage = string.Format(AppResources.NotificationTripOnHoldText, tripCustomerName);
                    break;
                case TripNotificationContext.MarkedDone:
                    Title = AppResources.NotificationTripMarkedDoneTitle;
                    NotificationMessage = string.Format(AppResources.NotificationTripMarkedDoneText, tripCustomerName);
                    break;
                case TripNotificationContext.Resequenced: // Handled above
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private MvxAsyncCommand _ackCommand;
        public IMvxAsyncCommand AckCommand => _ackCommand ?? (_ackCommand = new MvxAsyncCommand(ExecuteAckCommandAsync));

        private async Task<ChangeResultWithItem<DriverTripAckProcess>> AckTripAsync()
        {
            var driverStatusModel = await _driverService.GetCurrentDriverStatusAsync();
            if (driverStatusModel == null)
            {
                Mvx.TaggedWarning(Constants.ScrapRunner, $"Failed to find DriverStatus - Can't ack trip {_tripNumber}.");
                return null;
            }
            var ackProcess = new DriverTripAckProcess
            {
                EmployeeId = driverStatusModel.EmployeeId,
                TripNumber = _tripNumber,
                ActionDateTime = DateTime.Now,
                Mdtid = driverStatusModel.EmployeeId
            };
            return await _tripService.ProcessDriverTripAck(ackProcess);
        }

        private async Task ExecuteAckCommandAsync()
        {
            if (_notificationContext == TripNotificationContext.New ||
                _notificationContext == TripNotificationContext.Modified)
            {
                var ackResult = await AckTripAsync();
                if (!ackResult.WasSuccessful)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, $"ProcessDriverTripAck failed {ackResult.Failure?.Summary}");
                }
            }
            Close();
        }

        private void Close()
        {
            _notificationService.Cancel(_notificationId);
            Close(this);
        }
    }
}