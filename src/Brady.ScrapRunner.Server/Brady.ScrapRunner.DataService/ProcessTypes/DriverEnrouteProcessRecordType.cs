using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Util;
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
            // We only process one driver enroute at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    int tripSegmentMileageCount = 0;
                    int containerHistoryInsertCount = 0;
                    int driverHistoryInsertCount = 0;
                    int powerHistoryInsertCount = 0;

                    var driverEnrouteProcess = (DriverEnrouteProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process enroute for DriverId: " + driverEnrouteProcess.EmployeeId));
                        break;
                    }

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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " 
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: " 
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


                    ////////////////////////////////////////////////
                    //Get a list of all  segments for the trip
                    var tripSegList = Common.GetTripSegments(dataService, settings, userCulture, userRoleIds,
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                            driverEnrouteProcess.TripNumber + "-" + driverEnrouteProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Adjust odometer based on previously recorded odometer. 
                    //If odometer from mobile device (driverEnrouteProcess.Odometer) is less than PowerMaster.PowerOdometer, 
                    //use the PowerMaster.PowerOdometer instead of the odometer from the mobile device.
                    //Adjust odometer here before we start using driverEnrouteProcess.Odometer
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: "
                                        + driverEnrouteProcess.PowerId));
                        break;
                    }

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
                    //Get a list of all containers for the segment
                    var tripSegContainerList = new List<TripSegmentContainer>();
                    tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
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
                                                  driverEnrouteProcess.PowerId, out fault);
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
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ContainerNumber: " + containerOnPowerId.ContainerNumber));
                                break;
                            }
                            //PowerId already set.
                            //containerMaster.ContainerPowerId = driverEnrouteProcess.PowerId;

                            //Set the container status. It is either Inbound to a yard or outbound to a customer site.
                            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Inbound;
                            }
                            else
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Outbound;
                                containerMaster.ContainerPrevCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                            }

                            containerMaster.ContainerCurrentTripNumber = driverEnrouteProcess.TripNumber;
                            containerMaster.ContainerCurrentTripSegNumber = driverEnrouteProcess.TripSegNumber;
                            containerMaster.ContainerCurrentTripSegType = currentTripSegment.TripSegType;
                            containerMaster.ContainerLastActionDateTime = driverEnrouteProcess.ActionDateTime;
                            containerMaster.ContainerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                            containerMaster.ContainerCustType = currentTripSegment.TripSegDestCustType;

                            //Remove these since container is now on the move
                            containerMaster.ContainerPendingMoveDateTime = null;
                            containerMaster.ContainerLocation = null;
                            containerMaster.ContainerLatitude = null;
                            containerMaster.ContainerLongitude = null;

                            //When the driver goes enroute on a RT segment, set the Inbound terminal to the serving
                            //terminal of the destination customer. This will be removed when the driver arrives.
                            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                            {
                                containerMaster.ContainerInboundTerminalId = destCustomerMaster.ServingTerminalId;
                            }

                            //Do the update
                            changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                            log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - Enroute.",
                                            containerOnPowerId.ContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                            {
                                var s = string.Format("Could not update ContainerMaster for ContainerNumber:{0}.",
                                         containerOnPowerId.ContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }

                            ////////////////////////////////////////////////
                            //Add record to Container History. 
                            if (!Common.InsertContainerHistory(dataService, settings, containerMaster,
                                ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                log.ErrorFormat("InsertContainerHistory failed: {0} during enroute request: {1}", fault.Message, driverEnrouteProcess);
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
                            tripSegmentContainer.TripSegContainerCommodityCode = containerOnPowerId.ContainerCommodityCode;
                            tripSegmentContainer.TripSegContainerCommodityDesc = containerOnPowerId.ContainerCommodityDesc;
                            tripSegmentContainer.TripSegContainerActionDateTime = driverEnrouteProcess.ActionDateTime;

                            //Do the update
                            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
                            log.DebugFormat("SRTEST:Saving TripSegmentContainerRecord for ContainerNumber:{0} - Enroute.",
                                            tripSegmentContainer.TripSegContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
                            {
                                var s = string.Format("Could not update TripSegmentContainerRecord for ContainerNumber:{0}.",
                                         tripSegmentContainer.TripSegContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
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
                    currentTripSegment.TripSegDriverName = Common.GetDriverName(employeeMaster);
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
                    log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Enroute.",
                                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
                    {
                        var s = string.Format("Could not update TripSegment for Trip:{0}-{1}.",
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
                                                   where -1 == item.TripSegNumber.CompareTo(driverEnrouteProcess.TripSegNumber)
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
                            log.DebugFormat("SRTEST:Saving previous TripSegment Record for Trip:{0}-{1} - Enroute.",
                                            previousTripSegment.TripNumber, previousTripSegment.TripSegNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, previousTripSegment, log))
                            {
                                var s = string.Format("Could not update previous TripSegment for Trip:{0}-{1}.",
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
                        //TODO: Delete any existing trip segment mileage records for this segment
                        //Driver might be starting this trip again. Maybe he went enroute/arrived, logged out and is now going enroute again.
                    }

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
                        Common.InsertTripSegmentMileage(dataService, settings, userRoleIds, userCulture, log,
                            currentTripSegment, containersOnPowerId, false, tripSegmentMileageCount, out fault);
                        log.DebugFormat("SRTEST:Adding TripSegmentMileage Record for Trip:{0}-{1} - Enroute.",
                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
                    }
                    else
                    {
                        //Do the update
                        changeSetResult = Common.UpdateTripSegmentMileageFromSegment
                            (dataService, settings, tripSegmentMileage, currentTripSegment, containersOnPowerId);
                        log.DebugFormat("SRTEST:Saving TripSegmentMileage Record for Trip:{0}-{1} - Enroute.",
                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
                        {
                            var s = string.Format("Could not update TripSegmentMileage for Trip:{0}-{1}.",
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
                    log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Enroute.",
                                    currentTrip.TripNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
                    {
                        var s = string.Format("Could not update Trip for Trip:{0}.",
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
                    log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Enroute.",
                                    driverStatus.EmployeeId);
                    if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                    {
                        var s = string.Format("Could not update DriverStatus for DriverId:{0}.",
                            driverStatus.EmployeeId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }


                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table.
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during enorute request: {1}", fault.Message, driverEnrouteProcess);
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
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - Enroute.",
                                    driverEnrouteProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("Could not update PowerMaster for PowerId:{0}.",
                                              driverEnrouteProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, 
                        ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerHistory failed: {0} during enorute request: {1}", fault.Message, driverEnrouteProcess);
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
                        log.DebugFormat("SRTEST:Saving CustomerMaster Record for CustHostCode:{0} - Enroute.",
                                        currentTripSegment.TripSegOrigCustHostCode);
                        if (Common.LogChangeSetFailure(changeSetResult, originCustomerMaster, log))
                        {
                            var s = string.Format("Could not update CustomerMaster for CustHostCode:{0}.",
                                                  currentTripSegment.TripSegOrigCustHostCode);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //TODO: Send GPS Customer Host Code packet to tracker. 

                    ////////////////////////////////////////////////
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
                        //These are not populated for logins in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetDriverName(employeeMaster),
                        EventTripNumber = driverEnrouteProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated for enroutes in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Enroute");
                    //if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    //{
                    //    var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                    //                         driverStatus.EmployeeId, EventCommentConstants.ReceivedDriverLogin);
                    //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    //    break;
                    //}


                    ////////////////////////////////////////////////
                    // TODO: Check if trip is out of order. 
                    // If trip is out or order, resequence his trips.
                    // Normally we would then send a resequence trips message to  driver.


                } //end of foreach...
            }//end of if (!changeSetResult.Failed...

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
                log.Debug("SRTEST:Transaction Rollback - Enroute");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Enroute");
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
