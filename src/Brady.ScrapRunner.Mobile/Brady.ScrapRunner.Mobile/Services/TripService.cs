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

        public async Task<bool> IsTripSequenceEnforcedAsync()
        {
            var preference = await _preferenceRepository.FindAsync(p => p.Parameter == "DEFEnforceSeqProcess");
            return preference != null && preference.ParameterValue == Constants.Yes;
        }

        public async Task<TripModel> FindTripAsync(string tripNumber)
        {
            return await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
        }

        public async Task<List<TripModel>> FindTripsAsync()
        {
            var sortedTrips = await _tripRepository.AsQueryable()
                .Where(t => t.TripStatus != "D" && t.TripStatus != "X")
                .OrderBy(t => t.TripSequenceNumber)
                .ToListAsync();
            return sortedTrips;
        }

        public async Task<TripModel> FindNextTripAsync()
        {
            var sortedTrips = await _tripRepository.AsQueryable()
                .Where(t => t.TripStatus != "D" && t.TripStatus != "X")
                .OrderBy(t => t.TripSequenceNumber)
                .ToListAsync();
            return sortedTrips.FirstOrDefault();
        }

        public async Task<List<TripSegmentModel>> FindNextTripSegmentsAsync(string tripNumber)
        {
            var segments = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    && ts.TripSegStatus != "D" && ts.TripSegStatus != "X")
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

        public async Task<int> CompleteTripAsync(string tripNumber)
        {
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            await _tripRepository.DeleteAsync(trip);

            return await Task.FromResult(1);
        }

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