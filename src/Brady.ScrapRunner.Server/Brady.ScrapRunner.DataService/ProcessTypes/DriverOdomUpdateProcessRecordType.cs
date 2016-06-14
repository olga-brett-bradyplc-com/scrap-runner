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
using Brady.ScrapRunner.Domain.Enums;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver odometer update.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverOdomUpdateProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverOdomUpdateProcess")]
    public class DriverOdomUpdateProcessRecordType : ChangeableRecordType
        <DriverOdomUpdateProcess, string, DriverOdomUpdateProcessValidator, DriverOdomUpdateProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverOdomUpdateProcess, DriverOdomUpdateProcess>();
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
                        ChangeSet<string, DriverOdomUpdateProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
        /// <summary>
        /// Perform the driver OdomUpdate processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverOdomUpdateProcess> changeSet, ProcessChangeSetSettings settings)
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
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    var driverOdomUpdateProcess = (DriverOdomUpdateProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverOdomUpdateProcess backfillDriverOdomUpdateProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverOdomUpdateProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverOdomUpdateProcess, driverOdomUpdateProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Unable to process OdomUpdate for DriverId: " + driverOdomUpdateProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverOdomUpdateProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid DriverId: "
                                        + driverOdomUpdateProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the DriverStatus record
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                      driverOdomUpdateProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == driverStatus)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid EmployeeId: "
                                        + driverOdomUpdateProcess.PowerId));
                        break;
                    }

                    /////////////////////////////////////////////////
                    //Split the processing into one of two functions:
                    //Load/drop action type
                    if (driverOdomUpdateProcess.PowerId != driverStatus.PowerId)
                    {
                        if (!ChangePowerUnit(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverOdomUpdateProcess, employeeMaster, driverStatus))
                        {
                            var s = string.Format("Could not change for PowerId:{0} Odom:{1}.",
                                    driverOdomUpdateProcess.PowerId, driverOdomUpdateProcess.Odometer);
                            break;
                        }
                    }
                    else
                    {
                        if (!CorrectOdometer(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverOdomUpdateProcess, employeeMaster, driverStatus))
                        {
                            var s = string.Format("Could not correct odometer for PowerId:{0} Odom:{1}.",
                                    driverOdomUpdateProcess.PowerId, driverOdomUpdateProcess.Odometer);
                            break;
                        }
                    }

 

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
                log.Debug("SRTEST:Transaction Rollback - OdomUpdate");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - OdomUpdate");
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
        /// Change Power Unit
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverOdomUpdateProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="driverStatus"></param>
        /// <returns></returns>
        public bool ChangePowerUnit(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
           DriverOdomUpdateProcess driverOdomUpdateProcess, EmployeeMaster employeeMaster, DriverStatus driverStatus)
        {
            DataServiceFault fault = null;
            int powerHistoryInsertCount = 0;
            int origPowerHistoryInsertCount = 0;

            ////////////////////////////////////////////////
            // Preferences:  Lookup the system preference "DEFAllowAnyPowerUnit".  
            // If preference prefAllowAnyPowerUnit is set to Y, then allow any power unit to be valid
            // Otherwise the power unit's region must match the driver's region. This is the norm.
            string prefAllowAnyPowerUnit = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                           Constants.SystemTerminalId, PrefSystemConstants.DEFAllowAnyPowerUnit, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            ////////////////////////////////////////////////
            // Get the PowerMaster record
            // PowerId must be a valid PowerId in the PowerMaster 
            var powerMaster = new PowerMaster();
            if (prefAllowAnyPowerUnit == Constants.Yes)
            {
                powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                          driverOdomUpdateProcess.PowerId, out fault);
            }
            else
            {
                powerMaster = Common.GetPowerUnitForRegion(dataService, settings, userCulture, userRoleIds,
                                          driverOdomUpdateProcess.PowerId, employeeMaster.RegionId, out fault);
            }
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == powerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Power ID " + driverOdomUpdateProcess.PowerId));
                return false;
            }
            //For testing
            log.Debug("SRTEST:GetPowerUnit or GetPowerUnitForRegion");
            log.DebugFormat("SRTEST:PowerId:{0} Driver:{1} Odom:{2} PowerStatus:{3} AllowAnyPowerUnit:{4} Region:{5}",
                             powerMaster.PowerId,
                             powerMaster.PowerDriverId,
                             powerMaster.PowerOdometer,
                             powerMaster.PowerStatus,
                             prefAllowAnyPowerUnit,
                             powerMaster.PowerRegionId);

            ////////////////////////////////////////////////
            // Check power unit status: Scheduled for the shop? PS_SHOP = "S"
            if (PowerStatusConstants.Shop == powerMaster.PowerStatus)
            {
                var s = string.Format("Do not use Power unit {0}.  It is scheduled for the shop. ",
                        driverOdomUpdateProcess.PowerId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            // Is unit in use by another driver? PS_INUSE = "I"
            if (powerMaster.PowerStatus == PowerStatusConstants.InUse &&
                powerMaster.PowerDriverId != employeeMaster.EmployeeId)
            {
                var s = string.Format("Power unit {0} in use by another driver.  Change power unit number or call Dispatch.",
                        driverOdomUpdateProcess.PowerId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            // If no odometer record for this unit, accept as sent by the handheld.  
            // If overrride flag = "Y", accept as sent by the handheld.  
            // Otherwise lookup the preference to determine the acceptable range of values for the tolerance check.
            // If the override flag is not set, and the odometer is within the range preference then accept the odometer 
            if (driverOdomUpdateProcess.OverrideFlag != Constants.Yes && powerMaster.PowerOdometer != null)
            {
                //Get the preference DEFOdomWarnRange which is an integer representing the + or - range, typically 5 
                string prefOdomWarnRange = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                            employeeMaster.TerminalId, PrefDriverConstants.DEFOdomWarnRange, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                if (null != prefOdomWarnRange)
                {
                    //For testing
                    log.Debug("SRTEST:Check Odometer");
                    log.DebugFormat("SRTEST:OdomRange:{0} PowerId:{1} Odom:{2} OdomFromDriver:{3}",
                                     prefOdomWarnRange,
                                     powerMaster.PowerId,
                                     powerMaster.PowerOdometer,
                                     driverOdomUpdateProcess.Odometer);

                    var deltaMiles = int.Parse(prefOdomWarnRange);
                    if (driverOdomUpdateProcess.Odometer < powerMaster.PowerOdometer.Value - deltaMiles ||
                        driverOdomUpdateProcess.Odometer > powerMaster.PowerOdometer.Value + deltaMiles)
                    {
                        var s = "Warning! Please check odometer and log in again.";
                        //log.Warn(s);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return false;
                    }
                }
            }

            ////////////////////////////////////////////////
            //Update PowerMaster for the original power unit
            //Save the original power id, in case we need it later
            string origPowerId = driverStatus.PowerId;
            ////////////////////////////////////////////////
            // Get the PowerMaster record.  driverStatus.PowerId contains the original power unit.
            var origPowerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                  origPowerId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == origPowerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid PowerId: "
                                + origPowerId));
                return false;
            }

            //Original power unit is no longer in use
            origPowerMaster.PowerStatus = PowerStatusConstants.Available;
            origPowerMaster.PowerDriverId = null;

            //Remove trip information
            origPowerMaster.PowerCurrentTripNumber = null;
            origPowerMaster.PowerCurrentTripSegNumber = null;
            origPowerMaster.PowerCurrentTripSegType = null;

            origPowerMaster.PowerLastActionDateTime = driverOdomUpdateProcess.ActionDateTime;

            //Do the update
            changeSetResult = Common.UpdatePowerMaster(dataService, settings, origPowerMaster);
            log.DebugFormat("SRTEST:Saving PowerMaster Record for OrigPowerId:{0} - OdomUpdate.",
                             origPowerId);
            if (Common.LogChangeSetFailure(changeSetResult, origPowerMaster, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update PowerMaster for OrigPowerId:{0}.",
                                       origPowerId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            ////////////////////////////////////////////////
            //Add record to PowerHistory table. Pass null for destCustomerMaster.
            if (!Common.InsertPowerHistory(dataService, settings, origPowerMaster, employeeMaster, null,
                ++origPowerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("InsertPowerHistory failed: {0} during power unit change request: {1}", fault.Message,
                                  driverOdomUpdateProcess);
                return false;
            }

            ////////////////////////////////////////////////
            //Update PowerMaster for the new power unit
            //Power unit is now in use
            powerMaster.PowerStatus = PowerStatusConstants.InUse;
            powerMaster.PowerDriverId = driverOdomUpdateProcess.EmployeeId;
            powerMaster.PowerOdometer = driverOdomUpdateProcess.Odometer;
            powerMaster.PowerLastActionDateTime = driverOdomUpdateProcess.ActionDateTime;

            //Do the update
            changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
            log.DebugFormat("SRTEST:Saving PowerMaster Record for NewPowerId:{0} Odom:{1} - OdomUpdate.",
                            driverOdomUpdateProcess.PowerId, driverOdomUpdateProcess.Odometer);
            if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update PowerMaster for NewPowerId:{0} Odom:{1}.",
                                        driverOdomUpdateProcess.PowerId, driverOdomUpdateProcess.Odometer);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            ////////////////////////////////////////////////
            //Add record to PowerHistory table. Pass null for destCustomerMaster.
            if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, null,
                ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("InsertPowerHistory failed: {0} during power unit change request: {1}", fault.Message,
                                  driverOdomUpdateProcess);
                return false;
            }
            ////////////////////////////////////////////////
            //Update the DriverStatus table. 
            driverStatus.PowerId = driverOdomUpdateProcess.PowerId;
            driverStatus.Odometer = driverOdomUpdateProcess.Odometer;
            driverStatus.ActionDateTime = driverOdomUpdateProcess.ActionDateTime;

            //Do the update
            changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
            log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - OdomUpdate.",
                            driverStatus.EmployeeId);
            if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update DriverStatus for DriverId:{0}.",
                    driverStatus.EmployeeId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            ////////////////////////////////////////////////
            //Do not insert a driver history as it creates too many entries.

            ///////////////////////////////////////////////
            //Get a list of incomplete trips for driver with the original power id
            var tripList = Common.GetTripsForDriverAndPowerId(dataService, settings, userCulture, userRoleIds,
                           driverOdomUpdateProcess.EmployeeId, origPowerId,out fault);

            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //There may not be any.
            if (null != tripList)
            {
                foreach (var trip in tripList)
                {
                    trip.TripPowerId = driverOdomUpdateProcess.PowerId;

                    //Do the update
                    changeSetResult = Common.UpdateTrip(dataService, settings, trip);
                    log.DebugFormat("SRTEST:Saving PowerId in Trip Record for Trip:{0} PowerId:{1} - OdomUpdate.",
                                    trip.TripNumber, driverOdomUpdateProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, trip, log))
                    {
                        var s = string.Format("DriverOdomUpdateProcess:Could not update Trip for Trip:{0} PowerId{1}.",
                                trip.TripNumber, driverOdomUpdateProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                }
            }

            ///////////////////////////////////////////////
            //Get a list of trip segments for the incomplete trips for driver with the original power id
            var tripSegList = Common.GetTripSegmentsForDriverAndPowerId(dataService, settings, userCulture, userRoleIds,
                              tripList, origPowerId, out fault);

            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //There may not be any.
            if (null != tripSegList)
            {
                foreach (var tripSegment in tripSegList)
                {
                    tripSegment.TripSegPowerId = driverOdomUpdateProcess.PowerId;

                    //Do the update
                    changeSetResult = Common.UpdateTripSegment(dataService, settings, tripSegment);
                    log.DebugFormat("SRTEST:Saving PowerId in TripSegment Record for Trip:{0}-{1} PowerId:{2} - OdomUpdate.",
                                    tripSegment.TripNumber, tripSegment.TripSegNumber, driverOdomUpdateProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegment, log))
                    {
                        var s = string.Format("DriverOdomUpdateProcess:Could not update TripSegment for Trip:{0}-{1} PowerId:{2}.",
                            tripSegment.TripNumber, tripSegment.TripSegNumber, driverOdomUpdateProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                }
            }
            ///////////////////////////////////////////////
            //Get a list of trip mileage segments for the incomplete trips for driver with the original power id
            var tripSegMileageList = Common.GetTripSegmentMileageForDriverAndPowerId(dataService, settings, userCulture, userRoleIds,
                              tripList, origPowerId, out fault);

            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //There may not be any.
            if (null != tripSegMileageList)
            {
                foreach (var tripSegMileage in tripSegMileageList)
                {
                    tripSegMileage.TripSegMileagePowerId = driverOdomUpdateProcess.PowerId;

                    //Do the update
                    changeSetResult = Common.UpdateTripSegmentMileage(dataService, settings, tripSegMileage);
                    log.DebugFormat("SRTEST:Saving PowerId in TripSegmentMileage Record for Trip:{0}-{1} PowerId:{2} - OdomUpdate.",
                                    tripSegMileage.TripNumber, tripSegMileage.TripSegNumber, driverOdomUpdateProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegMileage, log))
                    {
                        var s = string.Format("DriverOdomUpdateProcess:Could not update TripSegmentMileage for Trip:{0}-{1} PowerId:{2}.",
                            tripSegMileage.TripNumber, tripSegMileage.TripSegNumber, driverOdomUpdateProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                }
            }

            ////////////////////////////////////////////////
            //Add entry to Event Log – PowerUnitChange. 
            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(EventCommentConstants.ReceivedDriverPowerUnitChange);
            sbComment.Append(" HH:");
            sbComment.Append(driverOdomUpdateProcess.ActionDateTime);
            //if (driverStatus.TripNumber != null)
            //{
            //    sbComment.Append(" Trip:");
            //    sbComment.Append(driverStatus.TripNumber);
            //    sbComment.Append("-");
            //    sbComment.Append(driverStatus.TripSegNumber);
            //}
            sbComment.Append(" Drv:");
            sbComment.Append(driverOdomUpdateProcess.EmployeeId);
            sbComment.Append(" OrigPwr:");
            sbComment.Append(origPowerId);
            sbComment.Append(" NewPwr:");
            sbComment.Append(driverOdomUpdateProcess.PowerId);
            sbComment.Append(" Odom:");
            sbComment.Append(driverOdomUpdateProcess.Odometer);
            string comment = sbComment.ToString().Trim();

            var eventLog = new EventLog()
            {
                EventDateTime = driverOdomUpdateProcess.ActionDateTime,
                EventSeqNo = 0,
                EventTerminalId = employeeMaster.TerminalId,
                EventRegionId = employeeMaster.RegionId,
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverStatus.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - OdomUpdate");
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update EventLog for Driver {0} {1}.",
                        driverOdomUpdateProcess.EmployeeId, EventCommentConstants.ReceivedDriverOdomUpdate);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Correct Odometer
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverOdomUpdateProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="driverStatus"></param>
        /// <returns></returns>
        public bool CorrectOdometer(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
           DriverOdomUpdateProcess driverOdomUpdateProcess, EmployeeMaster employeeMaster, DriverStatus driverStatus)
        {
            DataServiceFault fault = null;

            ////////////////////////////////////////////////
            // Get the PowerMaster record.  
            var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                              driverOdomUpdateProcess.PowerId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == powerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid PowerId: "
                                + driverOdomUpdateProcess.PowerId));
                return false;
            }

            // original odometer should not be null, but if it is, set to 0
            if (powerMaster.PowerOdometer == null)
            {
                powerMaster.PowerOdometer = 0;
            }

            //Calculate the difference
            int? odometerDifference = driverOdomUpdateProcess.Odometer - powerMaster.PowerOdometer;

            //Make sure difference is not 0
            if (odometerDifference == 0)
            {
                //Nothing to do
                return true;
            }
            ////////////////////////////////////////////////
            //Update PowerMaster
            powerMaster.PowerOdometer = driverOdomUpdateProcess.Odometer;
            powerMaster.PowerLastActionDateTime = driverOdomUpdateProcess.ActionDateTime;

            //Do the update
            changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
            log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} Odom:{1} - OdomUpdate.",
                            driverOdomUpdateProcess.PowerId, driverOdomUpdateProcess.Odometer);
            if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update PowerMaster for PowerId:{0} Odom:{1}.",
                                        driverOdomUpdateProcess.PowerId, driverOdomUpdateProcess.Odometer);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Do not insert a power history as it creates too many entries.

            ////////////////////////////////////////////////
            //Update the DriverStatus table. 
            driverStatus.Odometer = driverOdomUpdateProcess.Odometer;
            driverStatus.ActionDateTime = driverOdomUpdateProcess.ActionDateTime;

            //Do the update
            changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
            log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - OdomUpdate.",
                            driverStatus.EmployeeId);
            if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update DriverStatus for DriverId:{0}.",
                    driverStatus.EmployeeId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            ////////////////////////////////////////////////
            //Do not insert a driver history as it creates too many entries.

            //Define for use later
            Trip currentTrip = new Trip();
            List<TripSegment> tripSegList = new List<TripSegment>();
            TripSegment currentTripSegment = new TripSegment();
            List<TripSegmentMileage> tripMileageList = new List<TripSegmentMileage>();

            if (driverStatus.TripNumber != null)
            {
                ////////////////////////////////////////////////
                // Get the Trip record
                currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                              driverStatus.TripNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                if (null == currentTrip)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid TripNumber: "
                                    + driverStatus.TripNumber));
                    return false;
                }
                ////////////////////////////////////////////////
                //Get a list of all  segments for the trip
                tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                                    driverStatus.TripNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                if (null == tripSegList)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid TripNumber: "
                                    + driverStatus.TripNumber));
                    return false;
                }
                ////////////////////////////////////////////////
                // Get the current TripSegment record
                currentTripSegment = (from item in tripSegList
                                          where item.TripSegNumber == driverStatus.TripSegNumber
                                          select item).FirstOrDefault();
                if (null == currentTripSegment)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverOdomUpdateProcess:Invalid TripSegment: " +
                        driverStatus.TripNumber + "-" + driverStatus.TripSegNumber));
                    return false;
                }
                //Update the odometers in the TripSegment records
                foreach (var tripSegment in tripSegList)
                {
                    if (-1 == tripSegment.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber) ||
                        0 == tripSegment.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber))
                    {
                        if (tripSegment.TripSegOdometerStart != null)
                            tripSegment.TripSegOdometerStart += odometerDifference;
                        if (tripSegment.TripSegOdometerEnd != null)
                            tripSegment.TripSegOdometerEnd += odometerDifference;

                        //Do the update
                        changeSetResult = Common.UpdateTripSegment(dataService, settings, tripSegment);
                        log.DebugFormat("SRTEST:Saving Odometer in TripSegment Record for Trip:{0}-{1} OdomDiff:{2} - OdomUpdate.",
                                        tripSegment.TripNumber, tripSegment.TripSegNumber, odometerDifference);
                        if (Common.LogChangeSetFailure(changeSetResult, tripSegment, log))
                        {
                            var s = string.Format("DriverOdomUpdateProcess:Could not update TripSegment for Trip:{0}-{1}.",
                                tripSegment.TripNumber, tripSegment.TripSegNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                }//end of foreach (var tripSegment in tripSegList)

                ////////////////////////////////////////////////
                //Get a list of all  mileage records for the trip
                tripMileageList = Common.GetTripSegmentMileageForTrip(dataService, settings, userCulture, userRoleIds,
                                    driverStatus.TripNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //There may not be any.
                if (null != tripMileageList)
                {
                    //Update the odometers in the TripSegmentMileage records
                    foreach (var tripMileage in tripMileageList)
                    {
                        if (-1 == tripMileage.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber) ||
                            0 == tripMileage.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber))
                        {
                            if (tripMileage.TripSegMileageOdometerStart != null)
                                tripMileage.TripSegMileageOdometerStart += odometerDifference;
                            if (tripMileage.TripSegMileageOdometerEnd != null)
                                tripMileage.TripSegMileageOdometerEnd += odometerDifference;

                            //Do the update
                            changeSetResult = Common.UpdateTripSegmentMileage(dataService, settings, tripMileage);
                            log.DebugFormat("SRTEST:Saving Odometer in TripSegment Record for Trip:{0}-{1} OdomDiff:{2} - OdomUpdate.",
                                            tripMileage.TripNumber, tripMileage.TripSegNumber, odometerDifference);
                            if (Common.LogChangeSetFailure(changeSetResult, tripMileage, log))
                            {
                                var s = string.Format("DriverOdomUpdateProcess:Could not update TripSegment for Trip:{0}-{1}.",
                                    tripMileage.TripNumber, tripMileage.TripSegNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }//end of foreach (var tripSegment in tripSegList)
                }//end of if (null != tripMileageList)

                ////////////////////////////////////////////////
                //Get a list of PowerFuel records for the trip
                var powerFuelList = Common.GetPowerFuelForTrip(dataService, settings, userCulture, userRoleIds,
                                    driverOdomUpdateProcess.PowerId, driverStatus.TripNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //There may not be any.
                if (null != powerFuelList)
                {
                    //Update odometers for the PowerFuel records for the trip
                    foreach (var powerFuel in powerFuelList)
                    {
                        if (powerFuel.PowerOdometer != null)
                        {
                            powerFuel.PowerOdometer += odometerDifference;
                            //Do the update
                            changeSetResult = Common.UpdatePowerFuel(dataService, settings, powerFuel);
                            log.DebugFormat("SRTEST:Saving Odometer in PowerFuel Record for PowerId:{0} Odom:{1} OdomDiff:{2} - OdomUpdate.",
                                            powerFuel.PowerId, powerFuel.PowerOdometer, odometerDifference);
                            if (Common.LogChangeSetFailure(changeSetResult, powerFuel, log))
                            {
                                var s = string.Format("DriverOdomUpdateProcess:Could not update PowerFuel for PowerId:{0} Odom:{1} OdomDiff:{2}.",
                                        powerFuel.PowerId, powerFuel.PowerOdometer, odometerDifference);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }//end of if (powerFuel.PowerOdometer != null)
                    }//end of foreach (var powerFuel in powerFuelList)
                }//end of if (null != powerFuelList)

                ////////////////////////////////////////////////
                //Get a list of PowerHistory records for the trip.
                var powerHistoryList = Common.GetPowerHistoryForTrip(dataService, settings, userCulture, userRoleIds,
                                  driverOdomUpdateProcess.PowerId, driverStatus.TripNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //There may not be any.
                if (null != powerHistoryList)
                {
                    //Update any PowerHistory records for the trip.
                    foreach (var powerHistory in powerHistoryList)
                    {
                        if (powerHistory.PowerOdometer != null)
                        {
                            powerHistory.PowerOdometer += odometerDifference;
                            //Do the update
                            changeSetResult = Common.UpdatePowerHistory(dataService, settings, powerHistory);
                            log.DebugFormat("SRTEST:Saving Odometer in PowerHistory Record for PowerId:{0} Odom:{1} OdomDiff:{2} - OdomUpdate.",
                                            powerHistory.PowerId, powerHistory.PowerOdometer, odometerDifference);
                            if (Common.LogChangeSetFailure(changeSetResult, powerHistory, log))
                            {
                                var s = string.Format("DriverOdomUpdateProcess:Could not update PowerHistory for PowerId:{0} Odom:{1} OdomDiff:{2}.",
                                        powerHistory.PowerId, powerHistory.PowerOdometer, odometerDifference);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }//end of if (powerHistory.PowerOdometer != null)
                    }//end of foreach (var powerHistory in powerHistoryList)
                }//end of if (null != powerHistoryList)

                ////////////////////////////////////////////////
                //Get a list of DriverHistory records for the trip.
                var driverHistoryList = Common.GetDriverHistoryForTrip(dataService, settings, userCulture, userRoleIds,
                                          driverOdomUpdateProcess.EmployeeId, driverStatus.TripNumber, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                if (null != driverHistoryList)
                {
                    //Update any DriverHistory records for the trip.
                    foreach (var driverHistory in driverHistoryList)
                    {
                        if (driverHistory.Odometer != null)
                        {
                            driverHistory.Odometer += odometerDifference;
                            //Do the update
                            changeSetResult = Common.UpdateDriverHistory(dataService, settings, driverHistory);
                            log.DebugFormat("SRTEST:Saving Odometer in DriverHistory Record for DriverId:{0} Odom:{1} OdomDiff:{2} - OdomUpdate.",
                                            driverHistory.EmployeeId, driverHistory.Odometer, odometerDifference);
                            if (Common.LogChangeSetFailure(changeSetResult, driverHistory, log))
                            {
                                var s = string.Format("DriverOdomUpdateProcess:Could not update DriverHistory for DriverId:{0} Odom:{1} OdomDiff:{2}.",
                                        driverHistory.EmployeeId, driverHistory.Odometer, odometerDifference);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }//end of if (driverHistory.Odometer != null)
                    }//end of foreach (var driverHistory in driverHistoryList)
                }//end of if (null != driverHistoryList)

            }//end of if (driverStatus.TripNumber != null)


            ////////////////////////////////////////////////
            //Add entry to Event Log – OdomUpdate. 
            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(EventCommentConstants.ReceivedDriverOdomUpdate);
            sbComment.Append(" HH:");
            sbComment.Append(driverOdomUpdateProcess.ActionDateTime);
            if (driverStatus.TripNumber != null)
            {
                sbComment.Append(" Trip:");
                sbComment.Append(driverStatus.TripNumber);
                sbComment.Append("-");
                sbComment.Append(driverStatus.TripSegNumber);
            }
            sbComment.Append(" Drv:");
            sbComment.Append(driverOdomUpdateProcess.EmployeeId);
            sbComment.Append(" Pwr:");
            sbComment.Append(driverOdomUpdateProcess.PowerId);
            sbComment.Append(" Odom:");
            sbComment.Append(driverOdomUpdateProcess.Odometer);
            string comment = sbComment.ToString().Trim();

            var eventLog = new EventLog()
            {
                EventDateTime = driverOdomUpdateProcess.ActionDateTime,
                EventSeqNo = 0,
                EventTerminalId = employeeMaster.TerminalId,
                EventRegionId = employeeMaster.RegionId,
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverStatus.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - OdomUpdate");
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("DriverOdomUpdateProcess:Could not update EventLog for Driver {0} {1}.",
                        driverOdomUpdateProcess.EmployeeId, EventCommentConstants.ReceivedDriverOdomUpdate);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            return true;
        }//end of public bool CorrectOdometer
    }
}

