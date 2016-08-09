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
            // @TODO: Use Resources here
            switch (_notificationContext)
            {
                case TripNotificationContext.New:
                    Title = "New Trip";
                    NotificationMessage = $"New Trip ({_tripNumber}) {tripCustomerName}";
                    break;
                case TripNotificationContext.Modified:
                    Title = "Modified Trip";
                    NotificationMessage = $"Trip Modified ({_tripNumber})";
                    break;
                case TripNotificationContext.Canceled:
                    Title = "Canceled Trip";
                    NotificationMessage = $"Trip canceled by DISPATCH {tripCustomerName}";
                    break;
                case TripNotificationContext.OnHold:
                    Title = "Trip On Hold";
                    NotificationMessage = $"Trip placed on hold by DISPATCH {tripCustomerName}";
                    break;
                case TripNotificationContext.Future:
                    Title = "Future Trip";
                    NotificationMessage = $"Trip canceled by DISPATCH {tripCustomerName}";
                    break;
                case TripNotificationContext.Reassigned:
                    Title = "Reassigned Trip";
                    NotificationMessage = $"Trip canceled by DISPATCH {tripCustomerName}";
                    break;
                case TripNotificationContext.Unassigned:
                    Title = "Unassigned Trip";
                    NotificationMessage = $"Trip canceled by DISPATCH {tripCustomerName}";
                    break;
                case TripNotificationContext.MarkedDone:
                    Title = "Done Trip";
                    NotificationMessage = $"Trip marked done by DISPATCH {tripCustomerName}";
                    break;
                case TripNotificationContext.Resequenced:
                    Title = "Trips Resequenced";
                    NotificationMessage = "Trips Resequenced";
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