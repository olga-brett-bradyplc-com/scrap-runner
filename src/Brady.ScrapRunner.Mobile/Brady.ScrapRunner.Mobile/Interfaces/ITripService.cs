﻿using System;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;

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

        Task<ChangeResultWithItem<TripInfoProcess>> FindTripsRemoteAsync(TripInfoProcess tripInfoProcess);

        Task<bool> IsTripLegTransactionAsync(string tripNumber);

        Task<bool> IsTripLegScaleAsync(string tripNumber);

        Task<bool> IsTripLegNoScreenAsync(string tripNumber);

        Task<bool> IsTripSequenceEnforcedAsync();

        Task<TripModel> FindTripAsync(string tripNumber);

        Task<List<TripModel>> FindTripsAsync();

        Task<TripModel> FindNextTripAsync();

        Task<TripSegmentModel> FindTripSegmentInfoAsync(string tripNumber, string tripSegmentNumber);

        Task<List<TripSegmentModel>> FindNextTripSegmentsAsync(string tripNumber);

        Task<List<TripSegmentContainerModel>> FindNextTripSegmentContainersAsync(string tripNumber, string tripSegNo);

        Task<TripSegmentContainerModel> FindTripSegmentContainer(string tripNumber, string tripSegNo,
            int? tripSegContainerSeqNumber);

        Task<int> UpdateTripSegmentContainerWeightTimesAsync(string tripNumber, string tripSegNo,
            string tripSegContainerNumber, DateTime? gsWt, DateTime? gs2Wt, DateTime? trWt);

        Task<int> UpdateTripSegmentContainerLongLatAsync(string tripNumber, string tripSegNo,
            string tripSegContainerNumber, int? latitude, int? longitude);

        Task<int> CompleteTripSegmentContainerAsync(string tripNumber, string tripSegNo, short tripSegContainerSeqNumber, string tripSegContainerNumber);

        Task<int> CompleteTripAsync(string tripNumber);

        Task<int> CompleteTripSegmentAsync(string tripNumber, string tripSegNo);
        Task<bool> IsTripLegAcctTypeScale(string tripNumber);

        Task<ChangeResultWithItem<DriverContainerActionProcess>> ProcessPublicScaleAsync(DriverContainerActionProcess driverContActionProcess);
        Task<ChangeResultWithItem<DriverSegmentDoneProcess>> ProcessContainerDoneAsync(DriverSegmentDoneProcess driverContActionProcess);

        Task<YardModel> FindYardInfo(string terminalId);

    }
}
