using Brady.ScrapRunner.Mobile.Helpers;

namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BWF.DataServices.Metadata.Models;
    using Domain;
    using Domain.Models;
    using Domain.Process;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;

    public class TripService : ITripService
    {
        private readonly IConnectionService _connection;
        private readonly IRepository<PreferenceModel> _preferenceRepository;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;
        private readonly IRepository<CodeTableModel> _codeTableRepository; 

        public TripService(
            IConnectionService connection,
            IRepository<PreferenceModel> preferenceRepository,
            IRepository<TripModel> tripRepository,
            IRepository<TripSegmentModel> tripSegmentRepository,
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository,
            IRepository<CodeTableModel> codeTableRepository )
        {
            _connection = connection;
            _preferenceRepository = preferenceRepository;
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            _codeTableRepository = codeTableRepository;
        }

        #region General purpose trip methods

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
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<TripSegmentContainer>, IEnumerable<TripSegmentContainerModel>>(
                    tripSegmentContainers);
            return _tripSegmentContainerRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Create a new, local trip
        /// </summary>
        /// <param name="trip"></param>
        /// <returns></returns>
        public async Task<int> CreateTripAsync(TripModel trip)
        {
            return await _tripRepository.InsertAsync(trip);
        }

        /// <summary>
        /// Create a new, local trip segment
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns></returns>
        public async Task<int> CreateTripSegmentAsync(TripSegmentModel tripSegment)
        {
            return await _tripSegmentRepository.InsertAsync(tripSegment);
        }

        /// <summary>
        /// Create a new, local trip segment container
        /// </summary>
        /// <param name="tripSegmentContainer"></param>
        /// <returns></returns>
        public async Task<int> CreateTripSegmentContainerAsync(TripSegmentContainerModel tripSegmentContainer)
        {
            return await _tripSegmentContainerRepository.InsertAsync(tripSegmentContainer);
        }

        /// <summary>
        /// Update the given trip model
        /// </summary>
        /// <param name="trip"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripAsync(TripModel trip)
        {
            return await _tripRepository.UpdateAsync(trip);
        }

        /// <summary>
        /// Update the given tripSegment
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripSegmentAsync(TripSegmentModel tripSegment)
        {
            return await _tripSegmentRepository.UpdateAsync(tripSegment);
        }

        /// <summary>
        /// Update the given trip segment container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripSegmentContainerAsync(TripSegmentContainerModel container)
        {
            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        public async Task<List<TripSegmentModel>> FindCustomerSegments(string custHostCode)
        {
            var tripSegments = await _tripSegmentRepository.AsQueryable()
                .Where(t => t.TripSegDestCustHostCode == custHostCode).ToListAsync();
            return tripSegments;
        }
        public async Task<int> UpdateGpsCustomerSegments(string custHostCode, int lat, int lon)
        {
            var tripSegments = await _tripSegmentRepository.AsQueryable()
                .Where(t => t.TripSegDestCustHostCode == custHostCode).ToListAsync();

            foreach (TripSegmentModel segment in tripSegments)
            {
                segment.TripSegEndLatitude = lat;
                segment.TripSegEndLongitude = lon;
            }
            return await _tripSegmentRepository.UpdateRangeAsync(tripSegments);
        }
        #endregion

        #region Search methods

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
                .Where(t => t.TripStatus == TripStatusConstants.Pending || t.TripStatus == TripStatusConstants.Missed || t.TripStatus == TripStatusConstants.Exception)
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
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<List<TripSegmentModel>> FindAllSegmentsForTripAsync(string tripNumber)
        {
            var tripSegments = await _tripSegmentRepository.AsQueryable()
                .Where(t => t.TripNumber == tripNumber)
                .OrderBy(t => t.TripSegNumber)
                .ToListAsync();

            if (!tripSegments.Any())
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Couldn't find segments for trip {tripNumber}.");
                return Enumerable.Empty<TripSegmentModel>().ToList();
            }

            return tripSegments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <returns></returns>
        public async Task<List<TripSegmentContainerModel>> FindAllContainersForTripSegmentAsync(string tripNumber,
            string tripSegNumber)
        {
            var tripSegmentContainers = await _tripSegmentContainerRepository.AsQueryable()
                .Where(tscm => tscm.TripNumber == tripNumber && tscm.TripSegNumber == tripSegNumber)
                .OrderBy(tscm => tscm.TripSegContainerSeqNumber)
                .ToListAsync();

            return tripSegmentContainers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegmentNumber"></param>
        /// <returns></returns>
        public async Task<TripSegmentModel> FindTripSegmentInfoAsync(string tripNumber, string tripSegmentNumber)
        {
            var segment = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    &&
                    ts.TripSegNumber == tripSegmentNumber)
                .OrderBy(ts => ts.TripSegNumber).FirstOrDefaultAsync();

            return segment;
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
                    &&
                    (ts.TripSegStatus == TripSegStatusConstants.Pending ||
                     ts.TripSegStatus == TripSegStatusConstants.Missed ||
                     ts.TripSegStatus == TripSegStatusConstants.Exception))
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
        /// This will return sequential trip segments with identical host codes and trip segment types.
        /// 
        /// Use this method instead of FindNextTripSegmentsAsync when you only want to process
        /// a certain group of segments for a given leg, e.g., a trip leg could contain DE, PF and SC,
        /// but you only want to process the DE & PF segments on the transaction screen, and the SC segment
        /// on the Scale screen, even though these segments all share the same host code 
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<List<TripSegmentModel>> FindNextTripLegSegmentsAsync(string tripNumber)
        {
            var segments = await _tripSegmentRepository.AsQueryable()
                .Where(ts =>
                    ts.TripNumber == tripNumber
                    &&
                    (ts.TripSegStatus == TripSegStatusConstants.Pending ||
                     ts.TripSegStatus == TripSegStatusConstants.Missed ||
                     ts.TripSegStatus == TripSegStatusConstants.Exception))
                .OrderBy(ts => ts.TripSegNumber)
                .ToListAsync();

            if (!segments.Any())
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Couldn't find next segments for trip {tripNumber}.");
                return Enumerable.Empty<TripSegmentModel>().ToList();
            }

            var list = new List<TripSegmentModel> { segments.FirstOrDefault() };

            // Start i at 1, since ElementAt(0) is our reference segment
            // We're doing it this way because multiple segments could share the same hostcode, but be on a different leg
            for (var i = 1; i < segments.Count; i++)
            {
                if (segments.FirstOrDefault().TripSegDestCustHostCode != segments.ElementAt(i).TripSegDestCustHostCode)
                    break;

                if (IsTripLegTransaction(segments.FirstOrDefault()) && IsTripLegTransaction(segments.ElementAt(i)))
                    list.Add(segments.ElementAt(i));
                else if (IsTripLegTypePublicScale(segments.FirstOrDefault()) && IsTripLegTypePublicScale(segments.ElementAt(i)))
                    list.Add(segments.ElementAt(i));
                else if (IsTripLegScale(segments.FirstOrDefault()) && IsTripLegScale(segments.ElementAt(i)))
                    list.Add(segments.ElementAt(i));
                else
                    break;
            }

            return list;
        }

        /// <summary>
        /// Find all trip segment containers for the given trip segment.
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNo"></param>
        /// <returns></returns>
        public async Task<List<TripSegmentContainerModel>> FindNextTripSegmentContainersAsync(string tripNumber,
            string tripSegNo)
        {
            var containers = await _tripSegmentContainerRepository.AsQueryable()
                .Where(tscm => tscm.TripNumber == tripNumber
                               && tscm.TripSegNumber == tripSegNo).ToListAsync();
            if (!containers.Any())
            {
                Mvx.TaggedError(Constants.ScrapRunner, $"Couldn't find next containers for trip {tripNumber}.");
                return Enumerable.Empty<TripSegmentContainerModel>().ToList();
            }
            return containers.ToList();
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

        #endregion

        #region Helper methods

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
        /// <param name="tripSegment"></param>
        /// <returns>bool</returns>
        public bool IsTripLegTransaction(TripSegmentModel tripSegment)
        {
            return tripSegment.TripSegType == BasicTripTypeConstants.DropEmpty ||
                   tripSegment.TripSegType == BasicTripTypeConstants.DropFull ||
                   tripSegment.TripSegType == BasicTripTypeConstants.PickupEmpty ||
                   tripSegment.TripSegType == BasicTripTypeConstants.PickupFull ||
                   tripSegment.TripSegType == BasicTripTypeConstants.Load ||
                   tripSegment.TripSegType == BasicTripTypeConstants.Unload ||
                   tripSegment.TripSegType == BasicTripTypeConstants.Respot;
        }

        /// <summary>
        /// Check to see if first segment of given leg contains a basic trip type that
        /// warrants sending user to through the scale summary screen.
        /// 
        /// Should note that we don't need to evaluate whether there are multiple trip segments
        /// that should be processed together, as they'll automatically be picked up on the 
        /// scale screen together.
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns>bool</returns>
        public bool IsTripLegScale(TripSegmentModel tripSegment)
        {
            return tripSegment.TripSegType == BasicTripTypeConstants.Scale ||
                   tripSegment.TripSegType == BasicTripTypeConstants.ReturnYard;
        }

        /// <summary>
        /// These trip types don't have an associated "next screen" and either
        /// complete the trip or go to the next segment once they've confirmed their
        /// arrival
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns></returns>
        public bool IsTripLegNoScreen(TripSegmentModel tripSegment)
        {
            return tripSegment.TripSegType == BasicTripTypeConstants.YardWork ||
                   tripSegment.TripSegType == BasicTripTypeConstants.ReturnYardNC;
        }

        /// <summary>
        /// Check to see if arriving segment of given trip type is W (Scale) type
        /// 
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns>bool</returns>
        public bool IsTripLegTypePublicScale(TripSegmentModel tripSegment)
        {
            return tripSegment.TripSegType == BasicTripTypeConstants.Scale;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns></returns>
        public bool IsTripLegDropped(TripSegmentModel tripSegment)
        {
            return tripSegment.TripSegType == BasicTripTypeConstants.DropEmpty ||
                   tripSegment.TripSegType == BasicTripTypeConstants.DropFull;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <param name="onlyNonEmpty"></param>
        /// <returns></returns>
        public bool IsTripLegLoaded(TripSegmentModel tripSegment, bool onlyNonEmpty = false)
        {
            if (onlyNonEmpty)
                return tripSegment.TripSegType == BasicTripTypeConstants.PickupFull ||
                       tripSegment.TripSegType == BasicTripTypeConstants.Load;

            return tripSegment.TripSegType == BasicTripTypeConstants.PickupEmpty ||
                   tripSegment.TripSegType == BasicTripTypeConstants.PickupFull ||
                   tripSegment.TripSegType == BasicTripTypeConstants.Load;

        }

        /// <summary>
        /// Complete a given trip
        /// We typically never have the trip object queried within the viewmodel, so we just pass the trip number
        /// and do the query here
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripAsync(string tripNumber)
        {
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            trip.TripStatus = TripStatusConstants.Done;
            return await _tripRepository.UpdateAsync(trip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripSegmentAsync(TripSegmentModel tripSegment)
        {
            tripSegment.TripSegStatus = TripSegStatusConstants.Done;
            return await _tripSegmentRepository.UpdateAsync(tripSegment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public async Task<int> CompleteTripSegmentContainerAsync(TripSegmentContainerModel container)
        {
            container.TripSegContainerReviewFlag = Constants.No;
            container.TripSegContainerActionDateTime = DateTime.Now;
            container.TripSegContainerComplete = Constants.Yes;
            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="gsWt"></param>
        /// <param name="gs2Wt"></param>
        /// <param name="trWt"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripSegmentContainerWeightTimesAsync(TripSegmentContainerModel container, DateTime? gsWt, DateTime? gs2Wt, DateTime? trWt)
        {
            container.WeightGrossDateTime = gsWt;
            container.WeightGross2ndDateTime = gs2Wt;
            container.WeightTareDateTime = trWt;

            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public async Task<int> UpdateTripSegmentContainerLongLatAsync(TripSegmentContainerModel container, int? latitude, int? longitude)
        {
            container.TripSegContainerLongitude = longitude;
            container.TripSegContainerLatitude = latitude;

            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        /// <summary>
        /// Mark a given trip as an exception
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <returns></returns>
        public async Task<int> MarkExceptionTripAsync(string tripNumber)
        {
            var trip = await _tripRepository.FindAsync(t => t.TripNumber == tripNumber);
            trip.TripStatus = TripStatusConstants.Exception;
            return await _tripRepository.UpdateAsync(trip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <returns></returns>
        public async Task<int> MarkExceptionTripSegmentAsync(TripSegmentModel tripSegment)
        {
            tripSegment.TripSegStatus = TripSegStatusConstants.Exception;
            return await _tripSegmentRepository.UpdateAsync(tripSegment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="reviewReason"></param>
        /// <returns></returns>
        public async Task<int> MarkExceptionTripSegmentContainerAsync(TripSegmentContainerModel container, string reviewReason)
        {
            container.TripSegContainerReviewFlag = TripSegStatusConstants.Exception;
            container.TripSegContainerReviewReason = reviewReason;

            // @TODO : As of now, this description will not be persisted if the user logs out and logs back in
            var desc =
                await _codeTableRepository.FindAsync(
                    ct => ct.CodeName == CodeTableNameConstants.ExceptionCodes && ct.CodeValue == reviewReason);
            
            container.TripSegContainerReivewReasonDesc = desc.CodeDisp1;

            return await _tripSegmentContainerRepository.UpdateAsync(container);
        }

        /// <summary>
        /// Propogate any container changes on loaded containers to RT/SC segments.
        /// 
        /// Currently, this will work by propagating container updates to subsequent RT/SC segments until
        /// it encounters a non-RT/SC segment. 
        /// 
        /// For example, if you have a LD->SC->UL->SC trip, then any
        /// container changes on the LD segment will propagate to the first SC segment, and then stop. Then,
        /// any container changes on the UL segment will propagate to the second SC segment, and stop.
        /// 
        /// If however you have a DE->PF->SC->RT trip, then any changes to the PF segment containers will 
        /// propagate to both the SC and RT segments.
        /// </summary>
        /// <param name="tripNumber"></param>
        /// <param name="containers"></param>
        /// <returns></returns>
        public async Task PropagateContainerUpdates(string tripNumber, IEnumerable<Grouping<TripSegmentModel, TripSegmentContainerModel>> containers)
        {
            // For any containers we're loading on the truck, make sure the information is proprogated to the RT/SC segment
            var tripSegments = await FindAllSegmentsForTripAsync(tripNumber);
            var groupingList = containers as IList<Grouping<TripSegmentModel, TripSegmentContainerModel>> ?? containers.ToList();
            var segmentsToProcess = new List<TripSegmentModel>();

            for (var i = 0; i < tripSegments.Count; i++)
            {
                if (int.Parse(tripSegments.ElementAt(i).TripSegNumber) <= int.Parse(groupingList.LastOrDefault().Key.TripSegNumber))
                    continue;

                if (!IsTripLegScale(tripSegments.ElementAt(i)))
                    break;

                segmentsToProcess.Add(tripSegments.ElementAt(i));
            }

            foreach (var segment in segmentsToProcess)
            {
                var currentContainers = await FindAllContainersForTripSegmentAsync(segment.TripNumber, segment.TripSegNumber);

                foreach (var container in groupingList.Where(segment2 => !IsTripLegDropped(segment2.Key)).SelectMany(segment2 => segment2))
                {
                    var sameContainerNumber = currentContainers.FirstOrDefault(ct => ct.TripSegContainerNumber == container.TripSegContainerNumber);
                    var noContainerNumber = currentContainers.FirstOrDefault(ct => ct.TripSegContainerNumber == null);

                    if (sameContainerNumber != null || noContainerNumber != null)
                    {
                        container.TripNumber = sameContainerNumber?.TripNumber ?? noContainerNumber.TripNumber;
                        container.TripSegNumber = sameContainerNumber?.TripSegNumber ?? noContainerNumber.TripSegNumber;
                        container.TripSegContainerSeqNumber = sameContainerNumber?.TripSegContainerSeqNumber ?? noContainerNumber.TripSegContainerSeqNumber;

                        await UpdateTripSegmentContainerAsync(container);
                    }
                    else // Create new trip segment container for RT or SC
                    {
                        var newSeqNumber = currentContainers.LastOrDefault().TripSegContainerSeqNumber + 1;
                        container.TripSegContainerSeqNumber = (short)newSeqNumber;
                        container.TripSegNumber = segment.TripSegNumber;
                        container.TripNumber = segment.TripNumber;

                        await CreateTripSegmentContainerAsync(container);
                    }
                }
            }
        }

        #endregion

        #region Remote process calls

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripInfoProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<TripInfoProcess>> ProcessTripInfoAsync(TripInfoProcess tripInfoProcess)
        {
            var tripProcess =
                await
                    _connection.GetConnection(ConnectionType.Online).UpdateAsync(tripInfoProcess, requeryUpdated: false);
            return tripProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverSegmentDoneProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverSegmentDoneProcess>> ProcessTripSegmentDoneAsync(
            DriverSegmentDoneProcess driverSegmentDoneProcess)
        {
            var containerProcess =
                await _connection.GetConnection().UpdateAsync(driverSegmentDoneProcess, requeryUpdated: false);
            return containerProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverContainerAction"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverContainerActionProcess>> ProcessContainerActionAsync(
            DriverContainerActionProcess driverContainerAction)
        {
            var actionProcess =
                await _connection.GetConnection().UpdateAsync(driverContainerAction, requeryUpdated: false);
            return actionProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverImage"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverImageProcess>> ProcessDriverImageAsync(
            DriverImageProcess driverImage)
        {
            var imageProcess = await _connection.GetConnection().UpdateAsync(driverImage, requeryUpdated: false);
            return imageProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverAck"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverTripAckProcess>> ProcessDriverTripAck(
            DriverTripAckProcess driverAck)
        {
            var tripAckProcess = await _connection.GetConnection().UpdateAsync(driverAck, requeryUpdated: false);
            return tripAckProcess;
        }

        #endregion
    }
}