using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using Brady.ScrapRunner.Domain.Enums;
using BWF.DataServices.PortableClients;
using MvvmCross.Localization;
using MvvmCross.Plugins.Sqlite;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Linq;
    using Acr.UserDialogs;
    using MvvmCross.Core.ViewModels;
    using Validators;

    public class PowerUnitViewModel : BaseViewModel
    {
        private readonly IConnectionService<DataServiceClient> _connection;
        private readonly IRepository<PowerMasterModel> _powerMasterRepository;
        private readonly IRepository<EmployeeMasterModel> _employeeMasterRepository;
        private readonly IRepository<DriverStatusModel> _driverStatusRepository;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository; 

        public PowerUnitViewModel( 
            IConnectionService<DataServiceClient> connection,
            IRepository<PowerMasterModel> powerMasterRepository,
            IRepository<EmployeeMasterModel> employeeMasterRepository,
            IRepository<TripModel> tripRepository,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository,
            IRepository<DriverStatusModel> driverStatusRepository )
        {
            _connection = connection;
            _powerMasterRepository = powerMasterRepository;
            _employeeMasterRepository = employeeMasterRepository;
            _driverStatusRepository = driverStatusRepository;
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            Title = AppResources.PowerUnitViewTitle;
            PowerUnitIdCommand = new MvxCommand(ExecutePowerUnitIdCommand, CanExecutePowerUnitIdCommand);
        }

        private string _truckId;
        private int? _odometer;

        public string TruckId
        {
            get { return _truckId; }
            set
            {
                SetProperty(ref _truckId, value);
                PowerUnitIdCommand.RaiseCanExecuteChanged();
            }
        }

        public int? Odometer
        {
            get { return _odometer; }
            set
            {
                SetProperty(ref _odometer, value);
                PowerUnitIdCommand.RaiseCanExecuteChanged();
            }
        }

        public MvxCommand PowerUnitIdCommand { get; protected set; }

        protected async void ExecutePowerUnitIdCommand()
        {
            var truckIdResults = Validate<PowerUnitValidator, string>(TruckId);
            if (!truckIdResults.IsValid)
            {
                UserDialogs.Instance.Alert(truckIdResults.Errors.First().ErrorMessage);
                return;
            }
            var odometerResults = Validate<OdometerRangeValidator, int?>(Odometer);
            if (!odometerResults.IsValid)
            {
                UserDialogs.Instance.Alert(odometerResults.Errors.First().ErrorMessage);
                return;
            }

            try
            {
                var truckAndOdometerResult = await TruckAndOdometerAsync();
                if (!truckAndOdometerResult)
                {
                    await UserDialogs.Instance.AlertAsync(AppResources.SignInProcessFailed,
                        AppResources.Error, AppResources.OK);
                    return;
                }
            }
            catch (Exception exception)
            {
                var message = exception?.InnerException?.Message ?? exception.Message;
                await UserDialogs.Instance.AlertAsync(
                    message, AppResources.Error, AppResources.OK);
            }

            Close(this);
            ShowViewModel<RouteSummaryViewModel>();
        }

        private async Task<bool> TruckAndOdometerAsync()
        {

            using (var powerunitData = UserDialogs.Instance.Loading(AppResources.LoadingTripInformation, maskType: MaskType.Clear))
            {
                // Validate Power ID;
                var powerIdTask = await _connection.GetConnection().GetAsync<string, PowerMaster>(TruckId);
                if (powerIdTask == null) return false;
                await SavePowerMasterAsync(powerIdTask);

                //  Check for duplicate login; Possibly no longer needed?
                // @TODO : Implement; This may no longer be needed?

                // Determine current trip number, segment, and driver status;
                // If driver status record doesn't exist, write out a new remote record with initial values
                // and then write it to local SQLite DB
                // @TODO : Create specialized employee service
                var currentEmployeeTask = await _employeeMasterRepository.AllAsync();
                var currentEmployeeId = currentEmployeeTask?.First().EmployeeId;
                if (string.IsNullOrWhiteSpace(currentEmployeeId))
                    return false;

                // @TODO : Create specialized driver status service?
                var driverStatusTask =
                    await _connection.GetConnection().GetAsync<string, DriverStatus>(currentEmployeeId);
                if (driverStatusTask == null)
                {
                    var driverStatusObj = new DriverStatus
                    {
                        EmployeeId = currentEmployeeId,
                        Status = "L",
                        TerminalId = currentEmployeeId,
                        RegionId = currentEmployeeId,
                        PowerId = TruckId,
                        LoginDateTime = DateTime.Now,
                        ActionDateTime = DateTime.Now,
                        Odometer = Odometer,
                        SendHHLogoffFlag = 0
                    };

                    var createDriverStatus = await _connection.GetConnection().UpdateAsync(driverStatusObj);
                    if (!createDriverStatus.WasSuccessful) return false;
                    await SaveDriverStatusAsync(driverStatusObj);
                }
                else
                {
                    driverStatusTask.ActionDateTime = DateTime.Now;
                    driverStatusTask.LoginDateTime = DateTime.Now;
                    var updateDriverStatus = await _connection.GetConnection().UpdateAsync(driverStatusTask);
                    await SaveDriverStatusAsync(driverStatusTask);
                }

                // @TODO : If trip is in progress, update Trip In Progress Flag in TripTable

                // Grab avaliable trips for Driver
                var tripsTask = await _connection.GetConnection().QueryAsync(new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(currentEmployeeId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver))
                    .OrderBy(x => x.TripSequenceNumber));
                   
                if (tripsTask == null) return false;
                await SaveTripsAsync(tripsTask.Records);

                // Grab trip segments for each trip brought back
                var tripNumbers = tripsTask.Records.Select(x => x.TripNumber).ToArray();
                var tripSegmentTask = await _connection.GetConnection().QueryAsync(new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).In(tripNumbers)));
                if (tripSegmentTask == null) return false;
                await SaveTripSegmentsAsync(tripSegmentTask.Records);

                // Grab all containers for each trip segment
                var tripSegmentContainerTask =
                    await _connection.GetConnection().QueryAsync(new QueryBuilder<TripSegmentContainer>()
                        .Filter(y => y.Property(x => x.TripNumber).In(tripNumbers)));
                if (tripSegmentContainerTask == null) return false;
                await SaveTripSegmentContainersAsync(tripSegmentContainerTask.Records);
            }

            return true;
        }

        private Task SavePowerMasterAsync(PowerMaster powerMaster)
        {
            var mapped = AutoMapper.Mapper.Map<PowerMaster, PowerMasterModel>(powerMaster);
            return _powerMasterRepository.InsertAsync(mapped);
        }

        private Task SaveDriverStatusAsync(DriverStatus driverStatus)
        {
            var mapped = AutoMapper.Mapper.Map<DriverStatus, DriverStatusModel>(driverStatus);
            return _driverStatusRepository.InsertAsync(mapped);
        }

        private Task SaveTripsAsync(List<Trip> trips)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(trips);
            return _tripRepository.InsertRangeAsync(mapped);
        }

        private Task SaveTripSegmentsAsync(List<TripSegment> tripSegments)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<TripSegment>, IEnumerable<TripSegmentModel>>(tripSegments);
            return _tripSegmentRepository.InsertRangeAsync(mapped);
        }

        private Task SaveTripSegmentContainersAsync(List<TripSegmentContainer> containers)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<TripSegmentContainer>, IEnumerable<TripSegmentContainerModel>>(containers);
            return _tripSegmentContainerRepository.InsertRangeAsync(mapped);
        }

        protected bool CanExecutePowerUnitIdCommand()
        {
            return !string.IsNullOrWhiteSpace(TruckId)
                   && Odometer.HasValue;
        }
    }
}