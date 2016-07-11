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

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver delay.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverDelayProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverDelayProcess")]
    public class DriverDelayProcessRecordType : ChangeableRecordType
        <DriverDelayProcess, string, DriverDelayProcessValidator, DriverDelayProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverDelayProcess, DriverDelayProcess>();
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
                        ChangeSet<string, DriverDelayProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver fuel entry processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverDelayProcess> changeSet, ProcessChangeSetSettings settings)
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
                    int driverHistoryInsertCount = 0;
                    int driverDelayInsertCount = 0;

                    var driverDelayProcess = (DriverDelayProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverDelayProcess backfillDriverDelayProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverDelayProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverDelayProcess, driverDelayProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process delay for DriverId: "
                            + driverDelayProcess.EmployeeId + "Delay: " + driverDelayProcess.DelayCode));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverDelayProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverDelay:Invalid DriverId: "
                                        + driverDelayProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Power record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                      driverDelayProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverDelay:Invalid PowerId: "
                                        + driverDelayProcess.PowerId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Only associated delay with trip if the trip is in progress
                    var currentTrip = new Trip();
                    var currentTripSegment = new TripSegment();

                    if (driverDelayProcess.TripNumber != null &&
                        driverDelayProcess.TripSegNumber != null)
                    {
                        ////////////////////////////////////////////////
                        // Get the Trip record
                        currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                      driverDelayProcess.TripNumber, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == currentTrip)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverDelay:Invalid TripNumber: "
                                            + driverDelayProcess.TripNumber));
                            break;
                        }
                        if (currentTrip.TripInProgressFlag == Constants.No ||
                            currentTrip.TripInProgressFlag == null)
                        {
                            driverDelayProcess.TripNumber = null;
                            driverDelayProcess.TripSegNumber = null;
                        }
                        if (driverDelayProcess.TripNumber != null &&
                            driverDelayProcess.TripSegNumber != null)
                        {
                            ////////////////////////////////////////////////
                            // Get the Trip segment record
                            currentTripSegment = Common.GetTripSegment(dataService, settings, userCulture, userRoleIds,
                                   driverDelayProcess.TripNumber, driverDelayProcess.TripSegNumber, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            if (null == currentTripSegment)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                   + driverDelayProcess.TripNumber + "-" + driverDelayProcess.TripSegNumber));
                                break;
                            }
                        }
                    }
                    if (driverDelayProcess.ActionType == DelayActionTypeConstants.Delay)
                    {
                        //Processing a delay
                        if (!ProcessDelay(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverDelayProcess, employeeMaster, currentTrip, currentTripSegment,
                                    driverDelayInsertCount, driverHistoryInsertCount))
                        {
                            break;
                        }

                    }
                    else
                    {
                        //Processing a back on duty
                        if (!ProcessBackOnDuty(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverDelayProcess, employeeMaster, currentTrip, currentTripSegment,
                                    driverDelayInsertCount, driverHistoryInsertCount))
                        {
                            break;
                        }
                    }
                }//end of foreach...
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
                log.Debug("SRTEST:Transaction Rollback - Delay/BackOnDuty");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Delay/BackOnDuty");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }

        private bool ProcessDelay(IDataService dataService, ProcessChangeSetSettings settings,
                ChangeSetResult<string> changeSetResult, string msgKey, long[] userRoleIds, string userCulture,
                DriverDelayProcess driverDelayProcess, EmployeeMaster employeeMaster, Trip currentTrip, TripSegment currentTripSegment, 
                int driverDelayInsertCount, int driverHistoryInsertCount)
        {
            DataServiceFault fault;

            ////////////////////////////////////////////////
            //Add a new DriverDelay Record
            var driverDelay = new DriverDelay();

            //Set the driver id and name
            driverDelay.DriverId = driverDelayProcess.EmployeeId;
            driverDelay.DriverName = Common.GetEmployeeName(employeeMaster);

            //If Trip Number is empty then put the driver id in the field
            if (driverDelayProcess.TripNumber == null)
            {
                driverDelay.TripNumber = Constants.DriverPrefix + driverDelayProcess.EmployeeId;
            }
            else
            {
                driverDelay.TripNumber = driverDelayProcess.TripNumber;
            }
            driverDelay.TripSegNumber = driverDelayProcess.TripSegNumber;

            //Set the terminal id
            driverDelay.TerminalId = employeeMaster.TerminalId;
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TerminalName in the TerminalMaster 
            var driverTerminalMaster = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          driverDelay.TerminalId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            driverDelay.TerminalName = driverTerminalMaster?.TerminalName;

            //Set the region id
            driverDelay.RegionId = employeeMaster.RegionId;
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the RegionName in the RegionMaster 
            var driverRegionMaster = Common.GetRegion(dataService, settings, userCulture, userRoleIds,
                                          driverDelay.RegionId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            driverDelay.RegionName = driverRegionMaster?.RegionName;

            //Set the start date/time for the delay
            driverDelay.DelayStartDateTime = driverDelayProcess.ActionDateTime;

            //Set the delay code and description
            driverDelay.DelayCode = driverDelayProcess.DelayCode;
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the Delay Description in the CodeTable DELAYCODES 
            var codeTableDelayCode = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.DelayCodes, driverDelay.DelayCode, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            driverDelay.DelayReason = codeTableDelayCode?.CodeDisp1;

            //Set the latitude and longitude
            driverDelay.DelayLatitude = driverDelayProcess.Latitude;
            driverDelay.DelayLongitude = driverDelayProcess.Longitude;

            //Do the insert
            //There was a problem with  the insert function not working properly with #driverid in the tripnumber field.
            //But this has been corrected with a BWF update. Pass false to the GetQuery function.
            //The insert function will calculate the DelaySeqNumber
            log.DebugFormat("SRTEST:Saving DriverDelay Record for DriverId:{0} DelayCode:{1}-Driver Delay.",
                            driverDelay.DriverId, driverDelay.DelayCode);
            if (!Common.InsertDriverDelay(dataService, settings, userRoleIds, userCulture, log,
                                    driverDelay, ++driverDelayInsertCount, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("InsertDriverDelay failed: {0} during driver delay request: {1}", fault.Message, driverDelayProcess);
                return false;
            }

            ////////////////////////////////////////////////
            //Update DriverStatus
            var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                               driverDelayProcess.EmployeeId, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //ToDo: What do we do if there is no driver status record, driver not logged in.
            //Should we create a new driver status record?
            if (driverStatus != null)
            {

                //Set the field values for driver status
                //Save the original status in the PrevDriverStatus, so that it can be used after the back on duty is processed.
                //As a precaution, do not store delays or back on duties in previous status
                if (driverStatus.Status != DriverStatusSRConstants.Delay ||
                    driverStatus.Status != DriverStatusSRConstants.BackOnDuty)
                {
                    driverStatus.PrevDriverStatus = driverStatus.Status;
                }
                driverStatus.Status = DriverStatusSRConstants.Delay;

                //For delays store the delay code
                driverStatus.DelayCode = driverDelayProcess.DelayCode;

                if (driverDelayProcess.TripNumber != null)
                {
                    driverStatus.TripNumber = driverDelayProcess.TripNumber;
                    driverStatus.TripSegNumber = driverDelayProcess.TripSegNumber;
                    driverStatus.TripSegType = currentTripSegment.TripSegType;
                    driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                    driverStatus.TripStatus = currentTrip.TripStatus;
                    driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                }
                else
                {
                    driverStatus.TripNumber = null;
                    driverStatus.TripSegNumber = null;
                    driverStatus.TripSegType = null;
                    driverStatus.TripAssignStatus = null;
                    driverStatus.TripStatus = null;
                    driverStatus.TripSegStatus = null;
                }
                driverStatus.PowerId = driverDelayProcess.PowerId;
                driverStatus.MDTId = driverDelayProcess.Mdtid;
                driverStatus.ActionDateTime = driverDelayProcess.ActionDateTime;

                if (null == driverDelayProcess.Latitude || null == driverDelayProcess.Longitude)
                {
                    driverStatus.GPSXmitFlag = Constants.No;
                }
                else
                {
                    driverStatus.GPSXmitFlag = Constants.Yes;
                }

                //Do the DriverStatus update
                changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Driver Delay.",
                                driverStatus.EmployeeId);
                if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                {
                    var s = string.Format("Could not update DriverStatus for DriverId:{0}.",
                        driverStatus.EmployeeId);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }

                ////////////////////////////////////////////////
                //Add record to the DriverHistory table. 
                if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                    ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    log.ErrorFormat("InsertDriverHistory failed: {0} during Delay request: {1}", fault.Message, driverDelayProcess);
                    return false;
                }
            }

            ////////////////////////////////////////////////
            //Add entry to Event Log – Delay. 
            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(EventCommentConstants.ReceivedDriverDelay);
            sbComment.Append(" HH:");
            sbComment.Append(driverDelayProcess.ActionDateTime);
            if (driverDelayProcess.TripNumber != null)
            {
                sbComment.Append(" Trip:");
                sbComment.Append(driverDelayProcess.TripNumber);
                sbComment.Append("-");
                sbComment.Append(driverDelayProcess.TripSegNumber);
            }
            sbComment.Append(" Drv:");
            sbComment.Append(driverDelayProcess.EmployeeId);
            sbComment.Append(" Pwr:");
            sbComment.Append(driverDelayProcess.PowerId);
            sbComment.Append(" Delay:");
            sbComment.Append(driverDelayProcess.DelayCode);
            string comment = sbComment.ToString().Trim();
            var eventLog = new EventLog()
            {
                EventDateTime = driverDelayProcess.ActionDateTime,
                EventSeqNo = 0,
                EventTerminalId = employeeMaster.TerminalId,
                EventRegionId = employeeMaster.RegionId,
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverDelayProcess.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - Driver Delay");
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                        driverDelayProcess.EmployeeId, EventCommentConstants.ReceivedDriverDelay);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            return true;
        }

        private bool ProcessBackOnDuty(IDataService dataService, ProcessChangeSetSettings settings,
                 ChangeSetResult<string> changeSetResult, string msgKey, long[] userRoleIds, string userCulture,
                 DriverDelayProcess driverDelayProcess, EmployeeMaster employeeMaster, Trip currentTrip, TripSegment currentTripSegment,
                 int driverDelayInsertCount, int driverHistoryInsertCount)
        {
            DataServiceFault fault;

            ////////////////////////////////////////////////
            //Find the last open ended DriverDelay Record
            var driverDelayList = Common.GetDriverDelaysOpenEnded(dataService, settings, userCulture, userRoleIds,
                                          driverDelayProcess.EmployeeId, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (driverDelayList == null)
            {
                //ToDo: On a back on duty, if there is no delay started, should we start one and end it?
            }
            //List is in order of delay start date desc
            var lastDriverDelay = driverDelayList.FirstOrDefault();

            //Delete all but the last open ended delay. There should be none.
            foreach (var oldDriverDelay in driverDelayList)
            {
                if (oldDriverDelay != lastDriverDelay)
                {
                    //Do the delete. Deleting records with composite keys is now fixed.
                    changeSetResult = Common.DeleteDriverDelay(dataService, settings, oldDriverDelay);
                    log.DebugFormat("SRTEST:Deleting DriverDelay Record for DriverId:{0} Seq#:{1} Trip:{2} - BackOnDuty.",
                                    oldDriverDelay.DriverId, oldDriverDelay.DelaySeqNumber, oldDriverDelay.TripNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, oldDriverDelay, log))
                    {
                        var s = string.Format("DriverDelayProcess:Could not delete DriverDelay for DriverId:{0} Seq#:{1} Trip:{2}.",
                                oldDriverDelay.DriverId, oldDriverDelay.DelaySeqNumber, oldDriverDelay.TripNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                }
            }

            //Set the end date/time for the delay
            lastDriverDelay.DelayEndDateTime = driverDelayProcess.ActionDateTime;

            //Do the driverDelay update
            changeSetResult = Common.UpdateDriverDelay(dataService, settings, lastDriverDelay);
            log.DebugFormat("SRTEST:Saving DriverDelay Record for DriverId:{0} Seq#:{1} Trip:{2} - Driver BackOnDuty.",
                            lastDriverDelay.DriverId, lastDriverDelay.DelaySeqNumber,lastDriverDelay.TripNumber);
            if (Common.LogChangeSetFailure(changeSetResult, lastDriverDelay, log))
            {
                var s = string.Format("DriverDelayProcess(BackOnDuty):Could not update DriverDelay for DriverId:{0} Seq#:{1} Trip:{2}.",
                    lastDriverDelay.DriverId, lastDriverDelay.DelaySeqNumber, lastDriverDelay.TripNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Update DriverStatus
            var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                               driverDelayProcess.EmployeeId, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (driverStatus != null)
            {
                //Set the field values for driver status
                //Don't store the current status (Delay) in previous driver status
                //driverStatus.PrevDriverStatus = driverStatus.Status;

                driverStatus.Status = DriverStatusSRConstants.BackOnDuty;

                //For Back on duty remove the delay code
                driverStatus.DelayCode = null;

                if (driverDelayProcess.TripNumber != null)
                {
                    driverStatus.TripNumber = driverDelayProcess.TripNumber;
                    driverStatus.TripSegNumber = driverDelayProcess.TripSegNumber;
                    driverStatus.TripSegType = currentTripSegment.TripSegType;
                    driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                    driverStatus.TripStatus = currentTrip.TripStatus;
                    driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                }
                else
                {
                    driverStatus.TripNumber = null;
                    driverStatus.TripSegNumber = null;
                    driverStatus.TripSegType = null;
                    driverStatus.TripAssignStatus = null;
                    driverStatus.TripStatus = null;
                    driverStatus.TripSegStatus = null;
                }
                driverStatus.PowerId = driverDelayProcess.PowerId;
                driverStatus.MDTId = driverDelayProcess.Mdtid;
                driverStatus.ActionDateTime = driverDelayProcess.ActionDateTime;

                if (null == driverDelayProcess.Latitude || null == driverDelayProcess.Longitude)
                {
                    driverStatus.GPSXmitFlag = Constants.No;
                }
                else
                {
                    driverStatus.GPSXmitFlag = Constants.Yes;
                }

                ////////////////////////////////////////////////
                //Add record to the DriverHistory table. 
                if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                    ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    log.ErrorFormat("InsertDriverHistory failed: {0} during BackOnDuty request: {1}", fault.Message, driverDelayProcess);
                    return false;
                }

                //For Back on Duty set the driver status back to the previous status
                driverStatus.Status = driverStatus.PrevDriverStatus;

                //Now do the DriverStatus update
                changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Driver Delay.",
                                driverStatus.EmployeeId);
                if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                {
                    var s = string.Format("Could not update DriverStatus for DriverId:{0}.",
                        driverStatus.EmployeeId);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
            }

            ////////////////////////////////////////////////
            //Add entry to Event Log – BackOnDuty. 
            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(EventCommentConstants.ReceivedDriverBackOnDuty);
            sbComment.Append(" HH:");
            sbComment.Append(driverDelayProcess.ActionDateTime);
            if (driverDelayProcess.TripNumber != null)
            {
                sbComment.Append(" Trip:");
                sbComment.Append(driverDelayProcess.TripNumber);
                sbComment.Append("-");
                sbComment.Append(driverDelayProcess.TripSegNumber);
            }
            sbComment.Append(" Drv:");
            sbComment.Append(driverDelayProcess.EmployeeId);
            sbComment.Append(" Pwr:");
            sbComment.Append(driverDelayProcess.PowerId);
            sbComment.Append(" Delay:");
            sbComment.Append(driverDelayProcess.DelayCode);
            string comment = sbComment.ToString().Trim();
            var eventLog = new EventLog()
            {
                EventDateTime = driverDelayProcess.ActionDateTime,
                EventSeqNo = 0,
                EventTerminalId = employeeMaster.TerminalId,
                EventRegionId = employeeMaster.RegionId,
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverDelayProcess.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - Driver BackOnDuty");
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("DriverDelayProcess:Could not update EventLog for Driver {0} {1} BackOnDuty.",
                        driverDelayProcess.EmployeeId, EventCommentConstants.ReceivedDriverBackOnDuty);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            return true;
        }
    }
}
