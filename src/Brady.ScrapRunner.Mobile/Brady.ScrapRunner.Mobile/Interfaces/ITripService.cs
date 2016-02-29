namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface ITripService
    {
        Task<bool> IsTripSequenceEnforcedAsync();

        Task<TripModel> FindNextTripAsync();

        Task<List<TripSegmentModel>> FindNextTripSegmentsAsync(string tripNumber);
    }
}
