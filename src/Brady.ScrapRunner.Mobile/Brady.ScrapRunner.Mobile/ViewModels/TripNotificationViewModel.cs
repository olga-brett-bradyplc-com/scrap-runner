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

    public class TripNotificationViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private string _tripNumber;
        private TripNotificationContext _notificationContext;

        public TripNotificationViewModel(ITripService tripService)
        {
            _tripService = tripService;
        }

        private string _notificationMessage;
        public string NotificationMessage
        {
            get { return _notificationMessage;}
            set { SetProperty(ref _notificationMessage, value); }
        }

        public void Init(string tripNumber, TripNotificationContext notificationContext)
        {
            _tripNumber = tripNumber;
            _notificationContext = notificationContext;
        }

        public override async void Start()
        {
            base.Start();
            try
            {
                await StartAsync();
            }
            catch (Exception e)
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Error starting TripNotificationViewModel. {e.Message}");
            }
        }

        private async Task StartAsync()
        {
            var trip = await _tripService.FindTripAsync(_tripNumber);
            if (trip == null) return;
            var tripCustomerName = trip.TripCustName;
            // @TODO: Set NotificationMessage depending on _notificationContext here:
            switch (_notificationContext)
            {
                case TripNotificationContext.New:
                    // $"New Trip ({_tripNumber}) {tripCustomerName}"
                    break;
                case TripNotificationContext.Modified:
                    // $"Trip Modified ({_tripNumber})"
                    break;
                case TripNotificationContext.Canceled:
                    // $"Trip canceled by DISPATCH {tripCustomerName}"
                    break;
                case TripNotificationContext.OnHold:
                    // $"Trip placed on hold by DISPATCH {tripCustomerName}"
                    break;
                case TripNotificationContext.Future:
                    // $"Trip canceled by DISPATCH {tripCustomerName}"
                    break;
                case TripNotificationContext.Reassigned:
                    // $"Trip canceled by DISPATCH {tripCustomerName}"
                    break;
                case TripNotificationContext.Unassigned:
                    // $"Trip canceled by DISPATCH {tripCustomerName}"
                    break;
                case TripNotificationContext.MarkedDone:
                    // $"Trip marked done by DISPATCH {tripCustomerName}"
                    break;
                case TripNotificationContext.Resequenced:
                    // Trips Resequenced
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private MvxAsyncCommand _ackCommand;
        public IMvxAsyncCommand AckCommand => _ackCommand ?? (_ackCommand = new MvxAsyncCommand(ExecuteAckCommand));

        private Task<ChangeResultWithItem<DriverTripAckProcess>> AckTripAsync()
        {
            // @TODO: Get the EmployeeId of the driver.
            var ackProcess = new DriverTripAckProcess
            {
                //EmployeeId = employeeId,
                TripNumber = _tripNumber,
                ActionDateTime = DateTime.Now,
                //Mdtid = employeeId
            };
            return _tripService.ProcessDriverTripAck(ackProcess);
        }

        private async Task ExecuteAckCommand()
        {
            if (_notificationContext == TripNotificationContext.New ||
                _notificationContext == TripNotificationContext.Modified)
            {
                var ackResult = await AckTripAsync();
                if (!ackResult.WasSuccessful)
                {
                    Mvx.TaggedWarning(Constants.ScrapRunner, $"ProcessDriverTripAck failed {ackResult.Failure.Summary}");
                }
            }
            Close(this);
        }
    }
}