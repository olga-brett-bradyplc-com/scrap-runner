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
            ChangeSet<string, DriverEnrouteProcess> changeSet,
            bool persistChanges)
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " + driverEnrouteProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var tripRecord = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverEnrouteProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: " + driverEnrouteProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(tripRecord))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Enroute processing ends.",
                                        driverEnrouteProcess.TripNumber);
                        break;
                    }


                    ////////////////////////////////////////////////
                    //Get a list of all incomplete segments for the trip
                    var tripSegList = Common.GetTripSegmentsIncomplete(dataService, settings, userCulture, userRoleIds,
                                        driverEnrouteProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var tripSegmentRecord = (from item in tripSegList
                                             where item.TripSegNumber == driverEnrouteProcess.TripSegNumber
                                             select item).FirstOrDefault();
                    if (null == tripSegmentRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                            driverEnrouteProcess.TripNumber + "-" + driverEnrouteProcess.TripSegNumber));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Adjust odometer based on previously recorded odometer. 
                    //If odometer from mobile device (driverEnrouteProcess.Odometer) is less than PowerMaster.PowerOdometer, 
                    //use the PowerMaster.PowerOdometer instead of the odometer from the mobile device.
                    //Do we really want to do this?
                    //If so, we will need to adjust it here before we start using driverEnrouteProcess.Odometer

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
                    //Define the TripSegmentContainer for use later
                    TripSegmentContainer tripSegmentContainerRecord = new TripSegmentContainer();

                    ////////////////////////////////////////////////
                    //Update Container Status for all containers on the power unit.
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
                            //PowerId already set
                            //containerMaster.ContainerPowerId = driverEnrouteProcess.PowerId;
                            if (tripSegmentRecord.TripSegType == BasicTripTypeConstants.ReturnYard)
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Inbound;
                            }
                            else
                            {
                                containerMaster.ContainerStatus = ContainerStatusConstants.Outbound;
                                containerMaster.ContainerPrevCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                            }
                            containerMaster.ContainerCurrentTripNumber = driverEnrouteProcess.TripNumber;
                            containerMaster.ContainerCurrentTripSegNumber = driverEnrouteProcess.TripSegNumber;
                            containerMaster.ContainerCurrentTripSegType = tripSegmentRecord.TripSegType;
                            containerMaster.ContainerLastActionDateTime = driverEnrouteProcess.ActionDateTime;
                            containerMaster.ContainerCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                            containerMaster.ContainerCustType = tripSegmentRecord.TripSegDestCustType;
                            //Remove these since container is now on the move
                            containerMaster.ContainerPendingMoveDateTime = null;
                            containerMaster.ContainerLocation = null;
                            containerMaster.ContainerLatitude = null;
                            containerMaster.ContainerLongitude = null;

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
                                log.ErrorFormat("InsertContainerHistory failed: {0} during enorute request: {1}", fault.Message, driverEnrouteProcess);
                                break;
                            }

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
                                var tripSegmentContainerMax = Common.GetTripSegmentContainerLast(dataService, settings, userCulture, userRoleIds,
                                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber, out fault);
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
                                tripSegmentContainerRecord.TripNumber = driverEnrouteProcess.TripNumber;
                                tripSegmentContainerRecord.TripSegNumber = driverEnrouteProcess.TripSegNumber;
                            }
                            tripSegmentContainerRecord.TripSegContainerNumber = containerOnPowerId.ContainerNumber;
                            tripSegmentContainerRecord.TripSegContainerType = containerOnPowerId.ContainerType;
                            tripSegmentContainerRecord.TripSegContainerSize = containerOnPowerId.ContainerSize;
                            tripSegmentContainerRecord.TripSegContainerCommodityCode = containerOnPowerId.ContainerCommodityCode;
                            tripSegmentContainerRecord.TripSegContainerCommodityDesc = containerOnPowerId.ContainerCommodityDesc;
                            tripSegmentContainerRecord.TripSegContainerActionDateTime = driverEnrouteProcess.ActionDateTime;

                            //Do the update
                            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainerRecord);
                            log.DebugFormat("SRTEST:Saving TripSegmentContainerRecord for ContainerNumber:{0} - Enroute.",
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
                    //Since driver may have gone enroute, logged out and is now going enroute again
                    if (tripSegmentRecord.TripSegNumber == Constants.FirstSegment)
                    {
                        tripSegmentRecord.TripSegEndDateTime = null;
                        tripSegmentRecord.TripSegActualDriveEndDateTime = null;
                        tripSegmentRecord.TripSegActualStopStartDateTime = null;
                        tripSegmentRecord.TripSegActualStopEndDateTime = null;
                        tripSegmentRecord.TripSegOdometerEnd = null;
                    }

                    tripSegmentRecord.TripSegPowerId = driverEnrouteProcess.PowerId;
                    tripSegmentRecord.TripSegDriverId = driverEnrouteProcess.EmployeeId;
                    tripSegmentRecord.TripSegDriverName = Common.GetDriverName(employeeMaster);
                    //Driver has arrived. Segment start time begins. Drive time begins.
                    tripSegmentRecord.TripSegStartDateTime = driverEnrouteProcess.ActionDateTime;
                    tripSegmentRecord.TripSegActualDriveStartDateTime = driverEnrouteProcess.ActionDateTime;
                    tripSegmentRecord.TripSegOdometerStart = driverEnrouteProcess.Odometer;
                    tripSegmentRecord.TripSegStartLatitude = driverEnrouteProcess.Latitude;
                    tripSegmentRecord.TripSegStartLongitude = driverEnrouteProcess.Longitude;

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
                    log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Enroute.",
                                    tripSegmentRecord.TripNumber, tripSegmentRecord.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentRecord, log))
                    {
                        var s = string.Format("Could not update TripSegment for Trip:{0}-{1}.",
                            tripSegmentRecord.TripNumber, tripSegmentRecord.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Update Previous Segment End Dates.
                    //If not the first segment, set the previous segment's TripSegEndDateTime and TripSegActualStopEndDateTime
                    //to the current segment's TripSegStartDateTime (or driverEnrouteProcess.ActionDateTime).
                    //This was done in old SR to remove any gaps in time
                    //Do we really want to do this?
                    
                    ////////////////////////////////////////////////
                    //Update the TripSegmentMileage record.
                    if (tripSegmentRecord.TripSegNumber == Constants.FirstSegment)
                    {
                        //Delete any existing trip segment mileage records for this segment
                        //Driver must must starting this trip again. Maybe he went enroute,
                        //logged out and is now going enroute again.
                        //TODO
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
                        //Pass in false to not update ending odometer. 
                        //May not need this argument, since TripSegOdometerEnd is null at this point.
                        Common.InsertTripSegmentMileage(dataService, settings, userRoleIds, userCulture, log,
                            tripSegmentRecord, containersOnPowerId, false, tripSegmentMileageCount, out fault);
                        log.DebugFormat("SRTEST:Adding TripSegmentMileage Record for Trip:{0}-{1} - Enroute.",
                                        driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
                    }
                    else
                    {
                        //Do the update
                        changeSetResult = Common.UpdateTripSegmentMileageFromSegment
                            (dataService, settings, tripSegmentMileage, tripSegmentRecord, containersOnPowerId);
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
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
                    {
                        var s = string.Format("Could not update TripSegmentMileage for Trip:{0}-{1}.",
                              driverEnrouteProcess.TripNumber, driverEnrouteProcess.TripSegNumber);
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
                    }
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
                    log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Enroute.",
                                    tripRecord.TripNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripRecord, log))
                    {
                        var s = string.Format("Could not update Trip for Trip:{0}.",
                            tripRecord.TripNumber);
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
                    driverStatus.TripSegType = tripSegmentRecord.TripSegType;
                    driverStatus.TripAssignStatus = tripRecord.TripAssignStatus;
                    driverStatus.TripStatus = tripRecord.TripStatus;
                    driverStatus.TripSegStatus = tripSegmentRecord.TripSegStatus;
                    driverStatus.PowerId = driverEnrouteProcess.PowerId;
                    driverStatus.MDTId = driverEnrouteProcess.Mdtid;
                    driverStatus.ActionDateTime = driverEnrouteProcess.ActionDateTime;
                    driverStatus.Odometer = driverEnrouteProcess.Odometer;
                    driverStatus.GPSAutoGeneratedFlag = driverEnrouteProcess.GPSAutoFlag;
                    if (null == driverEnrouteProcess.Latitude ||
                        null == driverEnrouteProcess.Longitude)
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
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                                  driverEnrouteProcess.PowerId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (powerMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: " + driverEnrouteProcess.PowerId));
                        break;
                    }

                    powerMaster.PowerOdometer = driverEnrouteProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverEnrouteProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverEnrouteProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverEnrouteProcess.TripSegNumber;
                    powerMaster.PowerCurrentTripSegType = tripSegmentRecord.TripSegType;
                    //Set these to the destination. It is on the move.
                    powerMaster.PowerCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = tripSegmentRecord.TripSegDestCustType;

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
                        var customerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                                   tripSegmentRecord.TripSegOrigCustHostCode, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (powerMaster == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
                                              + tripSegmentRecord.TripSegOrigCustHostCode));
                            break;
                        }
                        customerMaster.CustAutoGPSFlag = driverEnrouteProcess.GPSAutoFlag;

                        //Do the update
                        changeSetResult = Common.UpdateCustomerMaster(dataService, settings, customerMaster);
                        log.DebugFormat("SRTEST:Saving CustomerMaster Record for CustHostCode:{0} - Enroute.",
                                        tripSegmentRecord.TripSegOrigCustHostCode);
                        if (Common.LogChangeSetFailure(changeSetResult, customerMaster, log))
                        {
                            var s = string.Format("Could not update CustomerMaster for CustHostCode:{0}.",
                                                  tripSegmentRecord.TripSegOrigCustHostCode);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Send GPS Customer Host Code packet to tracker. 
                    //Send GPS Location Status packet to tracker. (NOT USED)
                    //TODO



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
                    sbComment.Append(tripSegmentRecord.TripSegDestCustState);
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
