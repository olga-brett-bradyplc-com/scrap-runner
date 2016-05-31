using System;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;

    public class TripService : ITripService
    {
        private readonly IConnectionService<DataServiceClient> _connection;
        private readonly IRepository<PreferenceModel> _preferenceRepository;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

        public TripService(
            IConnectionService<DataServiceClient> connection,
            IRepository<PreferenceModel> preferenceRepository,
            IRepository<TripModel> tripRepository,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _connection = connection;
            _preferenceRepository = preferenceRepository;
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
        }

        /// <summary>
        /// Takes a list of trips provided by the server, and inserts into the local Trip SQLite DB
        /// </summary>
        /// <param name="trips"></param>
        /// <returns></returns>
        public Task UpdateTrips(IEnumerable<Trip> trips)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Trip>, IEnumerable<TripModel>>(trips);
            return _tripRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Takes a list of trip segments provided by the server, and inserts into the local TripSegment SQLite DB
        /// </summary>
        /// <param name="tripSegments"></param>
        /// <returns></returns>
        public Task UpdateTripSegments(IEnumerable<TripSegment> tripSegments)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<TripSegment>, IEnumerable<TripSegmentModel>>(tripSegments);
            return _tripSegmentRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Takes a list of trip segment containers provided by the server, and inserts into local TripSegmentContainer DB
        /// </summary>
        /// <param name="tripSegmentContainers"></param>
        /// <returns></returns>
        public Task UpdateTripSegmentContainers(IEnumerable<TripSegmentContainer> tripSegmentContainers)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<TripSegmentContainer>, IEnumerable<TripSegmentContainerModel>>(tripSegmentContainers);
            return _tripSegmentContainerRepository.InsertRangeAsync(mapped);
        }

        public async Task<ChangeResultWithItem<TripInfoProcess>> FindTripsRemoteAsync(TripInfoProcess tripInfoProcess)
        {
            var tripProcess = await _connection.GetConnection().UpdateAsync(tripInfoProcess, requeryUpdated: false);
            return tripProcess;
        }

        /// <summary>
        /// Retrieve whether trip order in enforced via preferences table
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsTripSequenceEnforcedAsync()
        {
            var preference = await _preferenceRepository.FindAsync(p => p.Parameter == "DEFEnforceSeqProcess");
            return preference != null && preference.ParameterValue == Constants.Yes;
        }

        /// <summary>
        /// Check to see if first segment of given leg contains a basic trip type that
        /// warrants sending user to through the transaction summary screen.
        /// 
        /// Should note that we don't need to evaluate whether there are multiple trip segments
        /// that should be processed together, as they'll automatically be picked up on the 
        /// transaction screen together.
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns>bool</returns>
        public async Task<bool> IsTripLegTransactionAsync(string tripNumber)
        {
            var segment = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    &&
                    (ts.TripSegStatus == TripSegStatusConstants.Pending ||
                     ts.TripSegStatus == TripSegStatusConstants.Missed))
                .OrderBy(ts => ts.TripSegNumber).FirstOrDefaultAsync();

            return segment?.TripSegType == BasicTripTypeConstants.DropEmpty ||
                   segment?.TripSegType == BasicTripTypeConstants.DropFull ||
                   segment?.TripSegType == BasicTripTypeConstants.PickupEmpty ||
                   segment?.TripSegType == BasicTripTypeConstants.PickupFull ||
                   segment?.TripSegType == BasicTripTypeConstants.Load ||
                   segment?.TripSegType == BasicTripTypeConstants.Unload ||
                   segment?.TripSegType == BasicTripTypeConstants.Respot;
        }

        /// <summary>
        /// Check to see if first segment of given leg contains a basic trip type that
        /// warrants sending user to through the scale summary screen.
        /// 
        /// Should note that we don't need to evaluate whether there are multiple trip segments
        /// that should be processed together, as they'll automatically be picked up on the 
        /// scale screen together.
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns>bool</returns>
        public async Task<bool> IsTripLegScaleAsync(string tripNumber)
        {
            var segment = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    &&
                    (ts.TripSegStatus == TripSegStatusConstants.Pending ||
                     ts.TripSegStatus == TripSegStatusConstants.Missed))
                .OrderBy(ts => ts.TripSegNumber).FirstOrDefaultAsync();

            return segment?.TripSegType == BasicTripTypeConstants.Scale ||
                   segment?.TripSegType == BasicTripTypeConstants.ReturnYard;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<bool> IsTripLegNoScreenAsync(string tripNumber)
        {
            var segment = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    &&
                    (ts.TripSegStatus == TripSegStatusConstants.Pending ||
                     ts.TripSegStatus == TripSegStatusConstants.Missed))
                .OrderBy(ts => ts.TripSegNumber).FirstOrDefaultAsync();

            return segment?.TripSegType == BasicTripTypeConstants.YardWork ||
                   segment?.TripSegType == BasicTripTypeConstants.ReturnYardNC;
        }

        /// <summary>
        /// Check to see if arriving segment of given trip type is W (Scale) type
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns>bool</returns>
        public async Task<bool> IsTripLegAcctTypeScale(string tripNumber)
        {
            var segment = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    &&
                    (ts.TripSegDestCustType == CustomerTypeConstants.Scale))
                .OrderBy(ts => ts.TripSegNumber).FirstOrDefaultAsync();

            return segment?.TripSegType == BasicTripTypeConstants.Scale ||
                   segment?.TripSegType == BasicTripTypeConstants.ReturnYard;
        }

        /// <summary>
        /// Find a given trip from the local Trip SQLite DB
        /// </summary>
        /// <param name="tripNumber">The trip you want to find</param>
        /// <returns></returns>
        public async Task<TripModel> FindTripAsync(string tripNumber)
        {
            return await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
        }

        /// <summary>
        /// Find all valid trips, ordered by TripSequenceNumber, from SQLite Trip DB
        /// </summary>
        /// <returns></returns>
        public async Task<List<TripModel>> FindTripsAsync()
        {
            var sortedTrips = await _tripRepository.AsQueryable()
                .Where(t => t.TripStatus == TripStatusConstants.Pending || t.TripStatus == TripStatusConstants.Missed)
                .OrderBy(t => t.TripSequenceNumber)
                .ToListAsync();
            return sortedTrips;
        }

        /// <summary>
        /// Find the next trip from the Trip SQLite DB
        /// </summary>
        /// <returns></returns>
        public async Task<TripModel> FindNextTripAsync()
        {
            var sortedTrips = await _tripRepository.AsQueryable()
                .Where(t => t.TripStatus == TripStatusConstants.Pending || t.TripStatus == TripStatusConstants.Missed)
                .OrderBy(t => t.TripSequenceNumber)
                .ToListAsync();
            return sortedTrips.FirstOrDefault();
        }

        /// <summary>
        /// Find all trip segments for the given leg of a trip
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<List<TripSegmentModel>> FindNextTripSegmentsAsync(string tripNumber)
        {
            var segments = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    && (ts.TripSegStatus == TripSegStatusConstants.Pending || ts.TripSegStatus == TripSegStatusConstants.Missed))
                .OrderBy(ts => ts.TripSegNumber)
                .ToListAsync();
            if (!segments.Any())
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Couldn't find next segments for trip {tripNumber}.");
                return Enumerable.Empty<TripSegmentModel>().ToList();
            }
            var firstCustomer = segments.First().TripSegDestCustHostCode;
            return segments.TakeWhile(ts => ts.TripSegDestCustHostCode == firstCustomer).ToList();
        }

        /// <summary>
        /// Find all trip segment containers for the given trip leg.
        /// @TODO : 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNo"></param>
        /// <returns></returns>
        public async Task<List<TripSegmentContainerModel>> FindNextTripSegmentContainersAsync(string tripNumber, string tripSegNo)
        {
            var containers = await _tripSegmentContainerRepository.AsQueryable()
                .Where(tscm => tscm.TripNumber == tripNumber
                               && tscm.TripSegNumber == tripSegNo).ToListAsync();
            if (!containers.Any())
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Couldn't find next containers for trip {tripNumber}.");
                return Enumerable.Empty<TripSegmentContainerModel>().ToList();
            }
            return containers.TakeWhile(tscm => tscm.TripSegContainerComplete != Constants.Yes).ToList();
        }

        /// <summary>
        /// Find a specific trip segment container
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNo"></param>
        /// <param name="tripSegContainerSeqNumber"></param>
        /// <returns></returns>
        public async Task<TripSegmentContainerModel> FindTripSegmentContainer(string tripNumber, string tripSegNo,
            int? tripSegContainerSeqNumber)
        {
            // We're using TripSegContainerSeqNumber instead of TripSegContainerNumber because the latter of the two
            // can be null, i.e. it hasn't been set/scanned yet
            var container = await _tripSegmentContainerRepository.AsQueryable()
                .Where(
                    tscm =>
                        tscm.TripNumber == tripNumber && tscm.TripSegNumber == tripSegNo &&
                        tscm.TripSegContainerSeqNumber == tripSegContainerSeqNumber).ToListAsync();
            return container.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNo"></param>
        /// <param name="tripSegContainerSeqNumber"></param>
        /// <param name="tripSegContainerNumer"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripSegmentContainerAsync(string tripNumber, string tripSegNo, short tripSegContainerSeqNumber, string tripSegContainerNumer)
        {
            // @TODO : Not complete
            var container = await _tripSegmentContainerRepository.AsQueryable()
                .Where(
                    tscm =>
                        tscm.TripNumber == tripNumber && tscm.TripSegNumber == tripSegNo && tscm.TripSegContainerSeqNumber == tripSegContainerSeqNumber).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(container.TripSegContainerNumber))
                container.TripSegContainerNumber = tripSegContainerNumer;

            container.TripSegContainerActionDateTime = DateTime.Now;
            container.TripSegContainerComplete = Constants.Yes;

            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNo"></param>
        /// <param name="tripSegContainerNumber"></param>
        /// <param name="gsWt"></param>
        /// <param name="gs2Wt"></param>
        /// <param name="trWt"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripSegmentContainerWeightTimesAsync(string tripNumber, string tripSegNo, string tripSegContainerNumber, DateTime? gsWt, DateTime? gs2Wt, DateTime? trWt)
        {
            // @TODO : Not complete
            var container = await _tripSegmentContainerRepository.AsQueryable()
                .Where(
                    tscm =>
                        tscm.TripNumber == tripNumber && tscm.TripSegNumber == tripSegNo &&
                        tscm.TripSegContainerNumber == tripSegContainerNumber).FirstOrDefaultAsync();

            container.WeightGrossDateTime = gsWt;
            container.WeightGross2ndDateTime = gs2Wt;
            container.WeightTareDateTime = trWt;

            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNo"></param>
        /// <param name="tripSegContainerNumber"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripSegmentContainerLongLatAsync(string tripNumber, string tripSegNo, string tripSegContainerNumber, int? latitude, int? longitude)
        {

            // @TODO : Not complete
            var container = await _tripSegmentContainerRepository.AsQueryable()
                .Where(
                    tscm =>
                        tscm.TripNumber == tripNumber && tscm.TripSegNumber == tripSegNo &&
                        tscm.TripSegContainerNumber == tripSegContainerNumber).FirstOrDefaultAsync();

            container.TripSegContainerLongitude = longitude;
            container.TripSegContainerLatitude = latitude;

            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        /// <summary>
        /// Complete a given trip
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripAsync(string tripNumber)
        {
            // @TODO : Implement remote process once it's completed
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            trip.TripStatus = TripStatusConstants.Done;
            return await _tripRepository.UpdateAsync(trip);
        }

        /// <summary>
        /// Complete a given leg of a trip; @TODO : This is not implemented correctly?
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripSegmentAsync(string tripNumber, string tripSegNumber)
        {
            var tripSegment =
                await _tripSegmentRepository.FindAsync(
                        ts => ts.TripNumber == tripNumber && ts.TripSegNumber == tripSegNumber);
            tripSegment.TripSegStatus = TripSegStatusConstants.Done;
            return await _tripSegmentRepository.UpdateAsync(tripSegment);
        }

        public async Task<ChangeResultWithItem<DriverContainerActionProcess>> ProcessPublicScaleAsync(
           DriverContainerActionProcess driverContainerActionProcess)
        {
            var publicScaleProcess =
                await _connection.GetConnection().UpdateAsync(driverContainerActionProcess, requeryUpdated: false);
            return publicScaleProcess;

        }
        public async Task<ChangeResultWithItem<DriverSegmentDoneProcess>> ProcessContainerDoneAsync(
           DriverSegmentDoneProcess driverSegmentDoneProcess)
        {
            var containerProcess =
                await _connection.GetConnection().UpdateAsync(driverSegmentDoneProcess, requeryUpdated: false);
            return containerProcess;

        }
    }
}