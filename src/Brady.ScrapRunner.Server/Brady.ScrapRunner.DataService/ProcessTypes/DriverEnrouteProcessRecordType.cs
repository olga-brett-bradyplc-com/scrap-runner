﻿using AutoMapper;
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
using Brady.ScrapRunner.Domain.Enums;
using log4net;
using System.Collections.Generic;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver going enroute.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverEnrouteProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverEnrouteProcess")]
    public class DriverEnrouteProcessRecordType : ChangeableRecordType
        <DriverEnrouteProcess, string, DriverEnrouteProcessValidator, DriverEnrouteProcessDeletionValidator>
    {
        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverEnrouteProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverEnrouteProcess, DriverEnrouteProcess>();
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
                        ChangeSet<string, DriverEnrouteProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver enroute processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverEnrouteProcess> changeSet, ProcessChangeSetSettings settings)
        {
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted  then open a new session
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

                    var driverEnrouteProcess = (DriverEnrouteProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverEnrouteProcess backfillDriverEnrouteProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverEnrouteProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverEnrouteProcess, driverEnrouteProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Unable to process enroute for DriverId: " + driverEnrouteProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //DriverEnrouteProcess has been called
                    log.DebugFormat("SRTEST:DriverEnrouteProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverEnrouteProcess Driver:{0} Trip:{1} Seg:{2} DT:{3} PowerId:{4} Odom:{5} GPSAuto:{6} Lat:{7} Lon:{8} MDT:{9}",
                                     driverEnrouteProcess.EmployeeId, driverEnrouteProcess.TripNumber,
                                     driverEnrouteProcess.TripSegNumber, driverEnrouteProcess.ActionDateTime,
                                     driverEnrouteProcess.PowerId, driverEnrouteProcess.Odometer,
                                     driverEnrouteProcess.GPSAutoFlag, driverEnrouteProcess.Latitude,
                                     driverEnrouteProcess.Longitude,driverEnrouteProcess.Mdtid);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverEnrouteProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null ==employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid DriverId: "
                                        + driverEnrouteProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverEnrouteProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == currentTrip)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid TripNumber: "
                                        + driverEnrouteProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(currentTrip))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Enroute processing ends.",
                                        driverEnrouteProcess.TripNumber);
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
                    driverEnrouteProcess.Mdtid = prefMdtPrefix + driverEnrouteProcess.EmployeeId;


                    ////////////////////////////////////////////////
                    // If the GPS Auto flag is not provided default it to N
                    if (driverEnrouteProcess.GPSAutoFlag == null)
                    {
                        driverEnrouteProcess.GPSAutoFlag = Constants.No;
                    }

                    ////////////////////////////////////////////////
                    // Get the PowerMaster record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                    driverEnrouteProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid PowerId: "
                                        + driverEnrouteProcess.PowerId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Adjust odometer here before we start using driverEnrouteProcess.Odometer
                    //Do not use the odometer from the driver if it is less than the last recorded 
                    //odometer stored in the PowerMaster.
                    if (powerMaster.PowerOdometer != null)
                    {
                        if (driverEnrouteProcess.Odometer < powerMaster.PowerOdometer)
                        {
                            driverEnrouteProcess.Odometer = (int)powerMaster.PowerOdometer;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all  segments for the trip
                    var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                                                        driverEnrouteProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var currentTripSegment = (from item in tripSegList
                                              where item.TripSegNumber == driverEnrouteProcess.TripSegNumber
                                              select item).FirstOrDefault();
                    if (null == currentTripSegment)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid TripSegment: " +
                            driverEnrouteProcess.TripNumber + "-" + driverEnrouteProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all containers for the segment
                    var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber, out fault);
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
                    // Check if trip is out of order. 
                    // If trip is out or order, resequence his trips.
                    // TODO: Normally we would then send a resequence trips message to driver. 
                    // Perhaps we no longer have to send a resequence trips message to driver. 
                    if (driverEnrouteProcess.TripSegNumber == Constants.FirstSegment)
                    {
                        if (!CheckForTripOutOfOrder(dataService, settings, changeSetResult, msgKey, userRoleIds,
                                               userCulture, employeeMaster, driverEnrouteProcess, currentTrip, currentTripSegment))
                        {
                            break;
                        }
                    }//if (driverEnrouteProcess.TripSegNumber == Constants.FirstSegment)

                    ////////////////////////////////////////////////
                    //Define the TripSegmentContainer for use later
                    TripSegmentContainer tripSegmentContainer = new TripSegmentContainer();

                    ////////////////////////////////////////////////
                    //Update Container Status for all containers on the power unit.
                    //The power id field in the ContainerMaster determines what containers are on the power unit.
                    //Note that we may not have any container numbers at the arrive because the driver may not have
                    //loaded any containers (on the app) before he left the yard.
                    //Containers on truck may consist of:
                    //  Container loaded on the truck and on the app before driver leaves the yard.
                    //  Container left on truck on yard/scale screen on previous trip.
                    //  Container picked up on a previous trip (milk run).
                    //The container may or may not be actually on the truck. We don't know at this point.
                    var containersOnPowerId = Common.GetContainersForPowerId(dataService, settings, userCulture, userRoleIds,
                                                  driverEnrouteProcess.PowerId, out fault);
                    if (null != containersOnPowerId && containersOnPowerId.Count() > 0)
                    {
                        //For each container, update the ContainerMaster. Do not upate the TripSegmentContainer table.
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
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid ContainerNumber: " 
                                                     + containerOnPowerId.ContainerNumber));
                                break;
                            }
                            //PowerId already set.
                            //containerMaster.ContainerPowerId = driverEnrouteProcess.PowerId;

                            //Set the container status. It is either Inbound to a yard or outbound to a customer site.
                            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Inbound;
                                //When the driver goes enroute on a RT segment, set the Inbound terminal to the serving
                                //terminal of the destination customer. This will be removed when the driver arrives.
                                containerMaster.ContainerInboundTerminalId = destCustomerMaster.ServingTerminalId;
                            }
                            else
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Outbound;
                                //Per 09/14/2016 discussion, do not set the host code. These should have no values.
                                //containerMaster.ContainerPrevCustHostCode = currentTripSegment.TripSegDestCustHostCode;                                
                                containerMaster.ContainerPrevCustHostCode = null;
                                containerMaster.ContainerCurrentTripNumber = null;
                                containerMaster.ContainerCurrentTripSegNumber = null;
                                containerMaster.ContainerCurrentTripSegType = null;
                                containerMaster.ContainerCustHostCode = null;
                                containerMaster.ContainerCustType = null;
                            }

                            //Per 09/14/2016 discussion, do not set the host code or trip number.
                            //containerMaster.ContainerCurrentTripNumber = driverEnrouteProcess.TripNumber;
                            //containerMaster.ContainerCurrentTripSegNumber = driverEnrouteProcess.TripSegNumber;
                            //containerMaster.ContainerCurrentTripSegType = currentTripSegment.TripSegType;
                            //containerMaster.ContainerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                            //containerMaster.ContainerCustType = currentTripSegment.TripSegDestCustType;

                            DateTime? prevLastActionDateTime = containerMaster.ContainerLastActionDateTime;
                            containerMaster.ContainerLastActionDateTime = driverEnrouteProcess.ActionDateTime;

                            //Remove these since container is now on the move
                            containerMaster.ContainerLocation = null;
                            containerMaster.ContainerLatitude = null;
                            containerMaster.ContainerLongitude = null;

                            //Do not change on an enroute
                            //containerMaster.ContainerPendingMoveDateTime;

                            //Do the update
                            changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                            log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving ContainerMaster Record for ContainerNumber:{0} - Enroute.",
                                            containerOnPowerId.ContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                            {
                                var s = string.Format("DriverEnrouteProcess:Could not update ContainerMaster for ContainerNumber:{0}.",
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
                                log.ErrorFormat("InsertContainerHistory failed: {0} during enroute request: {1}", fault.Message, driverEnrouteProcess);
                                break;
                            }

                            ////////////////////////////////////////////////
                            /*DO NOT Update TripSegmentContainer table.
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
                                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber, out fault);
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
                                tripSegmentContainer.TripNumber = driverEnrouteProcess.TripNumber;
                                tripSegmentContainer.TripSegNumber = driverEnrouteProcess.TripSegNumber;
                            }
                            tripSegmentContainer.TripSegContainerNumber = containerOnPowerId.ContainerNumber;
                            tripSegmentContainer.TripSegContainerType = containerOnPowerId.ContainerType;
                            tripSegmentContainer.TripSegContainerSize = containerOnPowerId.ContainerSize;
                            tripSegmentContainer.TripSegContainerActionDateTime = driverEnrouteProcess.ActionDateTime;

                            //Not applicable on an enroute.
                            //tripSegmentContainer.TripSegContainerCommodityCode = containerOnPowerId.ContainerCommodityCode;
                            //tripSegmentContainer.TripSegContainerCommodityDesc = containerOnPowerId.ContainerCommodityDesc;

                            //Do the update
                            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
                            log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving TripSegmentContainerRecord for ContainerNumber:{0} - Enroute.",
                                            tripSegmentContainer.TripSegContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
                            {
                                var s = string.Format("DriverEnrouteProcess:Could not update TripSegmentContainerRecord for ContainerNumber:{0}.",
                                         tripSegmentContainer.TripSegContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                            */
                            ////////////////////////////////////////////////
                        }// end of foreach...
                    }//end of if (null != containersOnPowerId...


                    ////////////////////////////////////////////////
                    //Update the TripSegment record.
                    //On first segment, remove previously recorded Trip Segment times and odometers.
                    //Since driver may have gone enroute, logged out and is now going enroute again
                    if (currentTripSegment.TripSegNumber == Constants.FirstSegment)
                    {
                        currentTripSegment.TripSegEndDateTime = null;
                        currentTripSegment.TripSegActualDriveEndDateTime = null;
                        currentTripSegment.TripSegActualStopStartDateTime = null;
                        currentTripSegment.TripSegActualStopEndDateTime = null;
                        currentTripSegment.TripSegOdometerEnd = null;
                    }

                    currentTripSegment.TripSegPowerId = driverEnrouteProcess.PowerId;
                    currentTripSegment.TripSegDriverId = driverEnrouteProcess.EmployeeId;
                    currentTripSegment.TripSegDriverName = Common.GetEmployeeName(employeeMaster);
                    currentTripSegment.TripSegOdometerStart = driverEnrouteProcess.Odometer;
                    currentTripSegment.TripSegStartLatitude = driverEnrouteProcess.Latitude;
                    currentTripSegment.TripSegStartLongitude = driverEnrouteProcess.Longitude;

                    //Driver is enroute. Segment start time begins. Drive time begins.
                    currentTripSegment.TripSegStartDateTime = driverEnrouteProcess.ActionDateTime;
                    currentTripSegment.TripSegActualDriveStartDateTime = driverEnrouteProcess.ActionDateTime;

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
                    log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving TripSegment Record for Trip:{0}-{1} - Enroute.",
                                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
                    {
                        var s = string.Format("DriverEnrouteProcess:Could not update TripSegment for Trip:{0}-{1}.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update Previous Segment End DateTime.
                    //We used to do this:
                    //If not the first segment, set the previous segment's TripSegEndDateTime and TripSegActualStopEndDateTime
                    //to the current segment's TripSegStartDateTime (or driverEnrouteProcess.ActionDateTime).
                    //This was done in old SR to remove any gaps in time.
                    //Maybe we should not set the TripSegActualStopEndDateTime, just the TripSegEndDateTime

                    //If this is not the first segment, set the TripSegEndDateTime for the previous segment
                    if (currentTripSegment.TripSegNumber != Constants.FirstSegment)
                    {
                        // Find the previous segment
                        var previousTripSegment = (from item in tripSegList
                                                   where -1 == String.Compare(item.TripSegNumber, driverEnrouteProcess.TripSegNumber, StringComparison.Ordinal)
                                                   select item).LastOrDefault();
                        if (previousTripSegment != null)
                        {
                            //Segment End Time is overwritten on previous segment when driver goes enroute on this segment
                            previousTripSegment.TripSegEndDateTime = driverEnrouteProcess.ActionDateTime;

                            //Stop End Time is overwritten on previous segment when driver goes enroute on this segment
                            previousTripSegment.TripSegActualStopEndDateTime = driverEnrouteProcess.ActionDateTime;
                            //Recalculate the stop minutes: StopEndDateTime - StopStartDateTime
                            if (previousTripSegment.TripSegActualStopStartDateTime != null && previousTripSegment.TripSegActualStopEndDateTime != null)
                            {
                                previousTripSegment.TripSegActualStopMinutes = (int)(previousTripSegment.TripSegActualStopEndDateTime.Value.Subtract
                                          (previousTripSegment.TripSegActualStopStartDateTime.Value).TotalMinutes);
                            }
                            else
                            {
                                previousTripSegment.TripSegActualStopMinutes = 0;
                            }

                            //Do the update
                            changeSetResult = Common.UpdateTripSegment(dataService, settings, previousTripSegment);
                            log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving previous TripSegment Record for Trip:{0}-{1} - Enroute.",
                                            previousTripSegment.TripNumber, previousTripSegment.TripSegNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, previousTripSegment, log))
                            {
                                var s = string.Format("DriverEnrouteProcess:Could not update previous TripSegment for Trip:{0}-{1}.",
                                    previousTripSegment.TripNumber, previousTripSegment.TripSegNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }

                    ////////////////////////////////////////////////
                    //Update the TripSegmentMileage record.
                    if (currentTripSegment.TripSegNumber == Constants.FirstSegment)
                    {
                        //Delete any existing trip segment mileage records for this segment
                        //Driver might be starting this trip again. Maybe he went enroute/arrived, logged out and is now going enroute again.
                        var oldTripSegmentMileageList = Common.GetTripSegmentMileage(dataService, settings, userCulture, userRoleIds,
                                                driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber, out fault);
                        foreach (var oldTripSegmentMileage in oldTripSegmentMileageList)
                        {
                            //Do the delete. Deleting records with composite keys is now fixed.
                            changeSetResult = Common.DeleteTripSegmentMileage(dataService, settings, oldTripSegmentMileage);
                            log.DebugFormat("SRTEST:DriverEnrouteProcess:Deleting TripSegmentMileage Record for Trip:{0}-{1} Seq:{2}- Enroute.",
                                            oldTripSegmentMileage.TripNumber, oldTripSegmentMileage.TripSegNumber,
                                            oldTripSegmentMileage.TripSegMileageSeqNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, oldTripSegmentMileage, log))
                            {
                                var s = string.Format("DriverEnrouteProcess:Could not delete TripSegmentContainer for Trip:{0}-{1} Seq:{2}.",
                                        oldTripSegmentMileage.TripNumber, oldTripSegmentMileage.TripSegNumber,
                                        oldTripSegmentMileage.TripSegMileageSeqNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }
                    //ToDo: When we delete records, they are not commited.  So when we get the max seq no, it will retrieve
                    //data before the delete. How to fix? Always pass in a sequence number?
                    //Normally for enroutes, we need to add a mileage record, but just in case there is an open-ended
                    //mileage record, we would need to overwrite the information
                    //Get the last open-ended trip segment mileage 
                    var tripSegmentMileage = Common.GetTripSegmentMileageOpenEndedLast(dataService, settings, userCulture, userRoleIds,
                                             driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripSegmentMileage)
                    {
                        //If there is no open-ended mileage record, add a one with just a start odometer.
                        //Pass in false to not update ending odometer. 
                        log.DebugFormat("SRTEST:DriverEnrouteProcess:Adding TripSegmentMileage Record for Trip:{0}-{1} - Enroute.",
                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
                        if(!Common.InsertTripSegmentMileage(dataService, settings, userRoleIds, userCulture, log,
                            currentTripSegment, containersOnPowerId, false, ++tripSegmentMileageCount, out fault))
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            log.ErrorFormat("InsertTripSegmentMileage failed: {0} during enroute request: {1}", fault.Message, driverEnrouteProcess);
                            break;
                        }
                    }
                    else
                    {
                        //Do the update
                        changeSetResult = Common.UpdateTripSegmentMileageFromSegment
                            (dataService, settings, tripSegmentMileage, currentTripSegment, containersOnPowerId);
                        log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving TripSegmentMileage Record for Trip:{0}-{1} - Enroute.",
                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
                        {
                            var s = string.Format("DriverEnrouteProcess:Could not update TripSegmentMileage for Trip:{0}-{1}.",
                                  driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
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
                    }
                    //Only set the trip start time if this is the first segment
                    if (currentTripSegment.TripSegNumber == Constants.FirstSegment)
                    {
                        currentTrip.TripStartedDateTime = currentTripSegment.TripSegStartDateTime;
                    }

                    if (currentTripSegment.TripSegContainerQty > 1)
                    {
                        currentTrip.TripMultContainerFlag = Constants.Yes;
                    }
                    else
                    {
                        currentTrip.TripMultContainerFlag = Constants.No;
                    }
                    currentTrip.TripInProgressFlag = Constants.Yes;

                    //Set the flag to send the scale notice, if applicable.
                    //Check if this enroute is returning to yard after picking up or loading a commodity
                    ////////////////////////////////////////////////
                    //Initially set the send flag not to send completed trip information to the host.
                    currentTrip.TripSendScaleNotificationFlag = TripSendScaleFlagValue.NoScale;
 
                    ////////////////////////////////////////////////
                    // Preferences:  Lookup the yard preference "DEFTHScale".  
                    // If preference DEFTHScale set to Y then set the send scale flag.
                    string preDefthScale = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                           currentTrip.TripTerminalId, PrefYardConstants.DEFTHScale, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //First, this flag must be set to Y or nothing will be sent.
                    if (preDefthScale == Constants.Yes)
                    {
                        //Also these flags must be set on the trip record
                        if (currentTrip.TripCommodityScaleMsg == Constants.Yes ||
                            currentTrip.TripCommodityPurchase == Constants.Yes)
                        {
                            //And the segment type must be return to yard
                            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                            {
                                //If there is any container on this segment that is loaded
                                string loaded = (from item in containersOnPowerId
                                                 where item.ContainerContents == ContainerContentsConstants.Loaded
                                                 select item.ContainerContents).FirstOrDefault();

                                if (loaded != null)
                                {
                                    //Set the scale notice flag to send
                                    currentTrip.TripSendScaleNotificationFlag = TripSendScaleFlagValue.ScaleReady;
                                }
                            }//end of if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                        }//end of if (currentTrip.TripCommodityScaleMsg == Constants.Yes 
                    }//end of if (preDEFTHScale == Constants.Yes)

                    //Do the update
                    changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
                    log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving Trip Record for Trip:{0} - Enroute.",
                                    currentTrip.TripNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
                    {
                        var s = string.Format("DriverEnrouteProcess:Could not update Trip for Trip:{0}.",
                            currentTrip.TripNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. 
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                  driverEnrouteProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " + driverEnrouteProcess.EmployeeId));
                        break;
                    }
                    driverStatus.Status = DriverStatusSRConstants.Enroute;
                    driverStatus.PrevDriverStatus = driverStatus.Status;
                    driverStatus.TripNumber = driverEnrouteProcess.TripNumber;
                    driverStatus.TripSegNumber = driverEnrouteProcess.TripSegNumber;
                    driverStatus.TripSegType = currentTripSegment.TripSegType;
                    driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                    driverStatus.TripStatus = currentTrip.TripStatus;
                    driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                    driverStatus.PowerId = driverEnrouteProcess.PowerId;
                    driverStatus.MDTId = driverEnrouteProcess.Mdtid;
                    driverStatus.ActionDateTime = driverEnrouteProcess.ActionDateTime;
                    driverStatus.Odometer = driverEnrouteProcess.Odometer;
                    driverStatus.GPSAutoGeneratedFlag = driverEnrouteProcess.GPSAutoFlag;
                    if (null == driverEnrouteProcess.Latitude || null == driverEnrouteProcess.Longitude)
                    {
                        driverStatus.GPSXmitFlag = Constants.No;
                    }
                    else
                    {
                        driverStatus.GPSXmitFlag = Constants.Yes;
                    }

                    //Do the update
                    changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                    log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving DriverStatus Record for DriverId:{0} - Enroute.",
                                    driverStatus.EmployeeId);
                    if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                    {
                        var s = string.Format("DriverEnrouteProcess:Could not update DriverStatus for DriverId:{0}.",
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
                        log.ErrorFormat("InsertDriverHistory failed: {0} during Enroute request: {1}", fault.Message, driverEnrouteProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 
                    powerMaster.PowerOdometer = driverEnrouteProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverEnrouteProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverEnrouteProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverEnrouteProcess.TripSegNumber;
                    powerMaster.PowerCurrentTripSegType = currentTripSegment.TripSegType;

                    //Set these to the destination. Power Unit is on the move.
                    powerMaster.PowerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = currentTripSegment.TripSegDestCustType;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving PowerMaster Record for PowerId:{0} - Enroute.",
                                    driverEnrouteProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("DriverEnrouteProcess:Could not update PowerMaster for PowerId:{0}.",
                                              driverEnrouteProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, destCustomerMaster,
                        ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerHistory failed: {0} during enroute request: {1}", fault.Message, driverEnrouteProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update CustomerMaster, if driver auto-departed, update auto-arrive flag for the origin customer. 
                    if (driverEnrouteProcess.GPSAutoFlag == Constants.Yes)
                    {
                        var originCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                                   currentTripSegment.TripSegOrigCustHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (originCustomerMaster == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
                                              + currentTripSegment.TripSegOrigCustHostCode));
                            break;
                        }
                        originCustomerMaster.CustAutoGPSFlag = driverEnrouteProcess.GPSAutoFlag;

                        //Do the update
                        changeSetResult = Common.UpdateCustomerMaster(dataService, settings, originCustomerMaster);
                        log.DebugFormat("SRTEST:DriverEnrouteProcess:Saving CustomerMaster Record for CustHostCode:{0} - Enroute.",
                                        currentTripSegment.TripSegOrigCustHostCode);
                        if (Common.LogChangeSetFailure(changeSetResult, originCustomerMaster, log))
                        {
                            var s = string.Format("DriverEnrouteProcess:Could not update CustomerMaster for CustHostCode:{0}.",
                                                  currentTripSegment.TripSegOrigCustHostCode);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Send GPS Customer Host Code packet to tracker.(NOT USED) 
                    //Send GPS Location Status packet to tracker. (NOT USED)

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Enroute. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverEnroute);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverEnrouteProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverEnrouteProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverEnrouteProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverEnrouteProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverEnrouteProcess.PowerId);
                    sbComment.Append(" State:");
                    sbComment.Append(currentTripSegment.TripSegDestCustState);
                    sbComment.Append(" Odom:");
                    sbComment.Append(driverEnrouteProcess.Odometer);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverEnrouteProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverEnrouteProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:DriverEnrouteProcess:Saving EventLog Record - Enroute");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("DriverEnrouteProcess:Could not update EventLog for Driver {0} {1}.",
                                driverEnrouteProcess.EmployeeId, EventCommentConstants.ReceivedDriverEnroute);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }


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
                log.Debug("SRTEST:DriverEnrouteProcess:Transaction Rollback - Enroute");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:DriverEnrouteProcess:Transaction Committed - Enroute");
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
        /// <summary>
        /// For the first segment, check to see if the driver has gone out of order.
        /// If so, resequence the trips and then reset the trip segment origins.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="driverEnrouteProcess"></param>
        /// <param name="currentTrip"></param>
        /// <param name="currentTripSegment"></param>
        /// <returns></returns>
        public bool CheckForTripOutOfOrder(IDataService dataService, ProcessChangeSetSettings settings,
          ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
          EmployeeMaster employeeMaster, DriverEnrouteProcess driverEnrouteProcess, Trip currentTrip,
          TripSegment currentTripSegment)
        {
            DataServiceFault fault;

            //Get all of the trips that have been assigned to the driver.
            //First, pending/missed trips dispatched to or acknowledged by the driver.
            //Followed by pending/missed trips assigned to the driver.
            //Followed by future trips assigned or dispatched to the driver.
            var allTripList = Common.GetAllTripsForDriver(dataService, settings, userCulture, userRoleIds,
                              employeeMaster.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (allTripList != null && allTripList.Count() > 0)
            {
                //If the driver's current trip is not the first in the list,we must resequence and ajust origins.
                if (allTripList.First().TripNumber != currentTrip.TripNumber)
                {
                    //Remove the current trip in the list.
                    allTripList.Remove(currentTrip);
                    //Insert it back at the top.
                    //Note: by removing and inserting the trip, we can then do additional update on the trip.
                    allTripList.Insert(0, currentTrip);

                    //Call the resequence method.
                    changeSetResult = Common.ResequenceTripsForDriver(dataService, settings,allTripList);

                    //Prepare to get al list of all segments for the set origins method.
                    //Get the last trip that was completed by the driver. We need this so that the origin of
                    //the first segment of driver's current trip can be set to the destination of the last
                    //segment of the last trip that the driver completed.
                    var lastCompletedTrip = Common.GetLastTripCompletedForDriver(dataService, settings, userCulture, userRoleIds,
                                            employeeMaster.EmployeeId, out fault);
                    if (lastCompletedTrip != null)
                    {
                        //Insert the segments of the last trip at the beginning of the list.
                        allTripList.Insert(0, lastCompletedTrip);
                    }

                    //Get all of the segments for all of the driver's trips keeping them in the same order as allTripList.
                    var allTripSegmentList = Common.GetAllTripSegmentsForTripList(dataService, settings, userCulture, 
                                             userRoleIds,  allTripList, out fault);
                    //Since we may be updating the first segment of the current trip, we need to remove the current
                    //trip segment and insert it back in at the same location.
                    //This remove/insert step prevents the exception:
                    //Exception saving 'TripSegment': NHibernate.NonUniqueObjectException: 
                    //a different object with the same identifier value was already associated with the session
                    int indexCurrentTripSegment = allTripSegmentList.IndexOf(currentTripSegment);
                    //Should always be in the list, but just in case...
                    if (-1 != indexCurrentTripSegment)
                    {
                        allTripSegmentList.RemoveAt(indexCurrentTripSegment);
                        allTripSegmentList.Insert(indexCurrentTripSegment, currentTripSegment);
                    }
                    //Now call the method of set the origins as needed. Only ones where the origins are changed
                    //will be updated.
                    changeSetResult = Common.SetTripSegmentOrigins(dataService, settings, allTripSegmentList);

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – "DRIVER OUT OF ORDER". 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.DriverOutOfOrder);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverEnrouteProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverEnrouteProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverEnrouteProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverEnrouteProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverEnrouteProcess.PowerId);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverEnrouteProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverEnrouteProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:DriverEnrouteProcess:Saving EventLog Record - Driver Out Of Order");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("DriverEnrouteProcess:Could not update EventLog for Driver {0} {1}.",
                                driverEnrouteProcess.EmployeeId, EventCommentConstants.DriverOutOfOrder);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return false;
                    }
                }//if (allTripList.First().TripNumber != driverEnrouteProcess.TripNumber)
            }//if (allTripList != null && allTripList.Count() > 0)
            return true;
        }

    }
}
