using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.RecordTypes;
using Brady.ScrapRunner.DataService.Util;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Enums;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;
using NHibernate;
using NHibernate.Util;
using System.Text;

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
            ChangeSet<string, DriverArriveProcess> changeSet,
            bool persistChanges)
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

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security, pipelines, etc. 
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only process one driver arrive at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    var driverArriveProcess = (DriverArriveProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process arrive for DriverId: " + driverArriveProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Util.Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " + driverArriveProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var tripRecord = Util.Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: " + driverArriveProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Util.Common.IsTripComplete(tripRecord))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Arrive processing ends.",
                                        driverArriveProcess.TripNumber);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all incomplete segments for the trip
                    var tripSegList = Util.Common.GetTripSegmentsIncomplete(dataService, settings, userCulture, userRoleIds,
                                        driverArriveProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var tripSegmentRecord = (from item in tripSegList
                              where item.TripSegNumber == driverArriveProcess.TripSegNumber
                              select item).FirstOrDefault();
                    if (null == tripSegmentRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                            driverArriveProcess.TripNumber + "-" + driverArriveProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Adjust odometer based on previously recorded odometer. 
                    //If odometer from mobile device (driverArriveProcess.Odometer) is less than PowerMaster.PowerOdometer, 
                    //use the PowerMaster.PowerOdometer instead of the odometer from the mobile device.
                    //Do we really want to do this?
                    //If so, we will need to adjust it here before we start using driverArriveProcess.Odometer

                    ////////////////////////////////////////////////
                    //Get a list of all containers for the segment
                    var tripSegContainerList = Util.Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                        driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Define the TripSegmentContainer for use later
                    TripSegmentContainer tripSegmentContainerRecord = new TripSegmentContainer();

                    ////////////////////////////////////////////////
                    //Update Container Status for all containers on the power unit.
                    var containersOnPowerId = Util.Common.GetContainersForPowerId(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.PowerId, out fault);
                    if (null != containersOnPowerId && containersOnPowerId.Count() > 0)
                    {
                        //For each container, update the ContainerMaster and the TripSegmentContainer table
                        foreach (var containerOnPowerId in containersOnPowerId)
                        {
                            var containerMaster = Util.Common.GetContainer(dataService, settings, userCulture, userRoleIds,
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
                            //PowerId already set. Container is still on the power id.
                            //containerMaster.ContainerPowerId = driverArriveProcess.PowerId;
                            if (tripSegmentRecord.TripSegDestCustType == CustomerTypeConstants.Yard)
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Yard;
                            }
                            else
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.CustomerSite;
                                containerMaster.ContainerPrevCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                            }
                            containerMaster.ContainerCurrentTripNumber = driverArriveProcess.TripNumber;
                            containerMaster.ContainerCurrentTripSegNumber = driverArriveProcess.TripSegNumber;
                            containerMaster.ContainerLastActionDateTime = driverArriveProcess.ActionDateTime;
                            containerMaster.ContainerCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                            containerMaster.ContainerCustType = tripSegmentRecord.TripSegDestCustType;
                            //Remove these since container has not yet been set down, still on the move.
                            containerMaster.ContainerPendingMoveDateTime = null;
                            containerMaster.ContainerLocation = null;
                            containerMaster.ContainerLatitude = null;
                            containerMaster.ContainerLongitude = null;

                            //Do the update
                            changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                            log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - Arrive.",
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
                            //TODO


                            ////////////////////////////////////////////////
                            //Update TripSegmentContainer table.
                            if (null != tripSegContainerList && tripSegContainerList.Count() > 0)
                            {
                                //First, try to find a container in the list that matches the container number on the power unit.
                                tripSegmentContainerRecord = (from item in tripSegContainerList
                                              where item.TripSegContainerNumber == containerMaster.ContainerNumber
                                              select item).FirstOrDefault();
                                if (null == tripSegmentContainerRecord)
                                {
                                    //Otherwise find a container record with no container number
                                    tripSegmentContainerRecord = (from item in tripSegContainerList
                                              where item.TripSegContainerNumber == null
                                              select item).FirstOrDefault();
                                }
                            }
                            //If still not found, then add a new one
                            if (null == tripSegmentContainerRecord)
                            {
                                //Set up a new record
                                tripSegmentContainerRecord = new TripSegmentContainer();

                                //Look up the last trip segment container for this trip and segment
                                //Use this to calculate the next sequence number.
                                var tripSegmentContainerMax = Util.Common.GetTripSegmentContainerLast(dataService, settings, userCulture, userRoleIds,
                                                        driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                                if (null != fault)
                                {
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                    break;
                                }
                                if (null == tripSegmentContainerMax)
                                {
                                    tripSegmentContainerRecord.TripSegContainerSeqNumber = 0;
                                }
                                else
                                {
                                    tripSegmentContainerRecord.TripSegContainerSeqNumber = ++tripSegmentContainerMax.TripSegContainerSeqNumber;
                                }
                                tripSegmentContainerRecord.TripNumber = driverArriveProcess.TripNumber;
                                tripSegmentContainerRecord.TripSegNumber = driverArriveProcess.TripSegNumber;
                            }
                            tripSegmentContainerRecord.TripSegContainerNumber = containerOnPowerId.ContainerNumber;
                            tripSegmentContainerRecord.TripSegContainerType = containerOnPowerId.ContainerType;
                            tripSegmentContainerRecord.TripSegContainerSize = containerOnPowerId.ContainerSize;
                            tripSegmentContainerRecord.TripSegContainerCommodityCode = containerOnPowerId.ContainerCommodityCode;
                            tripSegmentContainerRecord.TripSegContainerCommodityDesc = containerOnPowerId.ContainerCommodityDesc;
                            tripSegmentContainerRecord.TripSegContainerActionDateTime = driverArriveProcess.ActionDateTime;

                            //Do the update
                            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainerRecord);
                            log.DebugFormat("SRTEST:Saving TripSegmentContainerRecord for ContainerNumber:{0} - Arrive.",
                                            tripSegmentContainerRecord.TripSegContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainerRecord, log))
                            {
                                var s = string.Format("Could not update TripSegmentContainerRecord for ContainerNumber:{0}.",
                                         tripSegmentContainerRecord.TripSegContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }// end of foreach...
                    }//end of if (null != containersOnPowerId...

                    ////////////////////////////////////////////////
                    //Update the TripSegment record.
                    //On first segment, remove previously recorded Trip Segment times and odometers.
                    //Since driver may have gone enroute/arrived, logged out and has gone enroutearrive again
                    if (tripSegmentRecord.TripSegNumber == Constants.FirstSegment)
                    {
                        tripSegmentRecord.TripSegEndDateTime = null;
                        tripSegmentRecord.TripSegActualStopEndDateTime = null;
                    }

                    tripSegmentRecord.TripSegPowerId = driverArriveProcess.PowerId;
                    tripSegmentRecord.TripSegDriverId = driverArriveProcess.EmployeeId;
                    tripSegmentRecord.TripSegDriverName = Util.Common.GetDriverName(employeeMaster);
                    //Driver has arrived. Drive time ends. Stop time begins.
                    tripSegmentRecord.TripSegActualDriveEndDateTime = driverArriveProcess.ActionDateTime;
                    tripSegmentRecord.TripSegActualStopStartDateTime = driverArriveProcess.ActionDateTime;
                    tripSegmentRecord.TripSegOdometerEnd = driverArriveProcess.Odometer;
                    tripSegmentRecord.TripSegEndLatitude = driverArriveProcess.Latitude;
                    tripSegmentRecord.TripSegEndLongitude = driverArriveProcess.Longitude;

                    //Calculate the drive minutes:DriveEndDateTime - DriveStartDateTime
                    tripSegmentRecord.TripSegActualDriveMinutes = 
                       (tripSegmentRecord.TripSegActualDriveEndDateTime - tripSegmentRecord.TripSegActualDriveStartDateTime).Value.Minutes;

                    //Update TripSegment Primary Container Information from first TripSegmentContainer information. 
                    //Use the list instead of requerying the database. Values are not yet saved.
                    if (tripSegContainerList.Count() > 0)
                    {
                        var firstTripSegmentContainer = tripSegContainerList.First();

                        //Only if there is a container number
                        if (null != firstTripSegmentContainer.TripSegContainerNumber)
                        {
                            tripSegmentRecord.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                            tripSegmentRecord.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                            tripSegmentRecord.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                            tripSegmentRecord.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            tripSegmentRecord.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            tripSegmentRecord.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;
                        }
                    }
                    //Determine number of TripSegmentContainer records. Use the list.
                    tripSegmentRecord.TripSegContainerQty = tripSegContainerList.Count();

                    //Do the update
                    changeSetResult = Common.UpdateTripSegment(dataService, settings, tripSegmentRecord);
                    log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Arrive.",
                                    tripSegmentRecord.TripNumber, tripSegmentRecord.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentRecord, log))
                    {
                        var s = string.Format("Could not update TripSegment for Trip:{0}-{1}.",
                            tripSegmentRecord.TripNumber, tripSegmentRecord.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the odometers of subsequent trip segments with the same destination.
                    string hostcode = null;
                    foreach (var nextTripSegmentRecord in tripSegList)
                    {
                        if (nextTripSegmentRecord.TripSegNumber == driverArriveProcess.TripSegNumber)
                        {
                            hostcode = nextTripSegmentRecord.TripSegDestCustHostCode;
                        }
                        //The first string follows the second string in the sort order.
                        else if (1 == nextTripSegmentRecord.TripSegNumber.CompareTo(driverArriveProcess.TripSegNumber))
                        {
                            if (hostcode == nextTripSegmentRecord.TripSegDestCustHostCode)
                            {
                                //Since destination of the next segment is the same as the current segment, we have arrived at both.
                                nextTripSegmentRecord.TripSegOdometerStart = nextTripSegmentRecord.TripSegOdometerEnd = driverArriveProcess.Odometer;

                                //Do the update
                                changeSetResult = Common.UpdateTripSegment(dataService, settings, nextTripSegmentRecord);
                                log.DebugFormat("SRTEST:Saving Odometer in TripSegment Record for Trip:{0}-{1} - Arrive.",
                                                nextTripSegmentRecord.TripNumber, nextTripSegmentRecord.TripSegNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, nextTripSegmentRecord, log))
                                {
                                    var s = string.Format("Could not update Odometer in TripSegment for Trip:{0}-{1}.",
                                        nextTripSegmentRecord.TripNumber, nextTripSegmentRecord.TripSegNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
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
                    //Update the TripSegmentMileage record.
                    //Normally for arrives, there should be an open-ended record to update 
                    var tripSegmentMileage = Util.Common.GetTripSegmentMileageOpenEndedLast(dataService, settings, userCulture, userRoleIds,
                                        driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                   if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripSegmentMileage)
                    {
                        //Set up a new record
                        tripSegmentMileage = new TripSegmentMileage();

                        tripSegmentMileage.TripNumber = driverArriveProcess.TripNumber;
                        tripSegmentMileage.TripSegNumber = driverArriveProcess.TripSegNumber;

                        //Look up the last trip segment mileage for this trip and segment to calculate the next sequence number
                        var tripSegmentMileageMax = Util.Common.GetTripSegmentMileageLast(dataService, settings, userCulture, userRoleIds,
                                                driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == tripSegmentMileageMax)
                        {
                            tripSegmentMileage.TripSegMileageSeqNumber = 0;
                        }
                        else
                        {
                            tripSegmentMileage.TripSegMileageSeqNumber = ++tripSegmentMileageMax.TripSegMileageSeqNumber;
                        }
                        //We need to set a start odometer
                        tripSegmentMileage.TripSegMileageOdometerStart = driverArriveProcess.Odometer;
                    }
                    tripSegmentMileage.TripSegMileageOdometerEnd = driverArriveProcess.Odometer;
                    //We don't need to set State and Country. It should have been already set. But just in case....
                    if (null == tripSegmentMileage.TripSegMileageState)
                        tripSegmentMileage.TripSegMileageState = tripSegmentRecord.TripSegDestCustState;
                    if (null == tripSegmentMileage.TripSegMileageCountry)
                        tripSegmentMileage.TripSegMileageCountry = tripSegmentRecord.TripSegDestCustCountry;
                    tripSegmentMileage.TripSegMileagePowerId = tripSegmentRecord.TripSegPowerId;
                    tripSegmentMileage.TripSegMileageDriverId = tripSegmentRecord.TripSegDriverId;
                    tripSegmentMileage.TripSegMileageDriverName = tripSegmentRecord.TripSegDriverName;

                    //Set the loaded flag
                    tripSegmentMileage.TripSegLoadedFlag = Constants.No;
                    //If any container on the truck is loaded, these miles are loaded miles
                    var loaded = from item in containersOnPowerId
                                 where item.ContainerContents == ContainerContentsConstants.Loaded
                                 select item;
                    if (null != loaded && loaded.Count() > 0)
                    {
                        tripSegmentMileage.TripSegLoadedFlag = Constants.Yes;
                    }
                    else
                    {
                        //Also consider the miles to be loaded miles if the driver is dropping a full container,
                        //unloading a container, or driving to an independent scale.
                        var types = new List<string> {BasicTripTypeConstants.DropFull,
                                                  BasicTripTypeConstants.Scale,
                                                  BasicTripTypeConstants.Unload };
                        if (types.Contains(tripSegmentRecord.TripSegType))
                            tripSegmentMileage.TripSegLoadedFlag = Constants.Yes;
                    }

                    //Do the update
                    changeSetResult = Common.UpdateTripSegmentMileage(dataService, settings, tripSegmentMileage);
                    log.DebugFormat("SRTEST:Saving TripSegmentMileage Record for Trip:{0}-{1} - Arrive.",
                                    driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
                    {
                        var s = string.Format("Could not update TripSegmentMileage for Trip:{0}-{1}.",
                              driverArriveProcess.TripNumber, driverArriveProcess.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    //Update Primary Container Information if this is the first TripSegment.
                    //but only if the container number in the TripSegmentContainer record is not null. 
                    if (tripSegmentRecord.TripSegNumber == Constants.FirstSegment &&
                        null != tripSegmentRecord.TripSegPrimaryContainerNumber)
                    {
                        tripRecord.TripPrimaryContainerNumber = tripSegmentRecord.TripSegPrimaryContainerNumber;
                        tripRecord.TripPrimaryContainerType = tripSegmentRecord.TripSegPrimaryContainerType;
                        tripRecord.TripPrimaryContainerSize = tripSegmentRecord.TripSegPrimaryContainerSize;
                        tripRecord.TripPrimaryCommodityCode = tripSegmentRecord.TripSegPrimaryContainerCommodityCode;
                        tripRecord.TripPrimaryCommodityDesc = tripSegmentRecord.TripSegPrimaryContainerCommodityDesc;
                        tripRecord.TripPrimaryContainerLocation = tripSegmentRecord.TripSegPrimaryContainerLocation;
                        tripRecord.TripStartedDateTime = tripSegmentRecord.TripSegStartDateTime;
                        if (tripSegmentRecord.TripSegContainerQty > 1)
                        {
                            tripRecord.TripMultContainerFlag = Constants.Yes;
                        }
                        else
                        {
                            tripRecord.TripMultContainerFlag = Constants.No;
                        }
                        tripRecord.TripInProgressFlag = Constants.Yes;

                        changeSetResult = Common.UpdateTrip(dataService, settings, tripRecord);
                        log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Arrive.",
                                        tripRecord.TripNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, tripRecord, log))
                        {
                            var s = string.Format("Could not update Trip for Trip:{0}.",
                                tripRecord.TripNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. 
                    var driverStatus = Util.Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " + driverArriveProcess.EmployeeId));
                        break;
                    }
                    driverStatus.Status = DriverStatusSRConstants.Arrive;
                    driverStatus.TripNumber = driverArriveProcess.TripNumber;
                    driverStatus.TripSegNumber = driverArriveProcess.TripSegNumber;
                    driverStatus.TripSegType = tripSegmentRecord.TripSegType;
                    driverStatus.TripAssignStatus = tripRecord.TripAssignStatus;
                    driverStatus.TripStatus = tripRecord.TripStatus;
                    driverStatus.TripSegStatus = tripSegmentRecord.TripSegStatus;
                    driverStatus.PowerId = driverArriveProcess.PowerId;
                    driverStatus.MDTId = driverArriveProcess.Mdtid;
                    driverStatus.ActionDateTime = driverArriveProcess.ActionDateTime;
                    driverStatus.Odometer = driverArriveProcess.Odometer;
                    driverStatus.GPSAutoGeneratedFlag = driverArriveProcess.GPSAutoFlag;
                    if (null == driverArriveProcess.Latitude ||
                        null == driverArriveProcess.Longitude)
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
                        var s = string.Format("Could not update DriverStatus for DriverId:{0}.",
                            driverStatus.EmployeeId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table.
                    //TODO

                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 
                    var powerMaster = Util.Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                                  driverArriveProcess.PowerId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (powerMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: " + driverArriveProcess.PowerId));
                        break;
                    }

                    powerMaster.PowerOdometer = driverArriveProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverArriveProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverArriveProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverArriveProcess.TripSegNumber;
                    //Set these to the trip destination host code and type.
                    powerMaster.PowerCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = tripSegmentRecord.TripSegDestCustType;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - Arrive.",
                                    driverArriveProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("Could not update PowerMaster for PowerId:{0}.",
                                              driverArriveProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    //TODO

                    ////////////////////////////////////////////////
                    //Update CustomerMaster.  If driver auto-arrived, update auto-arrive flag for the destination customer.
                    if (driverArriveProcess.GPSAutoFlag == Constants.Yes)
                    {
                        var customerMaster = Util.Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                                   tripSegmentRecord.TripSegDestCustHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (customerMaster == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
                                              + tripSegmentRecord.TripSegDestCustHostCode));
                            break;
                        }
                        customerMaster.CustAutoGPSFlag = driverArriveProcess.GPSAutoFlag;

                        //Do the update
                        changeSetResult = Common.UpdateCustomerMaster(dataService, settings, customerMaster);
                        log.DebugFormat("SRTEST:Saving CustomerMaster Record for CustHostCode:{0} - Arrive.",
                                        tripSegmentRecord.TripSegDestCustHostCode);
                        if (Common.LogChangeSetFailure(changeSetResult, customerMaster, log))
                        {
                            var s = string.Format("Could not update CustomerMaster for CustHostCode:{0}.",
                                                  tripSegmentRecord.TripSegDestCustHostCode);
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
                    sbComment.Append(tripSegmentMileage.TripSegMileageState);
                    sbComment.Append(" Odom:");
                    sbComment.Append(driverArriveProcess.Odometer);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverArriveProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated for logins in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetDriverName(employeeMaster),
                        EventTripNumber = driverArriveProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated for enroutes in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Arrive");
                    //if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    //{
                    //    var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                    //                         driverStatus.EmployeeId, EventCommentConstants.ReceivedDriverLogin);
                    //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    //    break;
                    //}


                    ////////////////////////////////////////////////
                    // Check if trip is out of order. 
                    // If trip is out or order, resequence his trips.
                    // Normally we would then send a resequence trips message to  driver.
                    // TODO


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

            return changeSetResult;
        }
    }
}