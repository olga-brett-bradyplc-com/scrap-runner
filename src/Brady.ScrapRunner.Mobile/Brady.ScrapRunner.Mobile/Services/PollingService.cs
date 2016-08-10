namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using BWF.DataServices.Domain.Models;
    using BWF.DataServices.PortableClients;
    using Domain;
    using Domain.Enums;
    using Domain.Models;
    using Domain.Process;
    using Interfaces;
    using Messages;
    using Models;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Core.Views;
    using MvvmCross.Platform;
    using MvvmCross.Plugins.Messenger;
    using ViewModels;

    public class PollingService : IPollingService
    {
        private readonly IConnectionService _connectionService;
        private readonly INotificationService _notificationService;
        private readonly IMvxMessenger _mvxMessenger;
        private readonly IMessagesService _messagesService;
        private readonly ITerminalService _terminalService;
        private readonly IContainerService _containerService;
        private readonly ITripService _tripService;
        private readonly IDriverService _driverService;
        private readonly IPreferenceService _preferenceService;
        private readonly ICustomerService _customerService;

        public PollingService(IConnectionService connectionService, 
            INotificationService notificationService, 
            IMvxMessenger mvxMessenger, 
            IMessagesService messagesService, 
            ITerminalService terminalService, 
            IContainerService containerService, 
            ITripService tripService, 
            IDriverService driverService, 
            IPreferenceService preferenceService, 
            ICustomerService customerService)
        {
            _connectionService = connectionService;
            _notificationService = notificationService;
            _mvxMessenger = mvxMessenger;
            _messagesService = messagesService;
            _terminalService = terminalService;
            _containerService = containerService;
            _tripService = tripService;
            _driverService = driverService;
            _preferenceService = preferenceService;
            _customerService = customerService;
        }

        public async Task PollForChangesAsync(string driverId, string terminalId, string regionId, string areaId)
        {
            try
            {
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
            catch (Exception e)
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Caught Exception inside PollForChangesAsync: {e.Message}");
            }
        }

        private Task UpdateTripSendFlagAsync(IEnumerable<Trip>  trips, TripSendFlagValue tripSendFlagValue)
        {
            var updateChangeSet = new ChangeSet<string, Trip>();
            foreach (var trip in trips)
            {
                trip.TripSendFlag = tripSendFlagValue;
                updateChangeSet.AddUpdate(trip.Id, trip);
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Set Trip {trip.TripNumber} TripSendFlag to {tripSendFlagValue}");
            }
            return _connectionService.GetConnection(ConnectionType.Online).ProcessChangeSetAsync(updateChangeSet);
        }

        private async Task PollForTripsAfterLoginAsync(string employeeId)
        {
            var tripInfoProcess = new TripInfoProcess
            {
                EmployeeId = employeeId,
                SendOnlyNewModTrips = Constants.Yes
            };
            var tripInfoProcessChangeSet = await _connectionService.GetConnection(ConnectionType.Online)
                .UpdateAsync(tripInfoProcess, requeryUpdated: false);
            if (!tripInfoProcessChangeSet.WasSuccessful)
            {
                Mvx.TaggedWarning(Constants.ScrapRunner, $"ProcessTripInfoAsync {employeeId} failed");
                return;
            }
            if (tripInfoProcessChangeSet.Item?.Trips?.Count > 0)
                await _tripService.UpdateTrips(tripInfoProcessChangeSet.Item.Trips);
            if (tripInfoProcessChangeSet.Item?.TripSegments?.Count > 0)
                await _tripService.UpdateTripSegments(tripInfoProcessChangeSet.Item.TripSegments);
            if (tripInfoProcessChangeSet.Item?.TripSegmentContainers?.Count > 0)
                await _tripService.UpdateTripSegmentContainers(tripInfoProcessChangeSet.Item.TripSegmentContainers);
            if (tripInfoProcessChangeSet.Item?.CustomerCommodities?.Count > 0)
                await _customerService.UpdateCustomerCommodity(tripInfoProcessChangeSet.Item.CustomerCommodities);
            if (tripInfoProcessChangeSet.Item?.CustomerDirections?.Count > 0)
                await _customerService.UpdateCustomerDirections(tripInfoProcessChangeSet.Item.CustomerDirections);
            if (tripInfoProcessChangeSet.Item?.CustomerLocations?.Count > 0)
                await _customerService.UpdateCustomerLocation(tripInfoProcessChangeSet.Item.CustomerLocations);
            if (tripInfoProcessChangeSet.Item?.CustomerMasters?.Count > 0)
                await _customerService.UpdateCustomerMaster(tripInfoProcessChangeSet.Item.CustomerMasters);
            if (tripInfoProcessChangeSet.Item?.Trips?.Count > 0)
            {
                var mappedTrips = Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(tripInfoProcessChangeSet.Item.Trips);
                foreach (var trip in mappedTrips)
                {
                    var isNewTrip = await _tripService.FindTripAsync(trip.TripNumber) == null;
                    var tripContext = isNewTrip ? TripNotificationContext.New : TripNotificationContext.Modified;
                    if (isNewTrip)
                    {
                        await _tripService.CreateTripAsync(trip);
                        ShowTripNotificationActivity(trip.TripNumber, TripNotificationContext.New);
                    }
                    else
                    {
                        await _tripService.UpdateTripAsync(trip);
                        ShowTripNotificationActivity(trip.TripNumber, TripNotificationContext.Modified);
                    }
                    await _notificationService.TripAsync(trip, tripContext);
                    Mvx.TaggedTrace(Constants.ScrapRunner, $"Found {tripContext} Trip {trip.TripNumber}");
                    _mvxMessenger.Publish(new TripNotificationMessage(this)
                    {
                        Context = tripContext,
                        Trip = trip
                    });
                }
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
            if (canceledTrips.TotalCount == 0) return;
            await UpdateTripSendFlagAsync(canceledTrips.Records, TripSendFlagValue.CanceledSent);
            var mappedTrips = Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(canceledTrips.Records);
            foreach (var trip in mappedTrips)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Trip {trip.TripNumber} was canceled by dispatch");
                await _tripService.UpdateTripAsync(trip);
                await _notificationService.TripAsync(trip, TripNotificationContext.Canceled);
                _mvxMessenger.Publish(new TripNotificationMessage(this)
                {
                    Context = TripNotificationContext.Canceled,
                    Trip = trip
                });
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

        private Task ClearPrevTripAsync(QueryResult<Trip> trips)
        {
            var updateChangeSet = new ChangeSet<string, Trip>();
            foreach (var trip in trips.Records)
            {
                trip.TripDriverIdPrev = null;
                trip.TripStatusPrev = null;
                updateChangeSet.AddUpdate(trip.Id, trip);
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Trip {trip.TripNumber} TripDriverIdPrev, TripStatusPrev set to null");
            }
            return _connectionService.GetConnection(ConnectionType.Online).ProcessChangeSetAsync(updateChangeSet);
        }

        private async Task PollForTripsUnassignedAsync(string driverId)
        {
            var unassignedTrips = await GetTripsUnassignedAsync(driverId);
            if (unassignedTrips.TotalCount == 0) return;
            await ClearPrevTripAsync(unassignedTrips);
            var mappedTrips = Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(unassignedTrips.Records);
            foreach (var trip in mappedTrips)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Trip {trip.TripNumber} was unassigned by dispatch");
                await _tripService.UpdateTripAsync(trip);
                await _notificationService.TripAsync(trip, TripNotificationContext.Unassigned);
                _mvxMessenger.Publish(new TripNotificationMessage(this)
                {
                    Context = TripNotificationContext.Unassigned,
                    Trip = trip
                });
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
            if (doneTrips.TotalCount == 0) return;
            await ClearPrevTripAsync(doneTrips);
            var mappedTrips = Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(doneTrips.Records);
            foreach (var trip in mappedTrips)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Trip {trip.TripNumber} marked done by dispatch");
                await _tripService.UpdateTripAsync(trip);
                await _notificationService.TripAsync(trip, TripNotificationContext.MarkedDone);
                _mvxMessenger.Publish(new TripNotificationMessage(this)
                {
                    Context = TripNotificationContext.MarkedDone,
                    Trip = trip
                });
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

        private Task AckTripResequenceAsync(QueryResult<Trip> trips)
        {
            var updateChangeSet = new ChangeSet<string, Trip>();
            foreach (var trip in trips.Records)
            {
                trip.TripSendReseqFlag = TripSendReseqFlagValue.ReseqSent;
                updateChangeSet.AddUpdate(trip.TripNumber, trip);
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Trip {trip.TripNumber} TripSendReseqFlag set to {trip.TripSendReseqFlag}");
            }
            return _connectionService.GetConnection(ConnectionType.Online).ProcessChangeSetAsync(updateChangeSet);
        }

        private async Task PollForTripsResequencedAsync(string driverId)
        {
            var resequencedTrips = await GetTripsResequencedAsync(driverId);
            if (resequencedTrips.TotalCount == 0) return;
            await AckTripResequenceAsync(resequencedTrips);
            var mappedTrips = Mapper.Map<List<Trip>, List<TripModel>>(resequencedTrips.Records);
            for (var i = 0; i < mappedTrips.Count; i++)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Trip {mappedTrips[i].TripNumber} resequenced {resequencedTrips.Records[i].TripSendReseqFlag}");
                await _tripService.UpdateTripAsync(mappedTrips[i]);
                if (resequencedTrips.Records[i].TripSendReseqFlag != TripSendReseqFlagValue.ManualReseq) continue;
                await _notificationService.TripsResequencedAsync();
                _mvxMessenger.Publish(new TripResequencedMessage(this));
            }
        }

        private Task<QueryResult<ContainerChange>> GetContainerChangesAsync(string terminalId, string regionId)
        {
            QueryBuilder<ContainerChange> queryBuilder;
            if (regionId != null)
            {
                queryBuilder = new QueryBuilder<ContainerChange>().Filter(containerChange => containerChange
                    .Property(x => x.RegionId).EqualTo(regionId))
                    .OrderBy(x => x.ContainerNumber);
            }
            else if (terminalId != null)
            {
                queryBuilder = new QueryBuilder<ContainerChange>().Filter(containerChange => containerChange
                    .Property(x => x.TerminalId).EqualTo(terminalId))
                    .OrderBy(x => x.ContainerNumber);
            }
            else
            {
                queryBuilder = new QueryBuilder<ContainerChange>()
                    .OrderBy(x => x.ContainerNumber);
            }
            return _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private Task<QueryResult<ContainerChange>> GetContainerChangesAfterAsync(string terminalId, string regionId, DateTime modifiedAfter)
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
            var containerMasterDate = await _driverService.GetContainerMasterDateTimeAsync();
            QueryResult<ContainerChange> containerChanges;
            if (!containerMasterDate.HasValue)
                containerChanges = await GetContainerChangesAsync(terminalId, regionId);
            else
                containerChanges = await GetContainerChangesAfterAsync(terminalId, regionId, containerMasterDate.Value);
            if (!containerChanges.Records.Any()) return;

            await _containerService.UpdateContainerChangeIntoMaster(containerChanges.Records);
            await _driverService.UpdateContainerMasterDateTimeAsync(containerChanges.Records.Max(containerChange => containerChange.ActionDate));

            foreach (var containerChange in containerChanges.Records)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"ContainerChange {containerChange.ActionFlag} {containerChange.Id} at {containerChange.ActionDate}");
                _mvxMessenger.Publish(new ContainerChangeMessage(this) { Change = containerChange });
            }
        }

        private async Task<QueryResult<TerminalChange>> GetTerminalChangesAsync(string areaId, string regionId, string defSendOnlyYardsForArea)
        {
            QueryBuilder<TerminalChange> queryBuilder;
            if (defSendOnlyYardsForArea == Constants.Yes && areaId != null)
            {
                var areaMasterQueryBuilder = new QueryBuilder<AreaMaster>().Filter(y => y
                    .Property(x => x.AreaId).EqualTo(areaId));
                var areaMasterQuery = await _connectionService.GetConnection(ConnectionType.Online).QueryAsync(areaMasterQueryBuilder);
                var terminalList = areaMasterQuery.Records.Select(x => x.TerminalId).ToArray();
                queryBuilder = new QueryBuilder<TerminalChange>().Filter(terminalChange => terminalChange
                    .Property(x => x.TerminalId).In(terminalList))
                    .OrderBy(x => x.TerminalId);
            }
            else if (regionId != null)
            {
                queryBuilder = new QueryBuilder<TerminalChange>().Filter(terminalChange => terminalChange
                    .Property(x => x.RegionId).EqualTo(regionId))
                    .OrderBy(x => x.TerminalId);
            }
            else
            {
                queryBuilder = new QueryBuilder<TerminalChange>()
                    .OrderBy(x => x.TerminalId);
            }
            return await _connectionService.GetConnection(ConnectionType.Online).QueryAsync(queryBuilder);
        }

        private async Task<QueryResult<TerminalChange>> GetTerminalChangesAfterAsync(string areaId, string regionId, DateTime modifiedAfter, string defSendOnlyYardsForArea)
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
            var terminalMasterDateTime = await _driverService.GetTerminalMasterDateTimeAsync();
            var defSendOnlyYardsForArea = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFSendOnlyYardsForArea);
            QueryResult<TerminalChange> terminalChanges;
            if (!terminalMasterDateTime.HasValue)
                terminalChanges = await GetTerminalChangesAsync(areaId, regionId, defSendOnlyYardsForArea);
            else
                terminalChanges = await GetTerminalChangesAfterAsync(areaId, regionId, terminalMasterDateTime.Value, defSendOnlyYardsForArea);
            if (!terminalChanges.Records.Any()) return;

            await _terminalService.UpdateTerminalChangeIntoMaster(terminalChanges.Records);
            await _driverService.UpdateTerminalMasterDateTimeAsync(terminalChanges.Records.Max(terminalChange => terminalChange.ChgDateTime));

            foreach (var terminalChange in terminalChanges.Records)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"TerminalChange {terminalChange.ChgActionFlag} {terminalChange.Id} at {terminalChange.ChgDateTime}");
                _mvxMessenger.Publish(new TerminalChangeMessage(this) { Change = terminalChange });
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

        private Task AckForceLogoffAsync(DriverStatus driverStatus)
        {
            driverStatus.SendHHLogoffFlag = DriverForceLogoffValue.SentToDriver;
            Mvx.TaggedTrace(Constants.ScrapRunner, $"{driverStatus.EmployeeId} DriverStatus SendHHLogoffFlag set to {driverStatus.SendHHLogoffFlag}");
            return _connectionService.GetConnection(ConnectionType.Online).UpdateAsync(driverStatus);
        }

        private async Task PollForceLogoffAsync(string driverId)
        {
            var forceLogoff = await GetForceLogoffMessageAsync(driverId);
            if (!forceLogoff.Records.Any()) return;
            await AckForceLogoffAsync(forceLogoff.Records.First());
            _mvxMessenger.Publish(new ForceLogoffMessage(this));
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

        private Task ProcessMessagesAsync(QueryResult<Messages> messages)
        {
            var updateChangeSet = new ChangeSet<int, Messages>();
            foreach (var message in messages.Records)
            {
                message.Processed = Constants.Yes;
                updateChangeSet.AddUpdate(message.Id, message);
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Processed Message {message.Id}");
            }
            return _connectionService.GetConnection(ConnectionType.Online).ProcessChangeSetAsync(updateChangeSet);
        }

        private async Task PollForMessagesAsync(string driverId)
        {
            var messages = await GetMessagesAsync(driverId);
            if (messages.TotalCount == 0) return;
            await ProcessMessagesAsync(messages);
            var mappedMessages = Mapper.Map<IEnumerable<Messages>, IEnumerable<MessagesModel>>(messages.Records);
            foreach (var message in mappedMessages)
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, $"New Message {message.MsgId} sent by {message.SenderName}");
                await _messagesService.UpsertMessageAsync(message);
                await _notificationService.MessageAsync(message);
                ShowMessageNotificationActivity(message.MsgId.Value);
                _mvxMessenger.Publish(new NewMessagesMessage(this) { Message = message });
            }
        }

        private void ShowViewModel<TViewModel>(IDictionary<string, string> parameterValues) where TViewModel : BaseViewModel
        {
            var request = MvxViewModelRequest<TViewModel>.GetDefaultRequest();
            request.ParameterValues = parameterValues;
            var viewDispatcher = Mvx.Resolve<IMvxViewDispatcher>();
            viewDispatcher.ShowViewModel(request);
        }

        private void ShowTripNotificationActivity(string tripNumber, TripNotificationContext context)
        {
            ShowViewModel<TripNotificationViewModel>(new Dictionary<string, string>
            {
               { "tripNumber", tripNumber },
               { "notificationContext", context.ToString() }
            });
        }

        private void ShowMessageNotificationActivity(int messageId)
        {
            ShowViewModel<MessageNotificationViewModel>(new Dictionary<string, string>
            {
                {"messageId", messageId.ToString()}
            });
        }
    }
}