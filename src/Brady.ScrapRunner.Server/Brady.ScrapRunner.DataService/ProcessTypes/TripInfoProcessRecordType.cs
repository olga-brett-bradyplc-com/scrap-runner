using AutoMapper;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using NHibernate;
using NHibernate.Util;
using System.Text;

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
            Mapper.CreateMap<ContainerChangeProcess, ContainerChangeProcess>();

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
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    TripInfoProcess tripInfoProcess = (TripInfoProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

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

                    //
                    // Validate driver id / Get the EmployeeMaster record
                    //
                    EmployeeMaster employeeMaster = Util.Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
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

                    //
                    // Lookup Preference: DEFCommodSelection
                    //
                    string prefCommodSelection = Util.Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                  employeeMaster.TerminalId, PrefDriverConstants.DEFCommodSelection, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    // Lookup trip  
                    //
                    //Define this or pass this in from somewhere..
                    bool bLogin = true;
                    List<Trip> tripList = new List<Trip>();
                    if (bLogin)
                    {
                        tripList = Util.Common.GetTripsForDriverAtLogin(dataService, settings, userCulture, userRoleIds,
                          tripInfoProcess.EmployeeId, out fault);
                    }
                    else
                    {
                        tripList = Util.Common.GetTripsForDriver(dataService, settings, userCulture, userRoleIds,
                          tripInfoProcess.EmployeeId, out fault);
                    }
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    List<string> customersInTrips = new List<string>();
                    foreach (Trip tripInfo in tripList)
                    {
                        //For testing
                        log.Debug("SRTEST:TripInfoProcess - Trip");
                        log.DebugFormat("SRTEST:TripNumber:{0} Status:{1} AssignStatus:{2} Type:{3} Seq#:{4} Driver:{5} CustHostCode:{6} {7}",
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
                        List<TripReferenceNumber> tripReferenceList = new List<TripReferenceNumber>();
                        tripReferenceList = Util.Common.GetTripReferenceNumbers(dataService, settings, userCulture, userRoleIds,
                                            tripInfo.TripNumber, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //For testing
                        log.Debug("SRTEST:TripInfoProcess - Trip Reference Numbers");
                        foreach (TripReferenceNumber tripreference in tripReferenceList)
                        {
                      
                            log.DebugFormat("SRTEST:TripNumber:{0} RefNumber:{1} Desc:{2}",
                                            tripreference.TripNumber,
                                            tripreference.TripRefNumber,
                                            tripreference.TripRefNumberDesc);
                        }
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        //For each trip, get the segments
                        List<TripSegment> tripSegmentList = new List<TripSegment>();
                        tripSegmentList = Util.Common.GetTripSegments(dataService, settings, userCulture, userRoleIds,
                                          tripInfo.TripNumber, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //For testing
                        log.Debug("SRTEST:TripInfoProcess - Trip Segment");
                        foreach (TripSegment tripsegment in tripSegmentList)
                        {
                            ////////////////////////////////////////////////////////////////////////////////////////////////
                            //For each segment get the dest cust host code. 
                            //Add it to the customersInTrips list, if not already in the list
                            if (customersInTrips.Where(x => x.Contains(tripsegment.TripSegDestCustHostCode)).FirstOrDefault() == null)
                                customersInTrips.Add(tripsegment.TripSegDestCustHostCode);
                            //For testing
                            log.DebugFormat("SRTEST:TripNumber:{0} Seg:{1} Status:{2} Orig:{3} {4} Dest:{5} {6} ",
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
                        List<TripSegmentContainer> tripContainerList = new List<TripSegmentContainer>();
                        tripContainerList = Util.Common.GetTripContainers(dataService, settings, userCulture, userRoleIds,
                                            tripInfo.TripNumber, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //For testing
                        log.Debug("SRTEST:TripInfoProcess - Trip Containers");
                        foreach (TripSegmentContainer tripsegcontainer in tripContainerList)
                        {
                            log.DebugFormat("SRTEST:TripNumber:{0} SegNumber:{1} SeqNumber:{2} ContainerNumber:{3} Type:{4} Size:{5}",
                                tripsegcontainer.TripNumber,
                                tripsegcontainer.TripSegNumber,
                                tripsegcontainer.TripSegContainerSeqNumber,
                                tripsegcontainer.TripSegContainerNumber,
                                tripsegcontainer.TripSegContainerType,
                                tripsegcontainer.TripSegContainerSize);
                        }
                    }//end of foreach (Trip tripInfo in tripList)

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //Loop through the list of customer host codes
                    //Get the directions to each customer host code 
                    log.Debug("SRTEST:TripInfoProcess - Customer Directions");
                    foreach (string custHostCode in customersInTrips)
                    {
                        List<CustomerDirections> custDirectionsList = new List<CustomerDirections>();
                        custDirectionsList = Util.Common.GetCustomerDirections(dataService, settings, userCulture, userRoleIds,
                                             custHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (custDirectionsList.Count > 0)
                        { 
                            StringBuilder sbDirections = new StringBuilder();
                            foreach (CustomerDirections customerDirections in custDirectionsList)
                            {
                                sbDirections.Append(customerDirections.DirectionsDesc.Trim());
                                sbDirections.Append(" ");
                            }
                            string directions = sbDirections.ToString().Trim();
                            log.DebugFormat("SRTEST:HostCode:{0} Directions:{1}",
                                            custHostCode,
                                            directions);
                        }
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    if (prefCommodSelection == Constants.Yes)
                    {
                        //Loop through the list of customer host codes
                        //if DefCommodSelection = Y, set the commodities for each customer
                        log.Debug("SRTEST:TripInfoProcess - Customer Commodities");
                        foreach (string custHostCode in customersInTrips)
                        {
                            List<CustomerCommodity> custCommoditiesList = new List<CustomerCommodity>();
                            custCommoditiesList = Util.Common.GetCustomerCommodities(dataService, settings, userCulture, userRoleIds,
                                                  custHostCode, out fault);
                            if (fault != null)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            foreach (CustomerCommodity customerCommodity in custCommoditiesList)
                            {
                                 log.DebugFormat("SRTEST:HostCode:{0} Code:{1} Desc:{2}",
                                                customerCommodity.CustHostCode,
                                                customerCommodity.CustCommodityCode,
                                                customerCommodity.CustCommodityDesc);
                            }
                        }
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //Loop through the list of customer host codes
                    //Get the locations for each customer
                    log.Debug("SRTEST:TripInfoProcess - Customer Locations");
                    foreach (string custHostCode in customersInTrips)
                    {
                        List<CustomerLocation> custLocationsList = new List<CustomerLocation>();
                        custLocationsList = Util.Common.GetCustomerLocations(dataService, settings, userCulture, userRoleIds,
                                            custHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        foreach (CustomerLocation customerLocation in custLocationsList)
                        {

                            log.DebugFormat("SRTEST:HostCode:{0} Location:{1}",
                                           customerLocation.CustHostCode,
                                           customerLocation.CustLocation);
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
            }
            else
            {
                transaction.Commit();
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
