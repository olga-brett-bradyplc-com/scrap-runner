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

        public TripService(
            IRepository<PreferenceModel> preferenceRepository, 
            IRepository<TripModel> tripRepository, 
            IRepository<TripSegmentModel> tripSegmentRepository)
        {
            _preferenceRepository = preferenceRepository;
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
        }

        public async Task<bool> IsTripSequenceEnforcedAsync()
        {
            var preference = await _preferenceRepository.FindAsync(p => p.Parameter == "DEFEnforceSeqProcess");
            return preference != null && preference.ParameterValue == Constants.Yes;
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
    }
}