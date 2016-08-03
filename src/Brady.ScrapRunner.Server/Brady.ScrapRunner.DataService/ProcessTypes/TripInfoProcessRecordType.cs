using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Enums;
using Brady.ScrapRunner.DataService.Util;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Get the relevant client trip information for the driver.  
    /// </summary>
    /// 
    /// Note this our business processes is relatively independent of the "trivial" backing query.  
    /// call using the form PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery"
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/TripInfoChangeProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    [EditAction("TripInfoProcess")]
    public class TripInfoProcessRecordType : ChangeableRecordType
            <TripInfoProcess, string, TripInfoProcessValidator, TripInfoProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TripInfoProcess, TripInfoProcess>();

            // Note should we ever need to map the nested child list too, 
            // we would need to be more explicit.  see also: 
            // http://stackoverflow.com/questions/9394833/automapper-with-nested-child-list
            // Mapper.CreateMap<TripInfoProcess, TripInfoProcess>()
            //    .ForMember(dest => dest.Trip, opts => opts.MapFrom(src => src.Trip));
            // Mapper.CreateMap<Trip, Trip>();
        }

        /// <summary>
        /// This is the deprecated signature.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="token"></param>
        /// <param name="username"></param>
        /// <param name="changeSet"></param>
        /// <param name="persistChanges"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService, string token, string username,
            ChangeSet<string, TripInfoProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// This is the "real" method implementation.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
            ChangeSet<string, TripInfoProcess> changeSet, ProcessChangeSetSettings settings)
        {

            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted
            // then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository)repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.
            // This should gain the benefit of validators, auditing, security, piplines, etc.
            // However, it looks like we are losing some user input in the base.ProcessChangeSet
            // from a reretrieve or the sparse NHibernate mapping?
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only respond to one request at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() && !changeSetResult.FailedDeletions.Any())
            {

                // Determine userCulture and userRoleIds.
                var userCulture = "en-GB";
                var userRoleIds = Enumerable.Empty<long>().ToArray();
                if (null != settings.Username && null != settings.Token)
                {
                    var userCultureDetails = authorisation.GetUserCultureDetailsAsync(settings.Token, settings.Username).Result;
                    userCulture = userCultureDetails.LanguageCulture;
                    userRoleIds = authorisation.GetRoleIdsAsync(settings.Token, settings.Username).Result;
                }

                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    var tripInfoProcess = (TripInfoProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the gernal case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    TripInfoProcess backfillTripInfoProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillTripInfoProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillTripInfoProcess, tripInfoProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process trips for Driver ID " + tripInfoProcess.EmployeeId));
                        break;
                    }

                    //Process will return the following lists.
                    List<Trip> fullTripList = new List<Trip>();
                    List<TripSegment> fullTripSegmentList = new List<TripSegment>();
                    List<TripSegmentContainer> fullTripSegmentContainerList = new List<TripSegmentContainer>();
                    List<TripReferenceNumber> fullTripReferenceNumberList = new List<TripReferenceNumber>();
                    List<CustomerMaster> fullCustomerMasterList = new List<CustomerMaster>();
                    List<CustomerDirections> fullCustomerDirectionsList = new List<CustomerDirections>();
                    List<CustomerCommodity> fullCustomerCommodityList = new List<CustomerCommodity>();
                    List<CustomerLocation> fullCustomerLocationList = new List<CustomerLocation>();
                    List<TerminalMaster> fullTerminalList = new List<TerminalMaster>();

                    ////////////////////////////////////////////////
                    //TripInfoProcess has been called
                    log.DebugFormat("SRTEST:TripInfoProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:TripInfoProcess Driver:{0}",
                                     tripInfoProcess.EmployeeId);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         tripInfoProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + tripInfoProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Lookup Preference: DEFCommodSelection
                    string prefCommodSelection = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                 employeeMaster.TerminalId, PrefDriverConstants.DEFCommodSelection, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the list of trips for driver
                    List<Trip> tripList;
                    if (tripInfoProcess.SendOnlyNewModTrips == Constants.Yes)
                    {
                        tripList = Common.GetTripsForDriver(dataService, settings, userCulture, userRoleIds,
                          tripInfoProcess.EmployeeId, out fault);
                    }
                    else
                    {
                        tripList = Common.GetTripsForDriverAtLogin(dataService, settings, userCulture, userRoleIds,
                          tripInfoProcess.EmployeeId, out fault);
                    }
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //Save the list for sending to driver
                    fullTripList.AddRange(tripList);

                    var customersInTrips = new List<string>();
                    var terminalsInTrips = new List<string>();

                    foreach (var tripInfo in tripList)
                    {
                        //For testing
                        log.DebugFormat("SRTEST:TripInfoProcess:Trip:TripNumber:{0} Status:{1} AssignStatus:{2} Type:{3} Seq#:{4} Driver:{5} CustHostCode:{6} {7}",
                                        tripInfo.TripNumber,
                                        tripInfo.TripStatus,
                                        tripInfo.TripAssignStatus,
                                        tripInfo.TripType,
                                        tripInfo.TripSequenceNumber,
                                        tripInfo.TripDriverId,
                                        tripInfo.TripCustHostCode,
                                        tripInfo.TripCustName);

                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        //For each trip, get the reference numbers
                        var tripReferenceNumberList = Common.GetTripReferenceNumberForTrip(dataService, settings, userCulture, userRoleIds,
                                                      tripInfo.TripNumber, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullTripReferenceNumberList.AddRange(tripReferenceNumberList);
                        //For testing
                        foreach (var tripreference in tripReferenceNumberList)
                        {

                            log.DebugFormat("SRTEST:TripInfoProcess:Reference Numbers:TripNumber:{0} RefNumber:{1} Desc:{2}",
                                            tripreference.TripNumber,
                                            tripreference.TripRefNumber,
                                            tripreference.TripRefNumberDesc);
                        }
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        //For each trip, get the incomplete segments
                        var tripSegmentList = Common.GetTripSegmentsIncomplete(dataService, settings, userCulture, userRoleIds,
                                          tripInfo.TripNumber, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullTripSegmentList.AddRange(tripSegmentList);
                        //For testing
                        foreach (var tripsegment in tripSegmentList)
                        {
                            ////////////////////////////////////////////////////////////////////////////////////////////////
                            //For each segment get the dest cust host code. 
                            //Add it to the customersInTrips list, if not already in the list
                            if (customersInTrips.Where(x => x.Contains(tripsegment.TripSegDestCustHostCode)).FirstOrDefault() == null)
                                customersInTrips.Add(tripsegment.TripSegDestCustHostCode);
                            //For testing
                            log.DebugFormat("SRTEST:TripInfoProcess:Segment:TripNumber:{0} Seg:{1} Status:{2} Orig:{3} {4} Dest:{5} {6} ",
                                tripsegment.TripNumber,
                                tripsegment.TripSegNumber,
                                tripsegment.TripSegStatus,
                                tripsegment.TripSegOrigCustHostCode,
                                tripsegment.TripSegOrigCustName,
                                tripsegment.TripSegDestCustHostCode,
                                tripsegment.TripSegDestCustName);
                        }
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        //For each trip, get the containers for all of the segments
                        var tripContainerList = Common.GetTripContainersForTrip(dataService, settings, userCulture, userRoleIds,
                                            tripInfo.TripNumber, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullTripSegmentContainerList.AddRange(tripContainerList);
                        //For testing
                        foreach (var tripsegcontainer in tripContainerList)
                        {
                            log.DebugFormat("SRTEST:TripInfoProcess:Containers:TripNumber:{0} SegNumber:{1} SeqNumber:{2} ContainerNumber:{3} Type:{4} Size:{5}",
                                tripsegcontainer.TripNumber,
                                tripsegcontainer.TripSegNumber,
                                tripsegcontainer.TripSegContainerSeqNumber,
                                tripsegcontainer.TripSegContainerNumber,
                                tripsegcontainer.TripSegContainerType,
                                tripsegcontainer.TripSegContainerSize);
                        }
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        //For each trip get the terminal. 
                        //Add it to the terminalsInTrips list, if not already in the list
                        if (terminalsInTrips.Where(x => x.Contains(tripInfo.TripTerminalId)).FirstOrDefault() == null)
                            terminalsInTrips.Add(tripInfo.TripTerminalId);
                        //For testing
                        log.DebugFormat("SRTEST:TripInfoProcess:TripNumber:{0} TerminalId:{1}",
                            tripInfo.TripNumber,
                            tripInfo.TripTerminalId);
                    }//end of foreach (Trip tripInfo in tripList)

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //Loop through the list of customer host codes
                    //Get the customer master record for each customer host code 
                    foreach (var custHostCode in customersInTrips)
                    {
                        var custMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                            custHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullCustomerMasterList.Add(custMaster);

                        log.DebugFormat("SRTEST:TripInfoProcess:Customer Master:HostCode:{0} Name:{1} Signature Flag:{2}",
                                        custMaster.CustHostCode,
                                        custMaster.CustName,
                                        custMaster.CustSignatureRequired);
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //Loop through the list of customer host codes
                    //Get the directions to each customer host code 
                    foreach (var custHostCode in customersInTrips)
                    {
                        var custDirectionsList = Common.GetCustomerDirections(dataService, settings, userCulture, userRoleIds,
                                             custHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullCustomerDirectionsList.AddRange(custDirectionsList);
                        if (custDirectionsList.Count > 0)
                        {
                            StringBuilder sbDirections = new StringBuilder();
                            foreach (var customerDirections in custDirectionsList)
                            {
                                sbDirections.Append(customerDirections.DirectionsDesc.Trim());
                                sbDirections.Append(" ");
                            }
                            string directions = sbDirections.ToString().Trim();
                            log.DebugFormat("SRTEST:TripInfoProcess:Customer Directions:HostCode:{0} Directions:{1}",
                                            custHostCode,
                                            directions);
                        }
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    if (prefCommodSelection == Constants.Yes)
                    {
                        //Loop through the list of customer host codes
                        //if DefCommodSelection = Y, set the commodities for each customer
                        foreach (var custHostCode in customersInTrips)
                        {
                            var custCommodityList = Common.GetCustomerCommodities(dataService, settings, userCulture, userRoleIds,
                                                  custHostCode, out fault);
                            if (fault != null)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            //Save the list for sending to driver
                            fullCustomerCommodityList.AddRange(custCommodityList);
                            foreach (var customerCommodity in custCommodityList)
                            {
                                log.DebugFormat("SRTEST:TripInfoProcess:Customer Commodities:HostCode:{0} Code:{1} Desc:{2}",
                                               customerCommodity.CustHostCode,
                                               customerCommodity.CustCommodityCode,
                                               customerCommodity.CustCommodityDesc);
                            }
                        }
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //Loop through the list of customer host codes
                    //Get the locations for each customer
                    foreach (var custHostCode in customersInTrips)
                    {
                        var custLocationsList = Common.GetCustomerLocations(dataService, settings, userCulture, userRoleIds,
                                            custHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullCustomerLocationList.AddRange(custLocationsList);
                        foreach (var customerLocation in custLocationsList)
                        {

                            log.DebugFormat("SRTEST:TripInfoProcess:Customer Locations:HostCode:{0} Location:{1}",
                                           customerLocation.CustHostCode,
                                           customerLocation.CustLocation);
                        }
                    }
                    foreach(var term in terminalsInTrips)
                    {
                        var terminal = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                            term, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Save the list for sending to driver
                        fullTerminalList.Add(terminal);

                        log.DebugFormat("SRTEST:TripInfoProcess:Terminal:Id:{0}", term);
                    }
                    //Set the return values
                    tripInfoProcess.Trips = fullTripList;
                    tripInfoProcess.TripSegments = fullTripSegmentList;
                    tripInfoProcess.TripSegmentContainers = fullTripSegmentContainerList;
                    tripInfoProcess.TripReferenceNumbers = fullTripReferenceNumberList;
                    tripInfoProcess.CustomerMasters = fullCustomerMasterList;
                    tripInfoProcess.CustomerDirections = fullCustomerDirectionsList;
                    tripInfoProcess.CustomerLocations = fullCustomerLocationList;
                    tripInfoProcess.CustomerCommodities = fullCustomerCommodityList;
                    tripInfoProcess.Terminals = fullTerminalList;

                    //Now that we have sent all the trips, update the TripSendFlag in the Trip table
                    //so that the trips will not be sent again.
                    //If the trip is modified, SR will set the flag back to Ready status.
                    //If the driver logs in again, he will get trips in Ready status or SentToDriver status. 
                    foreach (var trip in tripList)
                    {
                        trip.TripSendFlag = TripSendFlagValue.SentToDriver;

                        ChangeSetResult<string> scratchChangeSetResult = Common.UpdateTrip(dataService, settings, trip);
                        log.DebugFormat("SRTEST:TripInfoProcess:Saving Trip Record:{0} TripSendFlag:{1}",
                                       trip.TripNumber,
                                       TripSendFlagValue.SentToDriver);
                        if (Common.LogChangeSetFailure(scratchChangeSetResult, trip, log))
                        {
                            var s = string.Format("TripInfoProcess:Could not update Trip for TripSendFlagValue: {0}.", TripSendFlagValue.SentToDriver);
                            scratchChangeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                    }
                }
            }
            // If our local session variable is set then it is our session/txn to deal with
            // otherwise we simply return the result.
            if (session == null)
            {
                return changeSetResult;
            }

            if (changeSetResult.FailedCreates.Any() || changeSetResult.FailedUpdates.Any() ||
                changeSetResult.FailedDeletions.Any())
            {
                transaction.Rollback();
                log.Debug("SRTEST:TripInfoProcess:Transaction Rollback - Trip Info");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:TripInfoProcess:Transaction Committed - Trip Info");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }
    }
}
