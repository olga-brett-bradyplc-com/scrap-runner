using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface ITripService
    {
        Task UpdateTrips(IEnumerable<Trip> trips);

        Task UpdateTripSegments(IEnumerable<TripSegment> tripSegments);

        Task UpdateTripSegmentContainers(IEnumerable<TripSegmentContainer> tripSegmentContainers);

        Task<bool> IsTripSequenceEnforcedAsync();

        Task<TripModel> FindTripAsync(string tripNumber);

        Task<List<TripModel>> FindTripsAsync();

        Task<TripModel> FindNextTripAsync();

        Task<List<TripSegmentModel>> FindNextTripSegmentsAsync(string tripNumber);

        Task<List<TripSegmentContainerModel>> FindNextTripSegmentContainersAsync(string tripNumber, string tripSegNo);

        Task<TripSegmentContainerModel> FindTripSegmentContainer(string tripNumber, string tripSegNo,
            int? tripSegContainerSeqNumber);

        Task<int> CompleteTripAsync(string tripNumber);

        Task<int> CompleteTripSegmentAsync(string tripNumber, string tripSegNo);
    }
}
