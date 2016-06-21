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

    public class PollingService : IPollingService
    {
        private readonly IConnectionService _connectionService;

        public PollingService(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public Task<QueryResult<Trip>> GetTripsAfterLoginAsync(string driverId)
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

        public Task<QueryResult<Trip>> GetTripsCanceledAsync(string driverId)
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

        public Task<QueryResult<Trip>> GetTripsUnassignedAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverIdPrev).EqualTo(driverId)
                .And().Property(x => x.TripStatusPrev).NotEqualTo(TripStatusConstants.Future).Or(x => x.TripStatusPrev).IsNull()
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Missed, TripStatusConstants.Pending)
                .Or(x => x.TripStatusPrev).In(TripStatusConstants.Missed, TripStatusConstants.Pending));
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        public Task<QueryResult<Trip>> GetTripsMarkedDoneAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverId).EqualTo(driverId)
                .And().Property(x => x.TripDriverIdPrev).IsNotNull()
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Done, TripStatusConstants.Exception, TripStatusConstants.Review, TripStatusConstants.ErrorQueue));
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        public Task<QueryResult<Trip>> GetTripsResequencedAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Trip>().Filter(trip => trip
                .Property(x => x.TripDriverId).EqualTo(driverId)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                .And().Property(x => x.TripSendReseqFlag).In(TripSendReseqFlagValue.AutoReseq, TripSendReseqFlagValue.ManualReseq))
                .OrderBy(x => x.TripSequenceNumber);
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        public Task<QueryResult<ContainerChange>> GetContainerChangesAsync(string terminalId, string regionId, DateTime modifiedAfter)
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

        public async Task<QueryResult<TerminalChange>> GetTerminalChangesAsync(string areaId, string regionId, DateTime modifiedAfter, string defSendOnlyYardsForArea)
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

        public Task<QueryResult<DriverStatus>> GetForceLogoffMessageAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<DriverStatus>().Filter(driverStatus => driverStatus
                .Property(x => x.EmployeeId).EqualTo(driverId)
                .And().Property(x => x.SendHHLogoffFlag).EqualTo(DriverForceLogoffValue.Ready)
                .And().Property(x => x.Status).NotIn(DriverStatusSRConstants.Disconnected, DriverStatusSRConstants.Ready));
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        public Task<QueryResult<Messages>> GetMessagesAsync(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) throw new ArgumentException(nameof(driverId));
            var queryBuilder = new QueryBuilder<Messages>().Filter(message => message
                .Property(x => x.ReceiverId).EqualTo(driverId)
                .And().Property(x => x.Processed).EqualTo(Constants.No)
                .And().Property(x => x.DeleteFlag).EqualTo(Constants.No))
                .OrderBy(x => x.MsgId);
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }
    }
}