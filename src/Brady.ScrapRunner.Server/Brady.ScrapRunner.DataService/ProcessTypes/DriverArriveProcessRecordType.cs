using AutoMapper;
using System;
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
using Brady.ScrapRunner.DataService.Util;
using log4net;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver arriving.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverArriveProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverArriveProcess")]
    public class DriverArriveProcessRecordType : ChangeableRecordType
        <DriverArriveProcess, string, DriverArriveProcessValidator, DriverArriveProcessDeletionValidator>
    {

        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverArriveProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverArriveProcess, DriverArriveProcess>();
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
                        ChangeSet<string, DriverArriveProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver arrive processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverArriveProcess> changeSet, ProcessChangeSetSettings settings)
        {
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository)repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security, pipelines, etc. 
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only process one record at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
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

                    int tripSegmentMileageCount = 0;
                    int containerHistoryInsertCount = 0;
                    int driverHistoryInsertCount = 0;
                    int powerHistoryInsertCount = 0;

                    var driverArriveProcess = (DriverArriveProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverArriveProcess backfillDriverArriveProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverArriveProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverArriveProcess, driverArriveProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process arrive for DriverId: " 
                                        + driverArriveProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //DriverArriveProcess has been called
                    log.DebugFormat("SRTEST:DriverArriveProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverArriveProcess Driver:{0} Trip:{1} Seg:{2} DT:{3} PowerId:{4} Odom:{5} GPSAuto:{6} Lat:{7} Lon:{8} MDT:{9}",
                                     driverArriveProcess.EmployeeId, driverArriveProcess.TripNumber,
                                     driverArriveProcess.TripSegNumber, driverArriveProcess.ActionDateTime,
                                     driverArriveProcess.PowerId, driverArriveProcess.Odometer,
                                     driverArriveProcess.GPSAutoFlag, driverArriveProcess.Latitude,
                                     driverArriveProcess.Longitude, driverArriveProcess.Mdtid);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverArriveProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid DriverId: "
                                        + driverArriveProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == currentTrip)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid TripNumber: "
                                        + driverArriveProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(currentTrip))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Arrive processing ends.",
                                        driverArriveProcess.TripNumber);
                        break;
                    }

                    ////////////////////////////////////////////////////////
                    //Do not use the MDTId from the mobile app. Build it using the MDT Prefix (if it exists) plus the employee id.
                    // Lookup Preference: DEFMDTPrefix
                    string prefMdtPrefix = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                    Constants.SystemTerminalId, PrefSystemConstants.DEFMDTPrefix, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    driverArriveProcess.Mdtid = prefMdtPrefix + driverArriveProcess.EmployeeId;

                    ////////////////////////////////////////////////
                    // If the GPS Auto flag is not provided default it to N
                    if (driverArriveProcess.GPSAutoFlag == null)
                    {
                        driverArriveProcess.GPSAutoFlag = Constants.No;
                    }

                    ////////////////////////////////////////////////
                    // Get the PowerMaster record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                      driverArriveProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid PowerId: "
                                        + driverArriveProcess.PowerId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Adjust odometer based on previously recorded odometer. 
                    //Do not use the odometer from the driver if it is less than the last recorded 
                    //odometer stored in the PowerMaster.
                    if (powerMaster.PowerOdometer != null)
                    {
                        if (driverArriveProcess.Odometer < powerMaster.PowerOdometer)
                        {
                            driverArriveProcess.Odometer = (int)powerMaster.PowerOdometer;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all segments for the trip
                    var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                                        driverArriveProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var currentTripSegment = (from item in tripSegList
                              where item.TripSegNumber == driverArriveProcess.TripSegNumber
                              select item).FirstOrDefault();
                    if (null == currentTripSegment)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid TripSegment: " +
                            driverArriveProcess.TripNumber + "-" + driverArriveProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all containers for the segment
                    var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                               driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Customer record for the destination cust host code
                    var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                             currentTripSegment.TripSegDestCustHostCode, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == destCustomerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid CustHostCode: "
                                        + currentTripSegment.TripSegDestCustHostCode));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Define the TripSegmentContainer for use later
                    TripSegmentContainer tripSegmentContainer = new TripSegmentContainer();

                    ////////////////////////////////////////////////
                    //Update Container Status for all containers on the power unit.
                    //Note that we may not have any container numbers at the arrive because the driver may not have
                    //loaded any containers (on the app) before he left the yard.
                    //The power id field in the ContainerMaster determines what containers are on the power unit.
                    var containersOnPowerId = Common.GetContainersForPowerId(dataService, settings, userCulture, userRoleIds,
                                              driverArriveProcess.PowerId, out fault);
                    if (null != containersOnPowerId && containersOnPowerId.Count() > 0)
                    {
                        //For each container, update the ContainerMaster and the TripSegmentContainer table
                        foreach (var containerOnPowerId in containersOnPowerId)
                        {
                            var containerMaster = Common.GetContainer(dataService, settings, userCulture, userRoleIds,
                                                  containerOnPowerId.ContainerNumber, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            if (null == containerMaster)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid ContainerNumber: "
                                                + containerOnPowerId.ContainerNumber));
                                break;
                            }
                            //PowerId already set. Container is still on the power id. Do not change.
                            //containerMaster.ContainerPowerId = driverArriveProcess.PowerId;

                            //Set the container status. It has either arrived at a yard or a customer site.
                            if (currentTripSegment.TripSegDestCustType == CustomerTypeConstants.Yard)
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Yard;
                            }
                            else
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.CustomerSite;
                                containerMaster.ContainerPrevCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                            }

                            containerMaster.ContainerCurrentTripNumber = driverArriveProcess.TripNumber;
                            containerMaster.ContainerCurrentTripSegNumber = driverArriveProcess.TripSegNumber;
                            containerMaster.ContainerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                            containerMaster.ContainerCustType = currentTripSegment.TripSegDestCustType;

                            DateTime? prevLastActionDateTime = containerMaster.ContainerLastActionDateTime;
                            containerMaster.ContainerLastActionDateTime = driverArriveProcess.ActionDateTime;

                            //Remove these since container has not yet been set down, still on the move.
                            containerMaster.ContainerLocation = null;
                            containerMaster.ContainerLatitude = null;
                            containerMaster.ContainerLongitude = null;

                            //Do not change on an arrive
                            //containerMaster.ContainerPendingMoveDateTime;

                            //Now that the driver has arrived on a RT segment, remove the Inbound terminal.
                            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                            {
                                containerMaster.ContainerInboundTerminalId = null;
                            }

                            //Do the update
                            changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                            log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - Arrive.",
                                            containerOnPowerId.ContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                            {
                                var s = string.Format("DriverArriveProcess:Could not update ContainerMaster for ContainerNumber:{0}.",
                                         containerOnPowerId.ContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }

                            ////////////////////////////////////////////////
                            //Add record to Container History. 
                            if (!Common.InsertContainerHistory(dataService, settings, containerMaster, destCustomerMaster, prevLastActionDateTime,
                                ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                log.ErrorFormat("InsertContainerHistory failed: {0} during arrive request: {1}", fault.Message, driverArriveProcess);
                                break;
                            }

                            ////////////////////////////////////////////////
                            //Update TripSegmentContainer table.
                            if (null != tripSegContainerList && tripSegContainerList.Count() > 0)
                            {
                                //First, try to find a container in the list that matches the container number on the power unit.
                                tripSegmentContainer = (from item in tripSegContainerList
                                              where item.TripSegContainerNumber == containerMaster.ContainerNumber
                                              select item).FirstOrDefault();
                                if (null == tripSegmentContainer)
                                {
                                    //Otherwise find a container record with no container number
                                    tripSegmentContainer = (from item in tripSegContainerList
                                              where item.TripSegContainerNumber == null
                                              select item).FirstOrDefault();
                                }
                            }
                            //If still not found, then add a new one
                            if (null == tripSegmentContainer)
                            {
                                //Set up a new record
                                tripSegmentContainer = new TripSegmentContainer();

                                //Look up the last trip segment container for this trip and segment
                                //Use this to calculate the next sequence number.
                                var tripSegmentContainerMax = Common.GetTripSegmentContainerLast(dataService, settings, userCulture, userRoleIds,
                                                              driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                                if (null != fault)
                                {
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                    break;
                                }
                                if (null == tripSegmentContainerMax)
                                {
                                    tripSegmentContainer.TripSegContainerSeqNumber = 0;
                                }
                                else
                                {
                                    tripSegmentContainer.TripSegContainerSeqNumber = ++tripSegmentContainerMax.TripSegContainerSeqNumber;
                                }
                                tripSegmentContainer.TripNumber = driverArriveProcess.TripNumber;
                                tripSegmentContainer.TripSegNumber = driverArriveProcess.TripSegNumber;
                            }
                            tripSegmentContainer.TripSegContainerNumber = containerOnPowerId.ContainerNumber;
                            tripSegmentContainer.TripSegContainerType = containerOnPowerId.ContainerType;
                            tripSegmentContainer.TripSegContainerSize = containerOnPowerId.ContainerSize;
                            tripSegmentContainer.TripSegContainerActionDateTime = driverArriveProcess.ActionDateTime;

                            //Not applicable on an arrive.
                            //tripSegmentContainer.TripSegContainerCommodityCode = containerOnPowerId.ContainerCommodityCode;
                            //tripSegmentContainer.TripSegContainerCommodityDesc = containerOnPowerId.ContainerCommodityDesc;

                            //Do the update
                            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
                            log.DebugFormat("SRTEST:Saving TripSegmentContainer for ContainerNumber:{0} - Arrive.",
                                            tripSegmentContainer.TripSegContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
                            {
                                var s = string.Format("DriverArriveProcess:Could not update TripSegmentContainer for ContainerNumber:{0}.",
                                         tripSegmentContainer.TripSegContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }// end of foreach...
                    }//end of if (null != containersOnPowerId...

                    ////////////////////////////////////////////////
                    //Update the TripSegment record.
                    //On first segment, remove previously recorded Trip Segment times and odometers.
                    //Since driver may have gone enroute/arrived, logged out and now has gone enroute/arrive again
                    if (currentTripSegment.TripSegNumber == Constants.FirstSegment)
                    {
                        currentTripSegment.TripSegEndDateTime = null;
                        currentTripSegment.TripSegActualStopEndDateTime = null;
                    }

                    currentTripSegment.TripSegPowerId = driverArriveProcess.PowerId;
                    currentTripSegment.TripSegDriverId = driverArriveProcess.EmployeeId;
                    currentTripSegment.TripSegDriverName = Common.GetEmployeeName(employeeMaster);
                    currentTripSegment.TripSegOdometerEnd = driverArriveProcess.Odometer;
                    currentTripSegment.TripSegEndLatitude = driverArriveProcess.Latitude;
                    currentTripSegment.TripSegEndLongitude = driverArriveProcess.Longitude;

                    //Driver has arrived. Drive time ends. Stop time begins.
                    currentTripSegment.TripSegActualDriveEndDateTime = driverArriveProcess.ActionDateTime;
                    currentTripSegment.TripSegActualStopStartDateTime = driverArriveProcess.ActionDateTime;

                    //Calculate the drive minutes:DriveEndDateTime - DriveStartDateTime
                    if (currentTripSegment.TripSegActualDriveStartDateTime != null && currentTripSegment.TripSegActualDriveEndDateTime != null)
                    {
                        currentTripSegment.TripSegActualDriveMinutes = (int)(currentTripSegment.TripSegActualDriveEndDateTime.Value.Subtract
                               (currentTripSegment.TripSegActualDriveStartDateTime.Value).TotalMinutes);
                    }
                    else
                    {
                        currentTripSegment.TripSegActualDriveMinutes = 0;
                    }

                    //Update TripSegment Primary Container Information from first TripSegmentContainer information. 
                    //Use the list instead of requerying the database. Values are not yet saved.
                    if (tripSegContainerList.Count() > 0)
                    {
                        var firstTripSegmentContainer = tripSegContainerList.First();

                        //Only if there is a container number
                        if (null != firstTripSegmentContainer.TripSegContainerNumber)
                        {
                            currentTripSegment.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                            currentTripSegment.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                            currentTripSegment.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                            currentTripSegment.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            currentTripSegment.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            currentTripSegment.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;
                        }
                    }
                    //Determine number of TripSegmentContainer records. Use the list.
                    currentTripSegment.TripSegContainerQty = tripSegContainerList.Count();

                    //Do the update
                    changeSetResult = Common.UpdateTripSegment(dataService, settings, currentTripSegment);
                    log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Arrive.",
                                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
                    {
                        var s = string.Format("DriverArriveProcess:Could not update TripSegment for Trip:{0}-{1}.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the TripSegmentMileage record.
                    //Normally for arrives, there should be an open-ended record to update 
                    var tripSegmentMileage = Common.GetTripSegmentMileageOpenEndedLast(dataService, settings, userCulture, userRoleIds,
                                        driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripSegmentMileage)
                    {
                        //If there is no open-ended mileage record to update, add a complete one with start and end odometers.
                        //Pass in true to set both starting and ending odometers.
                        log.DebugFormat("SRTEST:Adding TripSegmentMileage Record for Trip:{0}-{1} - Arrive.",
                                        driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber);
                        if(!Common.InsertTripSegmentMileage(dataService, settings, userRoleIds, userCulture, log,
                            currentTripSegment, containersOnPowerId, true, ++tripSegmentMileageCount, out fault))
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            log.ErrorFormat("InsertTripSegmentMileage failed: {0} during arrive request: {1}", fault.Message, driverArriveProcess);
                            break;
                        }
                    }
                    else
                    {
                        //Do the update
                        changeSetResult = Common.UpdateTripSegmentMileageFromSegment
                            (dataService, settings, tripSegmentMileage, currentTripSegment, containersOnPowerId);
                        log.DebugFormat("SRTEST:Saving TripSegmentMileage Record for Trip:{0}-{1} - Arrive.",
                                        driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
                        {
                            var s = string.Format("DriverArriveProcess:Could not update TripSegmentMileage for Trip:{0}-{1}.",
                                  driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Update the odometers of subsequent trip segments with the same destination.
                    string hostcode = null;
                    foreach (var nextTripSegment in tripSegList)
                    {
                        if (nextTripSegment.TripSegNumber == driverArriveProcess.TripSegNumber)
                        {
                            //This is our current destination customer.
                            hostcode = nextTripSegment.TripSegDestCustHostCode;
                        }
                        //Look for subsequent segments with the same destination customer.
                        //The first string follows the second string in the sort order.
                        else if (1 == String.Compare(nextTripSegment.TripSegNumber, driverArriveProcess.TripSegNumber, StringComparison.Ordinal))
                        {
                            if (hostcode == nextTripSegment.TripSegDestCustHostCode)
                            {
                                //Since destination of the next segment is the same as the current segment, we have arrived at both.
                                nextTripSegment.TripSegOdometerStart = nextTripSegment.TripSegOdometerEnd = driverArriveProcess.Odometer;

                                //Do the update
                                changeSetResult = Common.UpdateTripSegment(dataService, settings, nextTripSegment);
                                log.DebugFormat("SRTEST:Saving Odometer in TripSegment Record for Trip:{0}-{1} - Arrive.",
                                                nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, nextTripSegment, log))
                                {
                                    var s = string.Format("DriverArriveProcess:Could not update Odometer in TripSegment for Trip:{0}-{1}.",
                                        nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }
                                //Also add a TripMileage record for the next segment if the destination host code is
                                //the same as the current segment.
                                log.DebugFormat("SRTEST:Adding TripSegmentMileage Record for Trip:{0}-{1} - Arrive.",
                                                nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                                if(!Common.InsertTripSegmentMileage(dataService, settings, userRoleIds, userCulture, log,
                                    nextTripSegment, containersOnPowerId, true, ++tripSegmentMileageCount, out fault))
                                {
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                    log.ErrorFormat("InsertTripSegmentMileage failed: {0} during arrive request: {1}", fault.Message, driverArriveProcess);
                                    break;
                                }

                                //Keep looking for more segments with the same destination. There can be more.

                            }
                            else
                            {
                                //Once the next segment is a different destination, we are done. 
                                break;
                            }
                        }
                    }

                    ////////////////////////////////////////////////
                    //Update Trip Record.  Update Primary Container Information if this is the first TripSegment.
                    //But only if the container number in the TripSegmentContainer record is not null. 
                    if (currentTripSegment.TripSegNumber == Constants.FirstSegment &&
                        null != currentTripSegment.TripSegPrimaryContainerNumber)
                    {
                        currentTrip.TripPrimaryContainerNumber = currentTripSegment.TripSegPrimaryContainerNumber;
                        currentTrip.TripPrimaryContainerType = currentTripSegment.TripSegPrimaryContainerType;
                        currentTrip.TripPrimaryContainerSize = currentTripSegment.TripSegPrimaryContainerSize;
                        currentTrip.TripPrimaryCommodityCode = currentTripSegment.TripSegPrimaryContainerCommodityCode;
                        currentTrip.TripPrimaryCommodityDesc = currentTripSegment.TripSegPrimaryContainerCommodityDesc;
                        currentTrip.TripPrimaryContainerLocation = currentTripSegment.TripSegPrimaryContainerLocation;
                        currentTrip.TripStartedDateTime = currentTripSegment.TripSegStartDateTime;
                        if (currentTripSegment.TripSegContainerQty > 1)
                        {
                            currentTrip.TripMultContainerFlag = Constants.Yes;
                        }
                        else
                        {
                            currentTrip.TripMultContainerFlag = Constants.No;
                        }
                        currentTrip.TripInProgressFlag = Constants.Yes;

                        //Do the update
                        changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
                        log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Arrive.",
                                        currentTrip.TripNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
                        {
                            var s = string.Format("DriverArriveProcess:Could not update Trip for Trip:{0}.",
                                currentTrip.TripNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. 
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid DriverId: " 
                                          + driverArriveProcess.EmployeeId));
                        break;
                    }
                    driverStatus.Status = DriverStatusSRConstants.Arrive;
                    driverStatus.PrevDriverStatus = driverStatus.Status;
                    driverStatus.TripNumber = driverArriveProcess.TripNumber;
                    driverStatus.TripSegNumber = driverArriveProcess.TripSegNumber;
                    driverStatus.TripSegType = currentTripSegment.TripSegType;
                    driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                    driverStatus.TripStatus = currentTrip.TripStatus;
                    driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                    driverStatus.PowerId = driverArriveProcess.PowerId;
                    driverStatus.MDTId = driverArriveProcess.Mdtid;
                    driverStatus.ActionDateTime = driverArriveProcess.ActionDateTime;
                    driverStatus.Odometer = driverArriveProcess.Odometer;
                    driverStatus.GPSAutoGeneratedFlag = driverArriveProcess.GPSAutoFlag;
                    if (null == driverArriveProcess.Latitude || null == driverArriveProcess.Longitude)
                    {
                        driverStatus.GPSXmitFlag = Constants.No;
                    }
                    else
                    {
                        driverStatus.GPSXmitFlag = Constants.Yes;
                    }

                    //Do the update
                    changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                    log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Arrive.",
                                    driverStatus.EmployeeId);
                    if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                    {
                        var s = string.Format("DriverArriveProcess:Could not update DriverStatus for DriverId:{0}.",
                                               driverStatus.EmployeeId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table.
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during arrive request: {1}", fault.Message, driverArriveProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 

                    powerMaster.PowerOdometer = driverArriveProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverArriveProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverArriveProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverArriveProcess.TripSegNumber;
                    powerMaster.PowerCurrentTripSegType = currentTripSegment.TripSegType;

                    //Set these to the trip destination host code and type.
                    powerMaster.PowerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = currentTripSegment.TripSegDestCustType;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - Arrive.",
                                    driverArriveProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("DriverArriveProcess:Could not update PowerMaster for PowerId:{0}.",
                                              driverArriveProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, destCustomerMaster,
                        ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerHistory failed: {0} during enorute request: {1}", fault.Message, driverArriveProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update CustomerMaster.  If driver auto-arrived, update auto-arrive flag for the destination customer.
                    if (driverArriveProcess.GPSAutoFlag == Constants.Yes)
                    {
                        destCustomerMaster.CustAutoGPSFlag = driverArriveProcess.GPSAutoFlag;

                        //Do the update
                        changeSetResult = Common.UpdateCustomerMaster(dataService, settings, destCustomerMaster);
                        log.DebugFormat("SRTEST:Saving CustomerMaster Record for CustHostCode:{0} - Arrive.",
                                        currentTripSegment.TripSegDestCustHostCode);
                        if (Common.LogChangeSetFailure(changeSetResult, destCustomerMaster, log))
                        {
                            var s = string.Format("DriverArriveProcess:Could not update CustomerMaster for CustHostCode:{0}.",
                                                  currentTripSegment.TripSegDestCustHostCode);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }//end of if (driverArriveProcess.GPSAutoFlag 

                    ////////////////////////////////////////////////
                    //Send GPS Location Status packet to tracker. (NOT USED)

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Arrive. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverArrive);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverArriveProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverArriveProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverArriveProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverArriveProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverArriveProcess.PowerId);
                    sbComment.Append(" State:");
                    sbComment.Append(currentTripSegment.TripSegDestCustState);
                    sbComment.Append(" Odom:");
                    sbComment.Append(driverArriveProcess.Odometer);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverArriveProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverArriveProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Arrive");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("DriverArriveProcess:Could not update EventLog for Driver {0} {1}.",
                                driverArriveProcess.EmployeeId, EventCommentConstants.ReceivedDriverArrive);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // TODO: Check if trip is out of order. May not need to do this. Should have been done on the enroute.
                    // If trip is out or order, resequence his trips.
                    // Normally we would then send a resequence trips message to  driver.

                } //end of foreach...
            }//end of if (!changeSetResult.Failed...

            // If our local session variable is set then it is our session/txn to deal with
            // otherwise we simply return the result.
            if (session == null)
            {
                // Capture details of outgoing response too and log at INFO level
                log.Info(RequestResponseUtil.CaptureResponse(changeSetResult, requestRespStrBld));
                return changeSetResult;
            }

            if (changeSetResult.FailedCreates.Any() || changeSetResult.FailedUpdates.Any() ||
                changeSetResult.FailedDeletions.Any())
            {
                transaction.Rollback();
                log.Debug("SRTEST:Transaction Rollback - Arrive");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Arrive");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            // Capture details of outgoing response too and log at INFO level
            log.Info(RequestResponseUtil.CaptureResponse(changeSetResult, requestRespStrBld));
            return changeSetResult;
        }
    }
}