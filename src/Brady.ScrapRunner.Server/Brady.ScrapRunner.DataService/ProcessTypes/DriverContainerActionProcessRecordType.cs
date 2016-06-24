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
    /// Processing for a driver completing an action on a container.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverContainerActionProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverContainerActionProcess")]
    public class DriverContainerActionProcessRecordType : ChangeableRecordType
        <DriverContainerActionProcess, string, DriverContainerActionProcessValidator, DriverContainerActionProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverContainerActionProcess, DriverContainerActionProcess>();
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
                        ChangeSet<string, DriverContainerActionProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
        /// <summary>
        /// Perform the driver container action processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverContainerActionProcess> changeSet, ProcessChangeSetSettings settings)
        {
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
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    int containerHistoryInsertCount = 0;

                    var driverContainerActionProcess = (DriverContainerActionProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverContainerActionProcess backfillDriverContainerActionProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverContainerActionProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverContainerActionProcess, driverContainerActionProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process container action for DriverId: "
                                        + driverContainerActionProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Validate the ActionType
                    var actions = new List<string> {ContainerActionTypeConstants.Done,
                                                    ContainerActionTypeConstants.Exception,
                                                    ContainerActionTypeConstants.Review,
                                                    ContainerActionTypeConstants.Load,
                                                    ContainerActionTypeConstants.Dropped,
                                                    ContainerActionTypeConstants.Added};
                    if (!actions.Contains(driverContainerActionProcess.ActionType))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ActionType: "
                                        + driverContainerActionProcess.ActionType));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //DriverContainerActionProcess has been called
                    log.DebugFormat("SRTEST:DriverContainerActionProcess Called by {0}", key);
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Added)
                    {
                        log.DebugFormat("SRTEST:DriverContainerActionProcess:ADDED Driver:{0} Container:{1} DT:{2} MethodOfEntry:{3} Contents:{4} Lat:{5} Lon:{6}",
                                         driverContainerActionProcess.EmployeeId, driverContainerActionProcess.ContainerNumber,
                                         driverContainerActionProcess.ActionDateTime, driverContainerActionProcess.MethodOfEntry,
                                         driverContainerActionProcess.ContainerContents, driverContainerActionProcess.Latitude,
                                         driverContainerActionProcess.Longitude);
                    }
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Load)
                    {
                        log.DebugFormat("SRTEST:DriverContainerActionProcess:LOAD Driver:{0} Container:{1} DT:{2} MethodOfEntry:{3}",
                                         driverContainerActionProcess.EmployeeId, driverContainerActionProcess.ContainerNumber,
                                         driverContainerActionProcess.ActionDateTime, driverContainerActionProcess.MethodOfEntry);
                    }
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Dropped)
                    {
                        log.DebugFormat("SRTEST:DriverContainerActionProcess:DROPPED Driver:{0} Container:{1} DT:{2} MethodOfEntry:{3} Contents:{4} Lat:{5} Lon:{6}",
                                         driverContainerActionProcess.EmployeeId, driverContainerActionProcess.ContainerNumber,
                                         driverContainerActionProcess.ActionDateTime, driverContainerActionProcess.MethodOfEntry,
                                         driverContainerActionProcess.ContainerContents, driverContainerActionProcess.Latitude,
                                         driverContainerActionProcess.Longitude);
                    }
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Done)
                    {
                        log.DebugFormat("SRTEST:DriverContainerActionProcess:DONE Driver:{0} Container:{1} DT:{2} MethodOfEntry:{3} Contents:{4} Lat:{5} Lon:{6}",
                                         driverContainerActionProcess.EmployeeId, driverContainerActionProcess.ContainerNumber,
                                         driverContainerActionProcess.ActionDateTime, driverContainerActionProcess.MethodOfEntry,
                                         driverContainerActionProcess.ContainerContents, driverContainerActionProcess.Latitude,
                                         driverContainerActionProcess.Longitude);
                    }
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Review)
                    {
                        log.DebugFormat("SRTEST:DriverContainerActionProcess:REVIEW Driver:{0} Container:{1} DT:{2} MethodOfEntry:{3} Contents:{4} Lat:{5} Lon:{6} Code:{7} Desc:{8}",
                                         driverContainerActionProcess.EmployeeId, driverContainerActionProcess.ContainerNumber,
                                         driverContainerActionProcess.ActionDateTime, driverContainerActionProcess.MethodOfEntry,
                                         driverContainerActionProcess.ContainerContents, driverContainerActionProcess.Latitude,
                                         driverContainerActionProcess.Longitude, driverContainerActionProcess.ActionCode,
                                         driverContainerActionProcess.ActionDesc);
                    }
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Exception)
                    {
                        log.DebugFormat("SRTEST:DriverContainerActionProcess:EXCEPTION Driver:{0} Container:{1} DT:{2} MethodOfEntry:{3} Contents:{4} Lat:{5} Lon:{6} Code:{7} Desc:{8}",
                                         driverContainerActionProcess.EmployeeId, driverContainerActionProcess.ContainerNumber,
                                         driverContainerActionProcess.ActionDateTime, driverContainerActionProcess.MethodOfEntry,
                                         driverContainerActionProcess.ContainerContents, driverContainerActionProcess.Latitude,
                                         driverContainerActionProcess.Longitude, driverContainerActionProcess.ActionCode,
                                         driverContainerActionProcess.ActionDesc);
                    }
                    ////////////////////////////////////////////////
                    //Exceptions may not have a ContainerNumber. Otherwise it is required
                    if (driverContainerActionProcess.ActionType != ContainerActionTypeConstants.Exception)
                    {
                        if (driverContainerActionProcess.ContainerNumber == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("ContainerNumber is required for ActionType:"
                                                              + driverContainerActionProcess.ActionType));
                            break;
                        }
                        if (driverContainerActionProcess.ActionType != ContainerActionTypeConstants.Added)
                        {
                            if (driverContainerActionProcess.MethodOfEntry == null)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("MethodOfEntry is required for ActionType:"
                                                                  + driverContainerActionProcess.ActionType));
                                break;
                            }
                            if (driverContainerActionProcess.MethodOfEntry != ContainerMethodOfEntry.Manual &&
                                driverContainerActionProcess.MethodOfEntry != ContainerMethodOfEntry.Scanned)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid MethodOfEntry:"
                                                                  + driverContainerActionProcess.MethodOfEntry));
                                break;
                            }
                        }
                    }

                    ////////////////////////////////////////////////
                    //Trip Number is required for Action Type:D=Done,E=Exception,R=Review,A=Added.
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Done ||
                        driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Exception ||
                        driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Review ||
                        driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Added)
                    {
                        if (driverContainerActionProcess.TripNumber == null ||
                            driverContainerActionProcess.TripSegNumber == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("TripNumber and TripSegNumber are required for ActionType:"
                                                              + driverContainerActionProcess.ActionType));
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Action Code and Action Desc are required for Action Type:E=Exception,R=Review.
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Exception ||
                        driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Review)
                    {
                        if (driverContainerActionProcess.ActionCode == null ||
                            driverContainerActionProcess.ActionDesc == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Action Code and Desc are required for ActionType:"
                                                              + driverContainerActionProcess.ActionType));
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverContainerActionProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: "
                                        + driverContainerActionProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the PowerMaster record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                      driverContainerActionProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: "
                                        + driverContainerActionProcess.PowerId));
                        break;
                    }

                    /////////////////////////////////////////////////
                    //Split the processing into one of two functions:
                    //Load/drop action type
                    if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Load ||
                        driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Dropped)
                    {
                        if (!ContainerLoadDrop(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverContainerActionProcess, employeeMaster, containerHistoryInsertCount))
                        {
                            break;
                        }
                
                    }
                    //Added action type
                    else if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Added)
                    {
                        if (!ContainerAdd(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverContainerActionProcess, employeeMaster))
                        {
                            break;
                        }
                    }
                    //Exception action type
                    else if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Exception)
                    {
                        if (!ContainerException(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverContainerActionProcess, employeeMaster))
                        {
                            break;
                        }
                    }
                    //Done, Review action type
                    else
                    {
                        if (!ContainerDone(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverContainerActionProcess, employeeMaster, containerHistoryInsertCount))
                        {
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Container Action. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverContainerAction);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverContainerActionProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverContainerActionProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverContainerActionProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverContainerActionProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverContainerActionProcess.PowerId);
                    if (driverContainerActionProcess.ContainerNumber != null)
                    {
                        sbComment.Append(" Container:");
                        sbComment.Append(driverContainerActionProcess.ContainerNumber);
                    }
                    sbComment.Append(" Action:");
                    sbComment.Append(driverContainerActionProcess.ActionType);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverContainerActionProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverContainerActionProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Container Action");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                                driverContainerActionProcess.EmployeeId, EventCommentConstants.ReceivedDriverContainerAction);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                }//end of foreach 
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
                log.Debug("SRTEST:Transaction Rollback - Container Action");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Container Action");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }
        /// <summary>
        ///  Processing for Container Load/Drop actions:
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverContainerActionProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="containerHistoryInsertCount"></param>
        /// <returns></returns>
        public bool ContainerLoadDrop(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
           DriverContainerActionProcess driverContainerActionProcess, EmployeeMaster employeeMaster, 
           int containerHistoryInsertCount)
        {
            DataServiceFault fault = null;
            ////////////////////////////////////////////////
            //Update Container Information
            var containerMaster = Common.GetContainer(dataService, settings, userCulture, userRoleIds,
                                  driverContainerActionProcess.ContainerNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == containerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ContainerNumber: "
                                + driverContainerActionProcess.ContainerNumber));
                return false;
            }

            //No special processing is needed for the Containers with the QtyInID Flag set to Y.
            
            //Do not alter these values
            //ContainerType,ContainerSize,ContainerTerminalId,ContainerRegionId

            //Based on the action type, container may or may not be on the PowerId.
            if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Load)
            {
                containerMaster.ContainerPowerId = driverContainerActionProcess.PowerId;
            }
            else
            {
                containerMaster.ContainerPowerId = null;
            }

            DateTime? prevLastActionDateTime = containerMaster.ContainerLastActionDateTime;
            containerMaster.ContainerLastActionDateTime = driverContainerActionProcess.ActionDateTime;
            containerMaster.ContainerContents = driverContainerActionProcess.ContainerContents;

            //The container status should remain the same. Still at yard.
            //containerMaster.ContainerStatus = ContainerStatusConstants.Yard;
            //The previous trip number should remain the same.
            // containerMaster.ContainerPrevTripNumber;
            //The customer info remains the same.
            //containerMaster.ContainerCustHostCode;
            //containerMaster.ContainerCustType;

            //Container is not on a trip yet.
            containerMaster.ContainerCurrentTripNumber = null;
            containerMaster.ContainerCurrentTripSegNumber = null;
            containerMaster.ContainerCurrentTripSegType = null;
            containerMaster.ContainerCommodityCode = null;
            containerMaster.ContainerCommodityDesc = null;
            containerMaster.ContainerLocation = null;
            containerMaster.ContainerLevel = null;

            //Do not change when container is loaded or dropped at the yard. Does not apply here.. 
            //containerMaster.ContainerPendingMoveDateTime;
            //containerMaster.LocationWarningFlag;

            //If container is being loaded, it is on the move, so remove its current lat/lon
            if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Load)
            {
                containerMaster.ContainerLatitude = null;
                containerMaster.ContainerLongitude = null;
            }
            else
            {
                containerMaster.ContainerLatitude = driverContainerActionProcess.Latitude;
                containerMaster.ContainerLongitude = driverContainerActionProcess.Longitude;
            }

            //Do the update
            changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
            log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - Container Action {1}.",
                            driverContainerActionProcess.ContainerNumber, driverContainerActionProcess.ActionType);
            if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
            {
                var s = string.Format("Could not update ContainerMaster for ContainerNumber:{0} Action {1}",
                        driverContainerActionProcess.ContainerNumber, driverContainerActionProcess.ActionType);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Add record to Container History. 
            if (!Common.InsertContainerHistory(dataService, settings, containerMaster, null, prevLastActionDateTime,
                ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("InsertContainerHistory failed: {0} during container action request: {1}", fault.Message,
                                    driverContainerActionProcess);
                return false;
            }

            return true;

        }//end of public bool ContainerLoadDrop

        /// <summary>
        /// Processing for Container Done actions:D=Done,E=Exception, R=Review
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverContainerActionProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="containerHistoryInsertCount"></param>
        /// <returns></returns>
        public bool ContainerDone(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
           DriverContainerActionProcess driverContainerActionProcess, EmployeeMaster employeeMaster,
           int containerHistoryInsertCount)
        {
            DataServiceFault fault = null;

            ////////////////////////////////////////////////
            // Get the Trip record
            var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                             driverContainerActionProcess.TripNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == currentTrip)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                + driverContainerActionProcess.TripNumber));
                return false;
            }

            ////////////////////////////////////////////////
            //Check if trip is complete
            if (Common.IsTripComplete(currentTrip))
            {
                log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Container done processing ends.",
                                driverContainerActionProcess.TripNumber);
                return false;
            }

            ////////////////////////////////////////////////
            //Get a list of all segments for the trip
            var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                              driverContainerActionProcess.TripNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            ////////////////////////////////////////////////
            // Get the current TripSegment record
            var currentTripSegment = (from item in tripSegList
                                      where item.TripSegNumber == driverContainerActionProcess.TripSegNumber
                                      select item).FirstOrDefault();
            if (null == currentTripSegment)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                    driverContainerActionProcess.TripNumber + "-" + driverContainerActionProcess.TripSegNumber));
                return false;
            }
            if (currentTripSegment.TripSegType == BasicTripTypeConstants.PickupFull ||
               currentTripSegment.TripSegType == BasicTripTypeConstants.Load)
            { 
                ////////////////////////////////////////////////////////
                // Lookup Preference: DEFUseContainerLevel
                string prefUseContainerLevel = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                               employeeMaster.TerminalId, PrefDriverConstants.DEFUseContainerLevel, out fault);
                if (fault != null)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                if (prefUseContainerLevel == Constants.Yes)
                {
                    if(driverContainerActionProcess.ContainerLevel == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Container Level is required for: " +
                            driverContainerActionProcess.ContainerNumber));
                        return false;
                    }
                }
            }
            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
            {
                if (driverContainerActionProcess.SetInYardFlag == null)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("SetInYardFlag is required for: " +
                        driverContainerActionProcess.ContainerNumber));
                    return false;
                }
                if (driverContainerActionProcess.SetInYardFlag != Constants.Yes &&
                    driverContainerActionProcess.SetInYardFlag != Constants.No)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid SetInYardFlag for: " +
                        driverContainerActionProcess.ContainerNumber));
                    return false;
                }
            }

            ////////////////////////////////////////////////
            //Get a list of all containers for the segment
            var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                    driverContainerActionProcess.TripNumber, driverContainerActionProcess.TripSegNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            ////////////////////////////////////////////////
            // Get the Customer record for the destination cust host code
            var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                     currentTripSegment.TripSegDestCustHostCode, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == destCustomerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
                                + currentTripSegment.TripSegDestCustHostCode));
                return false;
            }
            ////////////////////////////////////////////////
            //Define the TripSegmentContainer for use later
            TripSegmentContainer tripSegmentContainer = new TripSegmentContainer();
            tripSegmentContainer = null;

            ////////////////////////////////////////////////
            //Update Container Information
            var containerMaster = Common.GetContainer(dataService, settings, userCulture, userRoleIds,
                                  driverContainerActionProcess.ContainerNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == containerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ContainerNumber: "
                                + driverContainerActionProcess.ContainerNumber));
                return false;
            }
            //Check the QtyInID Flag to see if we have do process the container differently
            if (containerMaster.ContainerQtyInIDFlag == Constants.Yes)
            {
                //ContainerNumber actually containe the quantity of containers and not a real container number
                //In this case we would want to update the ContainerQuantity and ContainerQuantityHIstory tables
                //instead of the ContainerMaster and ContainerHistory tables
                //But only for drops and pickups....
                var types = new List<string> {BasicTripTypeConstants.DropFull,
                                                  BasicTripTypeConstants.DropEmpty,
                                                  BasicTripTypeConstants.PickupFull,
                                                  BasicTripTypeConstants.DropEmpty};
                if (types.Contains(currentTripSegment.TripSegType))
                {
                    //TODO: Call a separate routine to process quantity in container number
                }
            }
            else
            {
                //Do not alter these values
                //ContainerType,ContainerSize,ContainerTerminalId,ContainerRegionId

                //Based on the segment type, container may or may not still be on the PowerId.
                if (Common.IsContainerOnPowerId(currentTripSegment.TripSegType,
                    driverContainerActionProcess.SetInYardFlag, driverContainerActionProcess.ActionType))
                {
                    containerMaster.ContainerPowerId = driverContainerActionProcess.PowerId;
                }
                else
                {
                    containerMaster.ContainerPowerId = null;
                }

                //Set the status based on the destination customer type
                if (currentTripSegment.TripSegDestCustType == CustomerTypeConstants.Yard)
                {
                    containerMaster.ContainerStatus = ContainerStatusConstants.Yard;
                }
                else
                {
                    containerMaster.ContainerStatus = ContainerStatusConstants.CustomerSite;
                    containerMaster.ContainerPrevCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                }
                containerMaster.ContainerCurrentTripNumber = driverContainerActionProcess.TripNumber;
                containerMaster.ContainerPrevTripNumber = driverContainerActionProcess.TripNumber;
                containerMaster.ContainerCurrentTripSegNumber = driverContainerActionProcess.TripSegNumber;
                containerMaster.ContainerCurrentTripSegType = currentTripSegment.TripSegType;
                containerMaster.ContainerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                containerMaster.ContainerCustType = currentTripSegment.TripSegDestCustType;

                DateTime? prevLastActionDateTime = containerMaster.ContainerLastActionDateTime;
                containerMaster.ContainerLastActionDateTime = driverContainerActionProcess.ActionDateTime;

                //Do not change when container is processed. 
                //Although it seems like we should, if segment type is PF,PE,LD,UL,RS...
                //and action type for this container was not an exception
                //but this is not how it is done in the current ScrapRunner.
                //containerMaster.ContainerPendingMoveDateTime;

                //Only update these if there are values present.
                if (driverContainerActionProcess.CommodityCode != null && driverContainerActionProcess.CommodityDesc != null)
                {
                    containerMaster.ContainerCommodityCode = driverContainerActionProcess.CommodityCode;
                    containerMaster.ContainerCommodityDesc = driverContainerActionProcess.CommodityDesc;
                }
                if (driverContainerActionProcess.ContainerLocation != null)
                {
                    containerMaster.ContainerLocation = driverContainerActionProcess.ContainerLocation;
                }

                containerMaster.ContainerContents = driverContainerActionProcess.ContainerContents;
                containerMaster.ContainerLevel = driverContainerActionProcess.ContainerLevel;
                containerMaster.ContainerLatitude = driverContainerActionProcess.Latitude;
                containerMaster.ContainerLongitude = driverContainerActionProcess.Longitude;

                //Set this to show the container current yard location.
                containerMaster.ContainerCurrentTerminalId = destCustomerMaster.ServingTerminalId;

                //The Inbound terminal is only set when the driver goes enroute on a RT segment.
                //It is removed when he arrives on a RT segment.
                //containerMaster.ContainerInboundTerminalId 

                /////////////////////////////////////////////////
                //Set the location warning flag
                //ToDo: The location warning flag needs to be tested.
                containerMaster.LocationWarningFlag = null;
                var types = new List<string> {BasicTripTypeConstants.DropFull,
                                              BasicTripTypeConstants.DropEmpty,
                                              BasicTripTypeConstants.Respot};

                if (types.Contains(currentTripSegment.TripSegType))
                {
                    //We need to look in the container history to find the previous trip and the last 
                    //status of the container at the end of that trip.
                    var containerLastTrip = Common.GetContainerHistoryLastTrip(dataService, settings, userCulture, userRoleIds,
                        driverContainerActionProcess.ContainerNumber, driverContainerActionProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        return false;
                    }
                    var statuses = new List<string> {ContainerStatusConstants.CustomerSite,
                                                     ContainerStatusConstants.Contractor,
                                                     ContainerStatusConstants.SpecialProject};
                    if (statuses.Contains(containerLastTrip?.ContainerStatus))
                    {
                        containerMaster.LocationWarningFlag = Constants.Yes;
                    }
                    else
                    {
                        containerMaster.LocationWarningFlag = Constants.No;
                    }
                }

                //Do the update
                changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - Container Done.",
                                driverContainerActionProcess.ContainerNumber);
                if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                {
                    var s = string.Format("Could not update ContainerMaster for ContainerNumber:{0}.",
                                driverContainerActionProcess.ContainerNumber);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }

                ////////////////////////////////////////////////
                //Add record to Container History. 
                if (!Common.InsertContainerHistory(dataService, settings, containerMaster, destCustomerMaster, prevLastActionDateTime,
                    ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    log.ErrorFormat("InsertContainerHistory failed: {0} during container done request: {1}", fault.Message,
                                     driverContainerActionProcess);
                    return false;
                }
            }

            ////////////////////////////////////////////////
            //Update TripSegmentContainer table.
            if (null != tripSegContainerList && tripSegContainerList.Count() > 0)
            {
                //First, try to find a container in the list that matches the container number on the power unit.
                //Allow for the use of the same container number multiple times on a segment.
                tripSegmentContainer = (from item in tripSegContainerList
                                        where item.TripSegContainerNumber == containerMaster.ContainerNumber
                                        && item.TripSegContainerComplete != Constants.Yes
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
                                              driverContainerActionProcess.TripNumber, driverContainerActionProcess.TripSegNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //Determine the next sequence number
                if (null == tripSegmentContainerMax)
                {
                    tripSegmentContainer.TripSegContainerSeqNumber = 0;
                }
                else
                {
                    tripSegmentContainer.TripSegContainerSeqNumber = ++tripSegmentContainerMax.TripSegContainerSeqNumber;
                }
                tripSegmentContainer.TripNumber = driverContainerActionProcess.TripNumber;
                tripSegmentContainer.TripSegNumber = driverContainerActionProcess.TripSegNumber;
            }
            //Container number from the mobile device is required.
            tripSegmentContainer.TripSegContainerNumber = driverContainerActionProcess.ContainerNumber;

            //Action Date/Time from the mobile device is required.
            tripSegmentContainer.TripSegContainerActionDateTime = driverContainerActionProcess.ActionDateTime;

            //Use the type and size from the container master, not the mobile device.
            tripSegmentContainer.TripSegContainerType = containerMaster.ContainerType;
            tripSegmentContainer.TripSegContainerSize = containerMaster.ContainerSize;

            //Comodity code and description are optional, from driver. May have been filled in by dispatch. 
            if (driverContainerActionProcess.CommodityCode != null && driverContainerActionProcess.CommodityDesc != null)
            {
                tripSegmentContainer.TripSegContainerCommodityCode = driverContainerActionProcess.CommodityCode;
                tripSegmentContainer.TripSegContainerCommodityDesc = driverContainerActionProcess.CommodityDesc;
            }

            //Location is optional, from driver. May have been filled in by dispatch. 
            if (driverContainerActionProcess.ContainerLocation != null)
            {
                tripSegmentContainer.TripSegContainerLocation = driverContainerActionProcess.ContainerLocation;
            }

            //Level is optional, as determined by a preference.
            if (driverContainerActionProcess.ContainerLevel != null)
            {
                tripSegmentContainer.TripSegContainerLevel = driverContainerActionProcess.ContainerLevel;
            }

            //If latitude/longitude not provided from driver, use the lat/lon for the destination customer.
            if (driverContainerActionProcess.Latitude == null || driverContainerActionProcess.Longitude == null)
            {
                tripSegmentContainer.TripSegContainerLatitude = destCustomerMaster.CustLatitude;
                tripSegmentContainer.TripSegContainerLongitude = destCustomerMaster.CustLongitude;
            }
            else
            {
                tripSegmentContainer.TripSegContainerLatitude = driverContainerActionProcess.Latitude;
                tripSegmentContainer.TripSegContainerLongitude = driverContainerActionProcess.Longitude;
            }
            //This indicates whether the container number was scanned or manually (hand-typed) entered.
            tripSegmentContainer.TripSegContainerEntryMethod = driverContainerActionProcess.MethodOfEntry;

            // The Review Flag and Reason contains either R=Review and the ReviewReason
            // or E=Exception and the Exception Reason (handled in a separate routine)
            if (driverContainerActionProcess.ActionType == ContainerActionTypeConstants.Review)
            {
                tripSegmentContainer.TripSegContainerReviewFlag = driverContainerActionProcess.ActionType;
                tripSegmentContainer.TripSegContainerReviewReason = driverContainerActionProcess.ActionCode
                    + "-" + driverContainerActionProcess.ActionDesc;
            }
            else
            {
                tripSegmentContainer.TripSegContainerReviewFlag = Constants.No;
            }

            // The purpose of this is field to indicate that processing of this container is complete.
            // Eventually we will remove all trip segment container records that are not complete.
            tripSegmentContainer.TripSegContainerComplete = Constants.Yes;

            //For return to yard processing only...
            if (currentTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard ||
                currentTripSegment.TripSegType == BasicTripTypeConstants.Scale)
            {
                //Scale reference number can be captured at gross action or at both gross and tare actions.
                if (driverContainerActionProcess.ScaleReferenceNumber != null)
                {
                    tripSegmentContainer.TripScaleReferenceNumber = driverContainerActionProcess.ScaleReferenceNumber;
                }
                //The ContainerLoaded flag is based on ContainerContents
                if (driverContainerActionProcess.ContainerContents == ContainerContentsConstants.Loaded)
                {
                    tripSegmentContainer.TripSegContainerLoaded = Constants.Yes;
                }
                else
                {
                    tripSegmentContainer.TripSegContainerLoaded = Constants.No;
                }

                //The TripSegContainerOnTruck is based on the SetInYardFlag flag
                if (driverContainerActionProcess.SetInYardFlag == Constants.Yes)
                {
                    //Set down
                    tripSegmentContainer.TripSegContainerOnTruck = Constants.No;
                }
                else
                {
                    //Left on Truck
                    tripSegmentContainer.TripSegContainerOnTruck = Constants.Yes;
                }

                //Gross, 2nd gross (optional) and tare times and weights (optional)
                tripSegmentContainer.WeightGrossDateTime = driverContainerActionProcess.Gross1ActionDateTime;
                tripSegmentContainer.TripSegContainerWeightGross = driverContainerActionProcess.Gross1Weight;
                tripSegmentContainer.WeightGross2ndDateTime = driverContainerActionProcess.Gross2ActionDateTime;
                tripSegmentContainer.TripSegContainerWeightGross2nd = driverContainerActionProcess.Gross2Weight;
                tripSegmentContainer.WeightTareDateTime = driverContainerActionProcess.TareActionDateTime;
                tripSegmentContainer.TripSegContainerWeightTare = driverContainerActionProcess.TareWeight;
            }

            //Do the update
            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
            log.DebugFormat("SRTEST:Saving TripSegmentContainer for ContainerNumber:{0} - Container Done.",
                            tripSegmentContainer.TripSegContainerNumber);
            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
            {
                var s = string.Format("Could not update TripSegmentContainer for ContainerNumber:{0}.",
                            tripSegmentContainer.TripSegContainerNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            return true;
        }//end of public bool ContainerDone
        /// <summary>
        /// Container Exception processing
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverContainerActionProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <returns></returns>
        public bool ContainerException(IDataService dataService, ProcessChangeSetSettings settings,
          ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
          DriverContainerActionProcess driverContainerActionProcess, EmployeeMaster employeeMaster)
        {
            DataServiceFault fault = null;

            ////////////////////////////////////////////////
            // Get the Trip record
            var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                             driverContainerActionProcess.TripNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == currentTrip)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                + driverContainerActionProcess.TripNumber));
                return false;
            }

            ////////////////////////////////////////////////
            //Check if trip is complete
            if (Common.IsTripComplete(currentTrip))
            {
                log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Container exception processing ends.",
                                driverContainerActionProcess.TripNumber);
                return false;
            }

            ////////////////////////////////////////////////
            //Define the TripSegmentContainer for use later
            TripSegmentContainer tripSegmentContainer = new TripSegmentContainer();
            tripSegmentContainer = null;

            ////////////////////////////////////////////////
            //Get a list of all containers for the segment
            var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                       driverContainerActionProcess.TripNumber, driverContainerActionProcess.TripSegNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null != tripSegContainerList && tripSegContainerList.Count() > 0)
            {
                if (driverContainerActionProcess.ContainerNumber != null)
                {
                    //First, try to find a container in the list that matches the container number on the power unit.
                    //Allow for the use of the same container number multiple times on a segment.
                    tripSegmentContainer = (from item in tripSegContainerList
                                            where item.TripSegContainerNumber == driverContainerActionProcess.ContainerNumber
                                            && item.TripSegContainerComplete != Constants.Yes
                                            select item).FirstOrDefault();
                }
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
                                              driverContainerActionProcess.TripNumber, driverContainerActionProcess.TripSegNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //Determine the next sequence number
                if (null == tripSegmentContainerMax)
                {
                    tripSegmentContainer.TripSegContainerSeqNumber = 0;
                }
                else
                {
                    tripSegmentContainer.TripSegContainerSeqNumber = ++tripSegmentContainerMax.TripSegContainerSeqNumber;
                }
                tripSegmentContainer.TripNumber = driverContainerActionProcess.TripNumber;
                tripSegmentContainer.TripSegNumber = driverContainerActionProcess.TripSegNumber;
            }
            //Action Date/Time from the mobile device is required.
            tripSegmentContainer.TripSegContainerActionDateTime = driverContainerActionProcess.ActionDateTime;

            //Save the exception information
            tripSegmentContainer.TripSegContainerReviewFlag = driverContainerActionProcess.ActionType;
            tripSegmentContainer.TripSegContainerReviewReason = driverContainerActionProcess.ActionCode
                + "-" + driverContainerActionProcess.ActionDesc;

            // The purpose of this is field to indicate that processing of this container is complete.
            // Eventually we will remove all trip segment container records that are not complete.
            tripSegmentContainer.TripSegContainerComplete = Constants.Yes;

            //Do the update
            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
            log.DebugFormat("SRTEST:Saving TripSegmentContainer for ContainerNumber:{0} - Container Exception.",
                            tripSegmentContainer.TripSegContainerNumber);
            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
            {
                var s = string.Format("Could not update TripSegmentContainer for ContainerNumber:{0}.",
                            tripSegmentContainer.TripSegContainerNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            return true;
        }//end of public bool ContainerException

        /// <summary>
        /// Container Add Processing
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverContainerActionProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="containerHistoryInsertCount"></param>
        /// <returns></returns>
        public bool ContainerAdd(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
           DriverContainerActionProcess driverContainerActionProcess, EmployeeMaster employeeMaster)
        {
            DataServiceFault fault = null;

            ////////////////////////////////////////////////
            // Get the Trip record
            var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                             driverContainerActionProcess.TripNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == currentTrip)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                + driverContainerActionProcess.TripNumber));
                return false;
            }

            ////////////////////////////////////////////////
            //Check if trip is complete
            if (Common.IsTripComplete(currentTrip))
            {
                log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Container add processing ends.",
                                driverContainerActionProcess.TripNumber);
                return false;
            }

            ////////////////////////////////////////////////
            //Get a list of all segments for the trip
            var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                              driverContainerActionProcess.TripNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            ////////////////////////////////////////////////
            //Define the TripSegmentContainer for use later
            TripSegment currentTripSegment = new TripSegment();
            //Define the TripSegmentContainer for use later
            TripSegmentContainer tripSegmentContainer = new TripSegmentContainer();

            ////////////////////////////////////////////////
            // Get the current TripSegment record
            currentTripSegment = (from item in tripSegList
                                      where item.TripSegNumber == driverContainerActionProcess.TripSegNumber
                                      select item).FirstOrDefault();

            ////////////////////////////////////////////////
            //Get a list of all containers for the segment
            var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                    driverContainerActionProcess.TripNumber, driverContainerActionProcess.TripSegNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            ////////////////////////////////////////////////
            //Get Container Information
            var containerMaster = Common.GetContainer(dataService, settings, userCulture, userRoleIds,
                                  driverContainerActionProcess.ContainerNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == containerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ContainerNumber: "
                                + driverContainerActionProcess.ContainerNumber));
                return false;
            }
            ////////////////////////////////////////////////
            //Update TripSegmentContainer table.
            if (null != tripSegContainerList && tripSegContainerList.Count() > 0)
            {
                //First, try to find a container in the list that matches the container number on the power unit.
                //Allow for the use of the same container number multiple times on a segment.
                tripSegmentContainer = (from item in tripSegContainerList
                                        where item.TripSegContainerNumber == containerMaster.ContainerNumber
                                        && item.TripSegContainerComplete != Constants.Yes
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
            if (null == tripSegmentContainer || null == tripSegmentContainer.TripNumber)
            {
                tripSegmentContainer = new TripSegmentContainer();

                //Look up the last trip segment container for this trip and segment
                TripSegmentContainer tripSegmentContainerMax = new TripSegmentContainer();

                //Use this to calculate the next sequence number.
                tripSegmentContainerMax = Common.GetTripSegmentContainerLast(dataService, settings, userCulture, userRoleIds,
                                              driverContainerActionProcess.TripNumber, driverContainerActionProcess.TripSegNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //Determine the next sequence number
                tripSegmentContainer.TripSegContainerSeqNumber = 0;
                if (null != tripSegmentContainerMax)
                {
                    tripSegmentContainer.TripSegContainerSeqNumber = ++tripSegmentContainerMax.TripSegContainerSeqNumber;
                }
                tripSegmentContainer.TripNumber = driverContainerActionProcess.TripNumber;
                tripSegmentContainer.TripSegNumber = driverContainerActionProcess.TripSegNumber;
            }
            //Container number from the mobile device is required.
            tripSegmentContainer.TripSegContainerNumber = driverContainerActionProcess.ContainerNumber;

            //Action Date/Time from the mobile device is required.
            tripSegmentContainer.TripSegContainerActionDateTime = driverContainerActionProcess.ActionDateTime;

            //Use the type and size from the container master, not the mobile device.
            tripSegmentContainer.TripSegContainerType = containerMaster.ContainerType;
            tripSegmentContainer.TripSegContainerSize = containerMaster.ContainerSize;

            //Comodity code and description are optional, from driver. May have been filled in by dispatch. 
            if (driverContainerActionProcess.CommodityCode != null && driverContainerActionProcess.CommodityDesc != null)
            {
                tripSegmentContainer.TripSegContainerCommodityCode = driverContainerActionProcess.CommodityCode;
                tripSegmentContainer.TripSegContainerCommodityDesc = driverContainerActionProcess.CommodityDesc;
            }

            //Location is optional, from driver. May have been filled in by dispatch. 
            if (driverContainerActionProcess.ContainerLocation != null)
            {
                tripSegmentContainer.TripSegContainerLocation = driverContainerActionProcess.ContainerLocation;
            }

            //Level is optional, as determined by a preference.
            if (driverContainerActionProcess.ContainerLevel != null)
            {
                tripSegmentContainer.TripSegContainerLevel = driverContainerActionProcess.ContainerLevel;
            }

            //This indicates whether the container number was scanned or manually (hand-typed) entered.
            tripSegmentContainer.TripSegContainerEntryMethod = driverContainerActionProcess.MethodOfEntry;


            //The ContainerLoaded flag is based on ContainerContents
            if (driverContainerActionProcess.ContainerContents == ContainerContentsConstants.Loaded)
            {
                tripSegmentContainer.TripSegContainerLoaded = Constants.Yes;
            }
            else
            {
                tripSegmentContainer.TripSegContainerLoaded = Constants.No;
            }


            tripSegContainerList.Add(tripSegmentContainer);

            //Do the update
            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
            log.DebugFormat("SRTEST:Saving TripSegmentContainer for ContainerNumber:{0} - Container Add.",
                            tripSegmentContainer.TripSegContainerNumber);
            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
            {
                var s = string.Format("Could not add TripSegmentContainer for ContainerNumber:{0}.",
                            tripSegmentContainer.TripSegContainerNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            if (currentTripSegment != null)
            {
                ////////////////////////////////////////////////
                //Update TripSegment Primary Container Information from first TripSegmentContainer information. 
                if (tripSegContainerList.Count() > 0)
                {
                    var firstTripSegmentContainer = tripSegContainerList.First();

                    if (null != firstTripSegmentContainer)
                    {
                        //Only if there is a container number
                        if (null != firstTripSegmentContainer.TripSegContainerNumber)
                        {
                            currentTripSegment.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                            currentTripSegment.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                            currentTripSegment.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                            currentTripSegment.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            currentTripSegment.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityDesc;
                            currentTripSegment.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;
                        }
                    }
                }

                ////////////////////////////////////////////////
                //Do the TripSegment table update
                changeSetResult = Common.UpdateTripSegment(dataService, settings, currentTripSegment);
                log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Container Add.",
                                currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
                {
                    var s = string.Format("Container Add:Could not update TripSegment for Trip:{0}-{1}.",
                        currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }

                //Update Trip Table
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
                log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Container Add.",
                                currentTrip.TripNumber);
                if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
                {
                    var s = string.Format("Container Add::Could not update Trip for Trip:{0}.",
                        currentTrip.TripNumber);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
            }
            return true;
        }//end of public bool ContainerAdd

    }
}
