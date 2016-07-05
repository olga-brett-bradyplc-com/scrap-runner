namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Models;
    using BWF.DataServices.PortableClients;
    using Domain;
    using Domain.Enums;
    using Domain.Models;
    using Interfaces;
    using Plugin.Settings.Abstractions;

    public class PollingService : IPollingService
    {
        private readonly IConnectionService _connectionService;
        private readonly INotificationService _notificationService;
        private readonly ISettings _settings;
        private const string TerminalMasterSettingsKey = "TerminalMasterDateTime";
        private const string ContainerMasterSettingsKey = "ContainerMasterDateTime";

        public PollingService(IConnectionService connectionService, 
            INotificationService notificationService, 
            ISettings settings)
        {
            _connectionService = connectionService;
            _notificationService = notificationService;
            _settings = settings;
        }

        public async Task PollForChangesAsync(string driverId, string terminalId, string regionId, string areaId)
        {
            if (driverId == null) throw new ArgumentNullException(nameof(driverId));
            await PollForMessagesAsync(driverId);
            await PollForTripsAfterLoginAsync(driverId);
            await PollForTripsCanceledAsync(driverId);
            await PollForTripsUnassignedAsync(driverId);
            await PollForTripsMarkedDoneAsync(driverId);
            await PollForTripsResequencedAsync(driverId);
            await PollForContainerChangesAsync(terminalId, regionId);
            await PollForTerminalChangesAsync(areaId, regionId);
            await PollForceLogoffAsync(driverId);
        }

        private Task<QueryResult<Trip>> GetTripsAfterLoginAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverId).EqualTo(driverId)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.Ready))
                .OrderBy(x => x.TripSequenceNumber);
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForTripsAfterLoginAsync(string driverId)
        {
            var tripsAfterLogin = await GetTripsAfterLoginAsync(driverId);
            foreach (var trip in tripsAfterLogin.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
                _notificationService.Trip(trip, TripNotificationContext.New);
            }
        }

        private Task<QueryResult<Trip>> GetTripsCanceledAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverId).EqualTo(driverId)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Canceled, TripStatusConstants.Hold, TripStatusConstants.Future)
                .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.CanceledReady))
                .OrderBy(x => x.TripSequenceNumber);
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForTripsCanceledAsync(string driverId)
        {
            var canceledTrips = await GetTripsCanceledAsync(driverId);
            foreach (var trip in canceledTrips.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
                _notificationService.Trip(trip, TripNotificationContext.Canceled);
            }
        }

        private Task<QueryResult<Trip>> GetTripsUnassignedAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverIdPrev).EqualTo(driverId)
                .And().Property(x => x.TripStatusPrev).NotEqualTo(TripStatusConstants.Future).Or(x => x.TripStatusPrev).IsNull()
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Missed, TripStatusConstants.Pending)
                .Or(x => x.TripStatusPrev).In(TripStatusConstants.Missed, TripStatusConstants.Pending));
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForTripsUnassignedAsync(string driverId)
        {
            var unassignedTrips = await GetTripsUnassignedAsync(driverId);
            foreach (var trip in unassignedTrips.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
                _notificationService.Trip(trip, TripNotificationContext.Unassigned);
            }
        }

        private Task<QueryResult<Trip>> GetTripsMarkedDoneAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverId).EqualTo(driverId)
                .And().Property(x => x.TripDriverIdPrev).IsNotNull()
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Done, TripStatusConstants.Exception, TripStatusConstants.Review, TripStatusConstants.ErrorQueue));
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForTripsMarkedDoneAsync(string driverId)
        {
            var doneTrips = await GetTripsMarkedDoneAsync(driverId);
            foreach (var trip in doneTrips.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
                _notificationService.Trip(trip, TripNotificationContext.MarkedDone);
            }
        }

        private Task<QueryResult<Trip>> GetTripsResequencedAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverId).EqualTo(driverId)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                .And().Property(x => x.TripSendReseqFlag).In(TripSendReseqFlagValue.AutoReseq, TripSendReseqFlagValue.ManualReseq))
                .OrderBy(x => x.TripSequenceNumber);
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForTripsResequencedAsync(string driverId)
        {
            var doneTrips = await GetTripsResequencedAsync(driverId);
            foreach (var trip in doneTrips.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
            }
            _notificationService.TripsResequenced();
        }

        private Task<QueryResult<ContainerChange>> GetContainerChangesAsync(string terminalId, string regionId, DateTime modifiedAfter)
        {
            QueryBuilder<ContainerChange> queryBuilder;
            if (regionId != null)
            {
                queryBuilder = new QueryBuilder<ContainerChange>().Filter(containerChange => containerChange
                    .Property(x => x.ActionDate).GreaterThan(modifiedAfter)
                    .And().Property(x => x.RegionId).EqualTo(regionId))
                    .OrderBy(x => x.ContainerNumber);
            }
            else if (terminalId != null)
            {
                queryBuilder = new QueryBuilder<ContainerChange>().Filter(containerChange => containerChange
                    .Property(x => x.ActionDate).GreaterThan(modifiedAfter)
                    .And().Property(x => x.TerminalId).EqualTo(terminalId))
                    .OrderBy(x => x.ContainerNumber);
            }
            else
            {
                queryBuilder = new QueryBuilder<ContainerChange>().Filter(containerChange => containerChange
                    .Property(x => x.ActionDate).GreaterThan(modifiedAfter))
                    .OrderBy(x => x.ContainerNumber);
            }
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForContainerChangesAsync(string terminalId, string regionId)
        {
            var modifiedAfter = _settings.GetValueOrDefault(ContainerMasterSettingsKey, default(DateTime));
            var containerChanges = await GetContainerChangesAsync(terminalId, regionId, modifiedAfter);
            foreach (var container in containerChanges.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
            }
            var maxActionDate = containerChanges.Records.Max(c => c.ActionDate);
            if (maxActionDate.HasValue)
            {
                _settings.AddOrUpdateValue(ContainerMasterSettingsKey, maxActionDate);
            }
        }

        private async Task<QueryResult<TerminalChange>> GetTerminalChangesAsync(string areaId, string regionId, DateTime modifiedAfter, string defSendOnlyYardsForArea)
        {
            QueryBuilder<TerminalChange> queryBuilder;
            if (defSendOnlyYardsForArea == Constants.Yes && areaId != null)
            {
                var areaMasterQueryBuilder = new QueryBuilder<AreaMaster>().Filter(y => y
                    .Property(x => x.AreaId).EqualTo(areaId));
                var areaMasterQuery = await _connectionService.GetConnection(ConnectionType.Online).QueryAsync(areaMasterQueryBuilder);
                var terminalList = areaMasterQuery.Records.Select(x => x.TerminalId).ToArray();
                queryBuilder = new QueryBuilder<TerminalChange>().Filter(terminalChange => terminalChange
                    .Property(x => x.ChgDateTime).GreaterThan(modifiedAfter)
                    .And().Property(x => x.TerminalId).In(terminalList))
                    .OrderBy(x => x.TerminalId);
            }
            else if (regionId != null)
            {
                queryBuilder = new QueryBuilder<TerminalChange>().Filter(terminalChange => terminalChange
                    .Property(x => x.ChgDateTime).GreaterThan(modifiedAfter)
                    .And().Property(x => x.RegionId).EqualTo(regionId))
                    .OrderBy(x => x.TerminalId);
            }
            else
            {
                queryBuilder = new QueryBuilder<TerminalChange>()
                    .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(modifiedAfter))
                    .OrderBy(x => x.TerminalId);
            }
            return await _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForTerminalChangesAsync(string areaId, string regionId)
        {
            var modifiedAfter = _settings.GetValueOrDefault(TerminalMasterSettingsKey, default(DateTime));
            // @TODO: Figure out how to get "DEFSendOnlyYardsForArea" preference from here?
            var defSendOnlyYardsForArea = "Y";
            var terminalChanges = await GetTerminalChangesAsync(areaId, regionId, modifiedAfter, defSendOnlyYardsForArea);
            foreach (var terminalChange in terminalChanges.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
            }
            var maxChgDateTime = terminalChanges.Records.Max(terminalChange => terminalChange.ChgDateTime);
            if (maxChgDateTime.HasValue)
            {
                _settings.AddOrUpdateValue(TerminalMasterSettingsKey, maxChgDateTime);
            }
        }

        private Task<QueryResult<DriverStatus>> GetForceLogoffMessageAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<DriverStatus>().Filter(driverStatus => driverStatus
                .Property(x => x.EmployeeId).EqualTo(driverId)
                .And().Property(x => x.SendHHLogoffFlag).EqualTo(DriverForceLogoffValue.Ready)
                .And().Property(x => x.Status).NotIn(DriverStatusSRConstants.Disconnected, DriverStatusSRConstants.Ready));
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForceLogoffAsync(string driverId)
        {
            var forceLogoff = await GetForceLogoffMessageAsync(driverId);
            // @TODO: Show ACR.UserDialog here.
        }

        private Task<QueryResult<Messages>> GetMessagesAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Messages>().Filter(message => message
                .Property(x => x.ReceiverId).EqualTo(driverId)
                .And().Property(x => x.Processed).EqualTo(Constants.No)
                .And().Property(x => x.DeleteFlag).EqualTo(Constants.No))
                .OrderBy(x => x.MsgId);
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task PollForMessagesAsync(string driverId)
        {
            var messages = await GetMessagesAsync(driverId);
            foreach (var message in messages.Records)
            {
                // @TODO: Update local database.
                // @TODO: Send MvxMessage.
                _notificationService.Message(message);
            }
        }
    }
}