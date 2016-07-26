using System;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.ViewModels;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using MvvmCross.Core.Platform;

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

        Task<int> CreateTripAsync(TripModel trip);

        Task<int> CreateTripSegmentAsync(TripSegmentModel tripSegment);

        Task<int> CreateTripSegmentContainerAsync(TripSegmentContainerModel tripSegmentContainer);

        Task<int> UpdateTripAsync(TripModel trip);

        Task<int> UpdateTripSegmentAsync(TripSegmentModel tripSegment);

        Task<int> UpdateTripSegmentContainerAsync(TripSegmentContainerModel container);

        Task UpdateYards(IEnumerable<TerminalMaster> yards);

        Task<TripModel> FindTripAsync(string tripNumber);

        Task<List<TripModel>> FindTripsAsync();

        Task<TripModel> FindNextTripAsync();

        Task<List<TripSegmentModel>> FindAllSegmentsForTripAsync(string tripNumber);

        Task<List<TripSegmentContainerModel>> FindAllContainersForTripSegmentAsync(string tripNumber,
            string tripSegNumber);

        Task<TripSegmentModel> FindTripSegmentInfoAsync(string tripNumber, string tripSegmentNumber);

        Task<List<TripSegmentModel>> FindNextTripSegmentsAsync(string tripNumber);

        Task<List<TripSegmentContainerModel>> FindNextTripSegmentContainersAsync(string tripNumber, string tripSegNo);

        Task<TripSegmentContainerModel> FindTripSegmentContainer(string tripNumber, string tripSegNo,
            int? tripSegContainerSeqNumber);



        bool IsTripLegTransaction(TripSegmentModel tripSegment);

        bool IsTripLegScale(TripSegmentModel tripSegmentContainer);

        bool IsTripLegNoScreen(TripSegmentModel tripSegmentContainer);

        bool IsTripLegTypePublicScale(TripSegmentModel tripSegmentContainer);

        bool IsTripLegDropped(TripSegmentModel tripSegment);

        bool IsTripLegLoaded(TripSegmentModel tripSegment);

        Task<bool> IsTripSequenceEnforcedAsync();

        Task<int> UpdateTripSegmentContainerWeightTimesAsync(TripSegmentContainerModel container, DateTime? gsWt, DateTime? gs2Wt, DateTime? trWt);

        Task<int> UpdateTripSegmentContainerLongLatAsync(TripSegmentContainerModel container, int? latitude, int? longitude);
        
        Task<int> CompleteTripAsync(string tripNumber);

        Task<int> CompleteTripSegmentAsync(TripSegmentModel tripSegment);

        Task<int> CompleteTripSegmentContainerAsync(TripSegmentContainerModel container);

        Task<YardModel> FindYardInfo(string terminalId);

        Task<int> MarkExceptionTripAsync(string tripNumber);

        Task<int> MarkExceptionTripSegmentAsync(TripSegmentModel tripSegment);

        Task<int> MarkExceptionTripSegmentContainerAsync(TripSegmentContainerModel container, string reviewReason);

        Task PropagateContainerUpdates(string tripNumber, IEnumerable<Grouping<TripSegmentModel, TripSegmentContainerModel>> containers);



        Task<ChangeResultWithItem<DriverSegmentDoneProcess>> ProcessTripSegmentDoneAsync(DriverSegmentDoneProcess driverContActionProcess);

        Task<ChangeResultWithItem<TripInfoProcess>> ProcessTripInfoAsync(TripInfoProcess tripInfoProcess);

        Task<ChangeResultWithItem<DriverContainerActionProcess>> ProcessContainerActionAsync(
            DriverContainerActionProcess driverContainerAction);

        Task<ChangeResultWithItem<DriverImageProcess>> ProcessDriverImageAsync(
            DriverImageProcess driverImage);

        Task<ChangeResultWithItem<DriverTripAckProcess>> ProcessDriverTripAck(
            DriverTripAckProcess driverAck);
        Task<List<TripSegmentModel>> FindCustomerSegments(string custHostCode);
    }
}
