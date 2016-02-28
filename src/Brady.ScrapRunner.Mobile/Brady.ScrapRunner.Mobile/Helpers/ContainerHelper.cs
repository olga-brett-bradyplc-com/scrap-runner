using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Helpers
{
    public static class ContainerHelper
    {
        public static async Task<bool> ContainersExist(
            string tripNumber,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            var tripSegments = await tripSegmentRepository.AsQueryable()
                .Where(ts => ts.TripNumber == tripNumber).ToListAsync();
            var tripSegmentContainers = await tripSegmentContainerRepository.AsQueryable()
                .Where(tsc => tsc.TripNumber == tripNumber).ToListAsync();

            return tripSegments.Any() || tripSegmentContainers.Any();
        }

        public static async Task<IEnumerable<Grouping<TripSegmentModel, TripSegmentContainerModel>>> ContainersForSegment(
            string tripNumber,
            IRepository<TripSegmentModel> tripSegmentRepository, 
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository )
        {
            // @TODO : This seems a bit clunky to determine which segments to show for each leg of the trip. Better way?
            // Grab the first avaliable segment in order to get TripSegDestCustHostCode
            // Then grab all trip segments that contain the first avaliable TripSegDestCustHostCode
            var firstAvaliableTripSegment = await tripSegmentRepository.AsQueryable()
                .Where(ts => ts.TripNumber == tripNumber).OrderBy(x => x.TripSegNumber).FirstAsync();

            var tripSegments = await tripSegmentRepository.AsQueryable()
                .Where(ts => ts.TripNumber == tripNumber && ts.TripSegDestCustHostCode == firstAvaliableTripSegment.TripSegDestCustHostCode).ToListAsync();
            var tripSegmentContainers = await tripSegmentContainerRepository.AsQueryable()
                .Where(tsc => tsc.TripNumber == tripNumber).ToListAsync();
            // Filter out containers where no trip segment number is found from the avaliable trip segments
            var filteredTripSegmentContainers =
                tripSegmentContainers.FindAll(
                    x => tripSegments.Select(y => y.TripSegNumber).ToArray().Contains(x.TripSegNumber)).OrderBy(x => x.TripSegNumber);
            var groupedContainers = from details in filteredTripSegmentContainers
                                    orderby details.TripSegNumber
                                    group details by new { details.TripNumber, details.TripSegNumber }
                                    into detailsGroup
                                    select new Grouping<TripSegmentModel, TripSegmentContainerModel>(tripSegments.Find(
                                            tsm => (tsm.TripNumber + tsm.TripSegNumber).Equals(detailsGroup.Key.TripNumber + detailsGroup.Key.TripSegNumber)
                                        ), detailsGroup);
            return groupedContainers;
        }
    }
}
