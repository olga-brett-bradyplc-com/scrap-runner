using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Helpers;

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
        private readonly IRepository<PreferenceModel> _preferenceRepository;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository; 

        public TripService(
            IRepository<PreferenceModel> preferenceRepository, 
            IRepository<TripModel> tripRepository, 
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository )
        {
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
        /// Find all trip segments for the given trip leg of a trip
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
            return containers;
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
        /// Complete a given trip; @TODO : This is not implemented correctly?
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripAsync(string tripNumber)
        {
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            await _tripRepository.DeleteAsync(trip);

            return await Task.FromResult(1);
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
            var tripSegmentContainers = _tripSegmentContainerRepository.AsQueryable()
                .Where(tscm => tscm.TripNumber == tripNumber && tscm.TripSegNumber == tripSegNumber)
                .ToListAsync();

            await _tripSegmentRepository.DeleteAsync(tripSegment);
            foreach( var container in tripSegmentContainers.Result)
            {
                await _tripSegmentContainerRepository.DeleteAsync(container);
            }

            // @TODO : Fix this ...
            return await Task.FromResult(1);
        }

    }
}