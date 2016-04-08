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
    /// The process for logging in a driver.  Call this process "withoutrequery".
    /// </summary>
    /// 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverLoginProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.
    [EditAction("DriverLoginProcess")]
    public class DriverLoginProcessRecordType : ChangeableRecordType
        <DriverLoginProcess, string, DriverLoginProcessValidator, DriverLoginProcessDeletionValidator>
    {

        /// <summary>
        /// The obligatory abstract function implementation
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverLoginProcess, DriverLoginProcess>();
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
            ChangeSet<string, DriverLoginProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }


        /// <summary>
        /// Perform the extensing login checking and updating assocaited with a driver login.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
            ChangeSet<string, DriverLoginProcess> changeSet, ProcessChangeSetSettings settings)
        {

            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted
            // then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository) repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security, piplines, etc.  Note however, we will loose non-persisted user input values which 
            // will not be propagated through into the changeSetResult.
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only log in one person at a time but in the more genreal cases we could be processing multiple records.
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DriverLoginProcess driverLoginProcess = (DriverLoginProcess) changeSetResult.GetSuccessfulUpdateForId(key);
                    try
                    {

                        DataServiceFault fault;
                        string msgKey = key;
                        ChangeSetResult<string> scratchChangeSetResult;
                        int powerHistoryInsertCount = 0;
                        int driverHistoryInsertCount = 0;

                        // TODO: determine these on a case by case basis.
                        string userCulture = "en-GB";
                        IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                        // It appears in the general case I must backfill non-persisted user input values from the initial change set.
                        // They will not be propagated through into the changeSetResult of base base.ProcessChangeSet() method.
                        DriverLoginProcess backfillDriverLoginProcess;
                        if (changeSet.Update.TryGetValue(key, out backfillDriverLoginProcess))
                        {
                            // Use a mapper?  This might not always be the best approach.  
                            // And these are static!  Can I set this to map a subset?
                            Mapper.Map(backfillDriverLoginProcess, driverLoginProcess);

                            // OR do I just simply backfill the minimalist set?
                            //driverLoginProcess.CodeListVersion = backfillDriverLoginProcess.CodeListVersion;
                            //driverLoginProcess.LastContainerMasterUpdate = backfillDriverLoginProcess.LastContainerMasterUpdate;
                            //driverLoginProcess.LastTerminalMasterUpdate = backfillDriverLoginProcess.LastTerminalMasterUpdate;
                            //driverLoginProcess.LocaleCode = backfillDriverLoginProcess.LocaleCode;
                            //driverLoginProcess.Mdtid = backfillDriverLoginProcess.Mdtid;
                            //driverLoginProcess.Odometer = backfillDriverLoginProcess.Odometer;
                            //driverLoginProcess.OverrideFlag = backfillDriverLoginProcess.OverrideFlag;
                            //driverLoginProcess.PndVer = backfillDriverLoginProcess.PndVer;
                            //driverLoginProcess.PowerId = backfillDriverLoginProcess.PowerId;
                            //driverLoginProcess.LoginDateTime = backfillDriverLoginProcess.LoginDateTime
                        }
                        else
                        {
                            var str = "Unable to process login for Driver ID.  Can find base request object." +
                                      driverLoginProcess.EmployeeId;
                            log.Error(str);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(str));
                            break;
                        }

                        //
                        // 1) Process information from handheld
                        // NOTE: We might want to validate (or backfill) something like Locale against configured dialects rather than a hardcoded list in validator
                        //

                        ////////////////////////////////////////////////
                        // 2) Validate driver id / Get the EmployeeMaster
                        // UserId must be a valid EmployeeId in the EmployeeMaster and SecurityLevel must be DR for driver.
                        var employeeMaster = Util.Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.EmployeeId, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == employeeMaster)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + driverLoginProcess.EmployeeId));
                            break;
                        }
                        //For testing
                        log.Debug("SRTEST:GetEmployeeDriver");
                        log.DebugFormat("SRTEST:Driver:{0} Terminal:{1} SecurityLevel:{2}",
                                         employeeMaster.EmployeeId,
                                         employeeMaster.TerminalId,
                                         employeeMaster.SecurityLevel);
                        //
                        // 3) Lookup preferences.  
                        // See PreferencesProcess.
                        //

                        // 4b) Check system pref "DEFAllowAnyPowerUnit".  
                        // If preference prefAllowAnyPowerUnit is set to Y, then allow any power unit to be valid
                        // Otherwise the power unit's region must match the driver's region. This is the norm.
                        string prefAllowAnyPowerUnit = Util.Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                       Constants.SYSTEM_TERMINALID, PrefSystemConstants.DEFAllowAnyPowerUnit, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // 4a) Validate PowerId
                        // PowerId must be a valid PowerId in the PowerMaster 
                        var powerMaster = new PowerMaster();
                        if (prefAllowAnyPowerUnit == Constants.Yes)
                        {
                            powerMaster = Util.Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.PowerId, out fault);
                        }
                        else
                        {
                            powerMaster = Util.Common.GetPowerUnitForRegion(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.PowerId, employeeMaster.RegionId, out fault);
                        }
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == powerMaster)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Power ID " + driverLoginProcess.PowerId));
                            break;
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
                        // 4c) Check power unit status: Scheduled for the shop? PS_SHOP = "S"
                        if (PowerStatusConstants.Shop == powerMaster.PowerStatus)
                        {
                            var s = string.Format("Do not use Power unit {0}.  It is scheduled for the shop. ", driverLoginProcess.PowerId);
                            log.Error(s);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // 4d) Is unit in use by another driver? PS_INUSE = "I"
                        if (powerMaster.PowerStatus == PowerStatusConstants.InUse &&
                            powerMaster.PowerDriverId != employeeMaster.EmployeeId)
                        {
                            var s = string.Format("Power unit {0} in use by another driver.  Change power unit number or call Dispatch.",
                                driverLoginProcess.PowerId);
                            log.Error(s);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // 4e) If no odometer record for this unit, accept as sent by the handheld.  
                        // 4f) If overrride flag = "Y", accept as sent by the handheld.  
                        // 4g) Odometer tolerance checks.
                        // If the override flag is not set, and the odometer is within the range preference then accept the odometer 
                        if (driverLoginProcess.OverrideFlag != Constants.Yes && powerMaster.PowerOdometer != null)
                        {
                            //Get the preference DEFOdomWarnRange which is an integer representing the + or - range, typically 5 
                            string prefOdomWarnRange = Util.Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                        employeeMaster.TerminalId, PrefDriverConstants.DEFOdomWarnRange, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            if (null != prefOdomWarnRange)
                            {
                                //For testing
                                log.Debug("SRTEST:Check Odometer");
                                log.DebugFormat("SRTEST:OdomRange:{0} PowerId:{1} Odom:{2} OdomFromDriver:{3}",
                                                 prefOdomWarnRange,
                                                 powerMaster.PowerId,
                                                 powerMaster.PowerOdometer,
                                                 driverLoginProcess.Odometer);

                                var deltaMiles = int.Parse(prefOdomWarnRange);
                                if (driverLoginProcess.Odometer < powerMaster.PowerOdometer.Value - deltaMiles ||
                                    driverLoginProcess.Odometer > powerMaster.PowerOdometer.Value + deltaMiles)
                                {
                                    var s = "Warning! Please check odometer and log in again.";
                                    //log.Warn(s);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }
                            }
                        }

                        ////////////////////////////////////////////////
                        //Get the driver status record for the driver. If one does not exist, eventually add one.
                        var driverStatus = Util.Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                    driverLoginProcess.EmployeeId, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }

                        // If no driver status record is found create one to add
                        if (null == driverStatus)
                        {
                            driverStatus = new DriverStatus()
                            {
                                EmployeeId = driverLoginProcess.EmployeeId,
                            };
                        }
                        //Now there is a driverStatus
                        //For testing
                        log.Debug("SRTEST:GetDriverStatus");
                        log.DebugFormat("SRTEST:DriverId:{0} DriverStatus:{1} TripNumber:{2} Seg:{3}",
                                                            driverStatus.EmployeeId,
                                                            driverStatus.Status,
                                                            driverStatus.TripNumber,
                                                            driverStatus.TripSegNumber);

                        // 5) Validate Duplicate login?  Assuming we don't need this at this time.
                        //    if (driverStatus.PowerId == driverLoginProcess.PowerId &&
                        //        driverStatus.Odometer == driverLoginProcess.Odometer)
                        //    {
                        //        var timeSpan = driverStatus.LoginDateTime - driverLoginProcess.LoginDateTime;
                        //        if (timeSpan < TimeSpan.FromSeconds(30) && timeSpan > TimeSpan.FromSeconds(-30))
                        //        {
                        //            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Duplicate Login"));
                        //            break;
                        //        }
                        //    }

                        ////////////////////////////////////////////////
                        // 6) Determine current trip number, segment, and driver status
                        //The purpose of the current trip info is, if the driver logs out and back in again in the
                        //middle of a trip, the mobile device can position the app on the current segment.
                        //This is to prevent the driver from having to re-enter information that he has already completed.

                        //If there is no current trip in the driver status record, then set these fields to null
                        //This means that the driver is not in the middle of a trip.
                        if (null == driverStatus.TripNumber)
                        {
                            driverLoginProcess.TripNumber = null;
                            driverLoginProcess.TripSegNumber = null;
                            driverLoginProcess.DriverStatus = null;
                        }
                        else
                        {
                            //We need to send this information back to the mobile device:
                            //TripNumber, TripSegNumber, DriverStatus
                            driverLoginProcess.TripNumber = driverStatus.TripNumber;

                            //If the driver status is done for this segment, then determine his next segment number.
                            //If he is done, we want the mobile device to position the app at the start of his next segment.
                            if (DriverStatusSRConstants.Done == driverStatus.Status)
                            {
                                var nextTripSegment = Util.Common.GetNextIncompleteTripSegment(dataService, settings, userCulture, userRoleIds,
                                                              driverStatus.TripNumber, out fault);
                                if (null != fault)
                                {
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                    break;
                                }
                                driverLoginProcess.TripSegNumber = nextTripSegment.TripSegNumber;
                                //We can't leave the status as done. 
                                //No need to do this after all, since we will be removing the driverstatus.
                                //driverLoginProcess.DriverStatus = DriverStatusSRConstants.Available;

                                //For testing
                                log.Debug("SRTEST:GetNextIncompleteTripSegment");
                                log.DebugFormat("SRTEST:TripNumber:{0} NextSegment:{1}",
                                                 nextTripSegment.TripNumber,
                                                 nextTripSegment.TripSegNumber);
                            }
                            //For all other statuses just use the current seg number in the driver status table.
                            else
                            {
                                driverLoginProcess.TripSegNumber = driverStatus.TripSegNumber;
                            }

                            //For testing
                            log.Debug("SRTEST:Driver Status");
                            log.DebugFormat("SRTEST:TripNumber:{0} Segment:{1} DriverStatus:{2} PrevDriverStatus:{3}",
                                             driverStatus.TripNumber,
                                             driverStatus.TripSegNumber,
                                             driverStatus.Status,
                                             driverStatus.PrevDriverStatus);

                            //Remove the driver status except for enroutes,arrives, and state line crossings
                            //Other statuses that the driver might have been in (delay, back on duty, fuel, done) don't matter anymore.
                            if (DriverStatusSRConstants.EnRoute != driverStatus.Status &&
                                DriverStatusSRConstants.Arrive != driverStatus.Status &&
                                DriverStatusSRConstants.StateCrossing != driverStatus.Status)
                            {
                                driverLoginProcess.DriverStatus = null;
                            }

                            //I think this covers the case where the driver might have been enroute, but then disconnected.
                            //The enroute is stored in the previous driver status, while disconnect is now the driver status.
                            //The enroute is the one we want to use.
                            //If there is a previous driver status, use it. Otherwise use driver status.
                            driverStatus.Status = driverStatus.PrevDriverStatus ?? driverStatus.Status;

                            ////////////////////////////////////////////////
                            // 7) Update Trip in progress flag in the trip table
                            if (driverStatus.TripNumber != null &&
                                driverStatus.TripSegNumber == Constants.TSS_FIRSTSEGNUMBER)
                            {
                                if (DriverStatusSRConstants.EnRoute != driverStatus.Status &&
                                    DriverStatusSRConstants.Arrive != driverStatus.Status &&
                                    DriverStatusSRConstants.StateCrossing != driverStatus.Status &&
                                    DriverStatusSRConstants.Done != driverStatus.Status &&
                                    DriverStatusSRConstants.Delay != driverStatus.Status &&
                                    DriverStatusSRConstants.BackOnDuty != driverStatus.Status &&
                                    DriverStatusSRConstants.Fuel != driverStatus.Status)
                                {
                                    // Update Trip in progress flag in the trip table to 'N'
                                    var trip = Util.Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                                driverStatus.TripNumber, out fault);
                                    if (null != fault)
                                    {
                                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                        break;
                                    }
                                    //For testing
                                    log.Debug("SRTEST:GetTrip to check TripInProgressFlag");
                                    log.DebugFormat("SRTEST:TripNumber:{0} TripInProgressFlag:{1} DriverStatus:{2}",
                                                                        trip.TripNumber,
                                                                        trip.TripInProgressFlag,
                                                                        driverStatus.Status);
                                    //Set to N only if it is not N already
                                    if (trip.TripInProgressFlag != Constants.No)
                                    {
                                        trip.TripInProgressFlag = Constants.No;

                                        scratchChangeSetResult = Common.UpdateTrip(dataService, settings, trip);
                                        log.Debug("SRTEST:Saving Trip Record - TripInProgressFlag");
                                        if (Common.LogChangeSetFailure(scratchChangeSetResult, trip, log))
                                        {
                                            var s = string.Format("Could not update Trip for TripInProgressFlag: {0}.", Constants.No);
                                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                            break;
                                        }
                                    }
                                }
                            }//end of Update Trip in progress flag in the trip table

                        }//end of else if (null == driverStatus.TripNumber)

                        ////////////////////////////////////////////////
                        // 8) Check for open ended delays 
                        CheckForOpenEndedDelays(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                     driverLoginProcess, employeeMaster, driverStatus, ref driverHistoryInsertCount);
                        /*
                        if (CheckForOpenEndedDelays(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                     driverLoginProcess, employeeMaster))
                        {
                            // 8b) Add "Back On Duty" flag to Driver History table.
                            // For the last open-ended delay that we added an end time, record a back on duty status in the DriverHistory table.
                            // The driver is logging in, so in effect he is back on duty, no longer on delay.
                            // No need to actually add it to the DriverStatus table since we will be adding a
                            // status of login very shortly and he did not actually click on back on duty. 
                            // Users viewing the driver history wanted to know where the back on duty was.
                            // We still need to do this even if there was no trip number because the driver can go on delay without being on a trip. 
                            // In that case the trip number in the DriverDelay table is #driverid
                            driverStatus.Status = DriverStatusSRConstants.BackOnDuty;
                            //For testing
                            log.Debug("SRTEST:Add BackOnDuty to DriverStatus, DriverHistory");
                            log.DebugFormat("SRTEST:Driver:{0} TripNumber:{1} Seg:{2} Status{3}",
                                             driverStatus.EmployeeId,
                                             driverStatus.TripNumber,
                                             driverStatus.TripSegNumber,
                                             driverStatus.Status);

                            //scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                            //if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                            //{
                            //    var s = "Could not update driver status";
                            //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            //    break;
                            //}
                            if (!InsertDriverHistory(dataService, settings, employeeMaster, driverStatus,
                                ++driverHistoryInsertCount, userRoleIds, userCulture, out fault))
                            {
                                if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                                {
                                    break;
                                }
                                var s = "Could not update driver status";
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                        */
                        ////////////////////////////////////////////////
                        // 9) Check for power unit change
                        // There has to be a trip number and trip segment number...
                        if (null != driverLoginProcess.TripNumber && null != driverLoginProcess.TripSegNumber)
                        {
                            CheckForPowerIdChange(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                                driverLoginProcess, powerMaster, employeeMaster);
                        }
                        // 10) Add/update record to DriverStatus table.

                        //These fields in the DriverStatus table are not used:
                        //     TripSegType, TripAssignStatus.TripStatus, TripSegStatus,DriverArea,RFIDFlag
                        //These fields are not affected by the login process:
                        //     DriverCumMinutes, GPSAutoGeneratedFlag, DelayCode, PrevDriverStatus, GPSXmitFlag, SendHHLogoffFlag
                        //This field is used by router and is no longer needed: RouteTo

                        // Now set the new values in the driverstatus table:
                        driverStatus.TripNumber = driverLoginProcess.TripNumber;
                        driverStatus.TripSegNumber = driverLoginProcess.TripSegNumber;
                        driverStatus.Status = DriverStatusSRConstants.LoggedIn;
                        driverStatus.TerminalId = employeeMaster.TerminalId;
                        driverStatus.RegionId = employeeMaster.RegionId;
                        driverStatus.PowerId = driverLoginProcess.PowerId;
                        driverStatus.MDTId = driverLoginProcess.Mdtid;
                        driverStatus.ActionDateTime = driverLoginProcess.LoginDateTime;
                        driverStatus.LoginDateTime = driverLoginProcess.LoginDateTime;
                        driverStatus.Odometer = driverLoginProcess.Odometer;
                        driverStatus.LoginProcessedDateTime = DateTime.Now;
                        driverStatus.ContainerMasterDateTime = driverLoginProcess.ContainerMasterDateTime;
                        driverStatus.MdtVersion = driverLoginProcess.PndVer;
                        driverStatus.TerminalMasterDateTime = driverLoginProcess.TerminalMasterDateTime;
                        driverStatus.DriverLCID = driverLoginProcess.LocaleCode;

                        scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                        log.Debug("SRTEST:Saving DriverStatus Record - Login");
                        if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                        {
                            var s = string.Format("Could not update DriverStatus for Driver {0}.",
                                driverLoginProcess.EmployeeId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // 11) Add record to DriverHistory table
                        if (!InsertDriverHistory(dataService, settings, employeeMaster, driverStatus,
                                ++driverHistoryInsertCount, userRoleIds, userCulture, out fault))
                        {
                            if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                            {
                                break;
                            }
                            changeSetResult.FailedUpdates.Add(msgKey,
                                new MessageSet(string.Format("Could not insert Driver History for Driver {0}.",
                                    driverLoginProcess.EmployeeId)));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // 12) Update PowerMaster table
                        powerMaster.PowerStatus = PowerStatusConstants.InUse;
                        powerMaster.PowerDriverId = driverLoginProcess.EmployeeId;
                        powerMaster.PowerOdometer = driverLoginProcess.Odometer;
                        powerMaster.PowerLastActionDateTime = driverLoginProcess.LoginDateTime;

                        // Trip Number and segment number are not populated at login.
                        // Only when driver goes enroute.
                        // In fact, we need to remove one if it exists
                        powerMaster.PowerCurrentTripNumber = null;
                        powerMaster.PowerCurrentTripSegNumber = null;

                        scratchChangeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                        log.Debug("SRTEST:Saving PowerMaster Record - Login");
                        if (Common.LogChangeSetFailure(scratchChangeSetResult, powerMaster, log))
                        {
                            var s = string.Format("Could not update Power Master for Power Unit {0}.",
                                driverLoginProcess.PowerId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // 13) Add record to PowerHistory table
                        if (!InsertPowerHistory(dataService, settings, powerMaster, employeeMaster,
                                ++powerHistoryInsertCount, userRoleIds, userCulture, out fault))
                        {
                            if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                            {
                                break;
                            }
                            changeSetResult.FailedUpdates.Add(msgKey,
                                new MessageSet(string.Format("Could not insert Power History for Power Unit {0}.",
                                    driverLoginProcess.PowerId)));
                            break;
                        }

                        // 14) Send container master updates
                        // See ContainerChangeProcess.

                        // 15) Send terminal master updates
                        // See TerminalChangeProcess.

                        // 16) Send Universal Commodities
                        // See CommodityMasterProcess.

                        // 17) Send preferences (after some filtering)
                        // See PreferencesProcess.

                        // 18) Send Code list
                        // See CodeTableProcess.

                        // 19) Send Trips
                        // See TripInfoProcess

                        ////////////////////////////////////////////////
                        // 20) Send Container Inventory
                        var containersOnPowerId = Util.Common.GetContainersForPowerId(dataService, settings, userCulture, userRoleIds,
                                                  driverLoginProcess.PowerId, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //For testing
                        log.Debug("SRTEST:GetContainersForPowerId");
                        foreach (var container in containersOnPowerId)
                        {
                            //This is some of the info that we send back
                            log.DebugFormat("SRTEST:Container#:{0} Loc:{1} Type:{2} Size:{3} Contents:{4} Trip:{5}-{6}",
                                             container.ContainerNumber,
                                             container.ContainerLocation,
                                             container.ContainerType,
                                             container.ContainerSize,
                                             container.ContainerContents,
                                             container.ContainerCurrentTripNumber,
                                             container.ContainerCurrentTripSegNumber);
                        }

                        ////////////////////////////////////////////////
                        // 21) Send dispatcher list for messaging
                        // Check system pref "DEFSendDispatchersForArea".  
                        // If preference DEFSendDispatchersForArea is set to Y, then send a list of dispatchers whose terminal id is in the driver's area. 
                        // Otherwise send a list of dispatchers whose region matches the driver's region.
                        string prefSendDispatchersForArea = Util.Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                       employeeMaster.TerminalId, PrefDriverConstants.DEFSendDispatchersForArea, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }

                        var usersForMessaging = new List<EmployeeMaster>();
                        if (prefSendDispatchersForArea == Constants.Yes)
                        {
                            usersForMessaging = Util.Common.GetDispatcherListForArea(dataService, settings, userCulture, userRoleIds,
                                                       employeeMaster.AreaId, out fault);
                        }
                        else
                        {
                            usersForMessaging = Util.Common.GetDispatcherListForRegion(dataService, settings, userCulture, userRoleIds,
                                                       employeeMaster.RegionId, out fault);
                        }
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //For testing
                        log.Debug("SRTEST:GetDispatcherList");
                        foreach (var user in usersForMessaging)
                        {
                            log.DebugFormat("SRTEST:LastName:{0} FirstName:{1} SecurityLevel:{2} AllowMessaging:{3} Region:{4} Terminal:{5} Area:{6}",
                                             user.LastName,
                                             user.FirstName,
                                             user.SecurityLevel,
                                             user.AllowMessaging,
                                             user.RegionId,
                                             user.TerminalId,
                                             user.AreaId);
                        }


                        // 22) populate and send the Login message back to the handheld

                        // 23) Send GPS Company Info to tracker

                        // 24) Send GV Driver START Packet to tracker

                        // 25) Send GV Driver CODE Packet to tracker

                        // 26) Send GV Driver NAME Packet to tracker

                        ////////////////////////////////////////////////
                        // 27) Add entry to Event Log - Login Received.
                        StringBuilder sbComment = new StringBuilder();
                        sbComment.Append(EventCommentConstants.ReceivedDriverLogin);
                        sbComment.Append(" HH:");
                        sbComment.Append(driverStatus.LoginDateTime);
                        if (null != driverStatus.TripNumber)
                        { 
                            sbComment.Append(" Trip:");
                            sbComment.Append(driverStatus.TripNumber);
                            sbComment.Append("-");
                            sbComment.Append(driverStatus.TripSegNumber);
                        }
                        sbComment.Append(" Drv:");
                        sbComment.Append(driverStatus.EmployeeId);
                        sbComment.Append(" Pwr:");
                        sbComment.Append(driverStatus.PowerId);
                        sbComment.Append(" Odom:");
                        sbComment.Append(driverStatus.Odometer);
                        string comment = sbComment.ToString().Trim();

                        var eventLog = new EventLog()
                        {
                            EventDateTime = driverStatus.LoginDateTime,
                            EventSeqNo = 0,
                            EventTerminalId = driverStatus.TerminalId,
                            EventRegionId = driverStatus.RegionId,
                            //These are not populated for logins in the current system.
                            // EventEmployeeId = driverStatus.EmployeeId,
                            // EventEmployeeName = Common.GetDriverName(employeeMaster),
                            EventTripNumber = driverStatus.TripNumber,
                            EventProgram = EventProgramConstants.Services,
                            //These are not populated for logins in the current system.
                            //EventScreen = null,
                            //EventAction = null,
                            EventComment = comment,
                        };

                        //scratchChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                        log.Debug("SRTEST:Saving EventLog Record - Login");
                        //if (Common.LogChangeSetFailure(scratchChangeSetResult, eventLog, log))
                        //{
                        //    var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                        //                         driverStatus.EmployeeId, EventCommentConstants.ReceivedDriverLogin);
                        //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        //    break;
                        //}

                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Unhandled general exception: {0} within driver login: {1}.", ex, driverLoginProcess);
                        changeSetResult.FailedUpdates.Add(key,
                            new MessageSet(string.Format("Unable to log in driver {0}.  A general error occured", 
                                driverLoginProcess.EmployeeId)));
                        break;
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
        /// <summary>
        /// Check for open ended delays for this driver
        /// </summary>
        public static void CheckForOpenEndedDelays(IDataService dataService, ProcessChangeSetSettings settings,
               ChangeSetResult<string> changeSetResult, String key, IEnumerable<long> userRoleIds, string userCulture,
               DriverLoginProcess driverLoginProcess, EmployeeMaster employeeMaster, DriverStatus driverStatus,  ref int driverHistoryInsertCount)
        {
            DataServiceFault fault;
            string msgKey = key;
            var driverDelays = Util.Common.GetDriverDelaysOpenEnded(dataService, settings, userCulture, userRoleIds,
                                             driverLoginProcess.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return;
            }
            //Even though no entries were found, the list was not null. Now also checking if count > 0.
            if (driverDelays != null && driverDelays.Count > 0)
            {
                log.Debug("SRTEST:GetDriverDelaysOpenEnded");
                //driverDelays is a list of driver delays that have no end date/time sorted by open datetime in descending order
                //Starting with the last record, set the end date/time to the login date/time
                //Then for each additional delay, set the end date/time to the previous delays start date/time
                //Usually only there will be no open-ended delays or perhaps just one.

                //Add 1 second to the action date time, so the BackOnDuty will display after the Login on the Driver History report
                //and the delay end time will match the driver history
                System.TimeSpan tSpan = new System.TimeSpan(0, 0, 0, 1);
                driverDelays[0].DelayEndDateTime = driverLoginProcess.LoginDateTime + tSpan;

                for (int i = 1; i < driverDelays.Count; i++)
                {
                    driverDelays[i].DelayEndDateTime = driverDelays[i - 1].DelayStartDateTime;
                }

                var driverDelayRecordType = (DriverDelayRecordType)
                    dataService.RecordTypes.Single(x => x.TypeName == "DriverDelay");
                var driverDelayChangeSet = (ChangeSet<string, DriverDelay>)
                    driverDelayRecordType.GetNewChangeSet();

                foreach (var driverDelay in driverDelays)
                {
                    //For testing
                    log.DebugFormat("SRTEST:TripNumber:{0} DelayCode:{1} Reason:{2} Start:{3} End:{4}",
                                                        driverDelay.TripNumber,
                                                        driverDelay.DelayCode,
                                                        driverDelay.DelayReason,
                                                        driverDelay.DelayStartDateTime,
                                                        driverDelay.DelayEndDateTime);

                    driverDelayChangeSet.AddUpdate(driverDelay.Id, driverDelay);
                    log.Debug("SRTEST:Saving Delay Record - Login");
                }
                var driverDelayChangeSetResult = driverDelayRecordType.ProcessChangeSet(dataService,
                    driverDelayChangeSet, settings);
                if (driverDelayChangeSetResult.FailedCreates.Any() ||
                    driverDelayChangeSetResult.FailedUpdates.Any() ||
                    driverDelayChangeSetResult.FailedDeletions.Any())
                {
                    var s = "Could not update open ended driver delays";
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return;
                }

                // 8b) Add "Back On Duty" flag to Driver History table.
                // For the last open-ended delay that we added an end time, record a back on duty status in the DriverHistory table.
                // The driver is logging in, so in effect he is back on duty, no longer on delay.
                // No need to actually add it to the DriverStatus table since we will be adding a
                // status of login very shortly and he did not actually click on back on duty. 
                // Users viewing the driver history wanted to know where the back on duty was.
                // We still need to do this even if there was no trip number because the driver can go on delay without being on a trip. 
                // In that case the trip number in the DriverDelay table is #driverid
                DriverStatus bodDriverStatus = driverStatus;
                bodDriverStatus.Status = DriverStatusSRConstants.BackOnDuty;
                //Add 1 second to the action date time, so the BackOnDuty will display after the Login on the Driver History report
                bodDriverStatus.ActionDateTime = driverStatus.ActionDateTime + tSpan;
                //The delay code is stored in the DriverStatus table but not in the DriverHistory table, so it is not needed.
                log.Debug("SRTEST:Add BackOnDuty to DriverHistory");
                //scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                //if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                //{
                //    var s = "Could not update driver status";
                //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                //    break;
                //}
                if (!InsertDriverHistory(dataService, settings, employeeMaster, bodDriverStatus,
                    ++driverHistoryInsertCount, userRoleIds, userCulture, out fault))
                {
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        return;
                    }
                    return;
                }
            }
            //None found
            return;
        }

  
        /// <summary>
        /// Check for power unit change in the middle of a segment
        /// </summary>
        // If trip segment is partially completed & power unit id from the login is different that the
        // power unit in the trip segment table, then we want to use the new power unit
        // Also check for an open ended entry in the TripSegmentMileage table.
        // There should only be one, but there may be more.
        // Here I think, we just want to get the last one, since our purpose here is to adjust the power id
        // not to close open-ended entries.
        public static void CheckForPowerIdChange(IDataService dataService, ProcessChangeSetSettings settings,
                   ChangeSetResult<string> changeSetResult,String key, IEnumerable<long> userRoleIds, string userCulture,
                   DriverLoginProcess driverLoginProcess, PowerMaster powerMaster, EmployeeMaster employeeMaster)
        {
            ChangeSetResult<string> scratchChangeSetResult;
            DataServiceFault fault;
            string msgKey = key;
            //Get the current trip segment if open-ended
            var tripSegment = Util.Common.GetTripSegmentOpenEnded(dataService, settings, userCulture, userRoleIds,
                                    driverLoginProcess.TripNumber, driverLoginProcess.TripSegNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return;
            }
            if (null != tripSegment)
            {
                //For testing
                log.Debug("SRTEST:GetTripSegmentOpenEnded");
                log.DebugFormat("SRTEST:TripNumber:{0} Seg:{1} PrevPowerId:{2} PrevOdomStart:{3} PowerId:{4} OdomStart:{5}",
                                                    tripSegment.TripNumber,
                                                    tripSegment.TripSegNumber,
                                                    tripSegment.TripSegPowerId,
                                                    tripSegment.TripSegOdometerStart,
                                                    driverLoginProcess.PowerId,
                                                    driverLoginProcess.Odometer);

                // if the power unit has changed and the trip segment has been started but not finished we have a dilema.
                // What we currently do is set the new power id as the power id entire segment even though the first power id
                // may have completed part of the trip. 
                if (driverLoginProcess.PowerId != tripSegment.TripSegPowerId)
                {
                    tripSegment.TripSegPowerId = driverLoginProcess.PowerId;
                    tripSegment.TripSegOdometerStart = driverLoginProcess.Odometer;

                    //Update the TripSegment record
                    scratchChangeSetResult = Common.UpdateTripSegment(dataService, settings, tripSegment);
                    log.Debug("SRTEST:Saving TripSegment Record - Login (Power Unit Change)");
                    if (Common.LogChangeSetFailure(scratchChangeSetResult, tripSegment, log))
                    {
                        var s = string.Format("Could not update TripSegment for TripNumber {0}-{1}.",
                                               tripSegment.TripNumber, tripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return;
                    }
                }
            }
            //Get the last open-ended trip segment mileage 
            var tripSegmentMileage = Util.Common.GetTripSegmentMileageOpenEndedLast(dataService, settings, userCulture, userRoleIds,
                                    driverLoginProcess.TripNumber, driverLoginProcess.TripSegNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return;
            }
            if (null != tripSegmentMileage)
            {
                log.Debug("SRTEST:GetTripSegmentMileageOpenEndedLast");
                log.DebugFormat("SRTEST:TripNumber:{0} Seg:{1} PrevPowerId:{2} PrevOdomStart:{3} PrevDriverId:{4} PowerId:{5} OdomStart:{6} DriverId:{7}",
                                                    tripSegmentMileage.TripNumber,
                                                    tripSegmentMileage.TripSegNumber,
                                                    tripSegmentMileage.TripSegMileagePowerId,
                                                    tripSegmentMileage.TripSegMileageOdometerStart,
                                                    tripSegmentMileage.TripSegMileageDriverId,
                                                    driverLoginProcess.PowerId,
                                                    driverLoginProcess.Odometer,
                                                    driverLoginProcess.EmployeeId);

                if (driverLoginProcess.PowerId != tripSegmentMileage.TripSegMileagePowerId)
                {
                    tripSegmentMileage.TripSegMileagePowerId = driverLoginProcess.PowerId;
                    tripSegmentMileage.TripSegMileageOdometerStart = driverLoginProcess.Odometer;
                    tripSegmentMileage.TripSegMileageDriverId = driverLoginProcess.EmployeeId;
                    tripSegmentMileage.TripSegMileageDriverName = Common.GetDriverName(employeeMaster);

                    //Update the TripSegmentMileage record
                    scratchChangeSetResult = Common.UpdateTripSegmentMileage(dataService, settings, tripSegmentMileage);
                    log.Debug("SRTEST:Saving TripSegmentMileage Record - Login (Power Unit Change)");
                    if (Common.LogChangeSetFailure(scratchChangeSetResult, tripSegment, log))
                    {
                        var s = string.Format("Could not update TripSegmentMileage for TripNumber {0}-{1} Seq#{2}.",
                                tripSegmentMileage.TripNumber, tripSegmentMileage.TripSegNumber, tripSegmentMileage.TripSegMileageSeqNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Log an entry and add a failure to the changeSetResult if a fault is detected.
        /// </summary>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="fault"></param>
        /// <param name="driverLoginProcess"></param>
        /// <returns>true if a fault is detected</returns>
        private bool handleFault(ChangeSetResult<String> changeSetResult, String msgKey, DataServiceFault fault,
            DriverLoginProcess driverLoginProcess)
        {
            bool faultDetected = false;
            if (null != fault)
            {
                faultDetected = true;
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("Fault occured: {0} during login request: {1}", fault.Message, driverLoginProcess);
            }
            return faultDetected;
        }


        /// <summary>
        /// Insert a PowerHistory record.
        ///  Note:  caller must handle faults.  E.G. if( handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerMaster"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="callCountThisTxn">Start with 1 and incremenet if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        private bool InsertPowerHistory(IDataService dataService, ProcessChangeSetSettings settings,
            PowerMaster powerMaster, EmployeeMaster employeeMaster, int callCountThisTxn,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Note this is a commited snapshot read, a not dirty value, thus we have to keep do our own bookkeeping
            // to support multiple inserts in one txn: callCountThisTxn

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last power history record for this power id to get the last sequence number used
            var powerHistoryMax = Util.Common.GetPowerHistoryLast(dataService, settings, userCulture, userRoleIds,
                                  powerMaster.PowerId, out fault);
            if (null != fault)
            {
                return false;
            }
            int powerSeqNo = callCountThisTxn;
            if (powerHistoryMax != null)
            {
                powerSeqNo = powerHistoryMax.PowerSeqNumber + callCountThisTxn;
            }
            //For testing
            log.Debug("SRTEST:Add PowerHistory");
            log.DebugFormat("SRTEST:Driver:{0} TripNumber:{1} Seg:{2} Status:{3} DateTime:{4} MaxSeq#:{5} callCountThisTxn:{6} Seq#:{7}",
                             powerMaster.PowerDriverId,
                             powerMaster.PowerCurrentTripNumber,
                             powerMaster.PowerCurrentTripSegNumber,
                             powerMaster.PowerStatus,
                             powerMaster.PowerLastActionDateTime,
                             powerHistoryMax.PowerSeqNumber,
                             callCountThisTxn,
                             powerSeqNo);

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TerminalName in the TerminalMaster 
            var powerTerminalMaster = Util.Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          powerMaster.PowerTerminalId, out fault);
            if (null != fault)
            {
                return false;
            }
            string powerTerminalName = powerTerminalMaster?.TerminalName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the RegionName in the RegionMaster 
            var powerRegionMaster = Util.Common.GetRegion(dataService, settings, userCulture, userRoleIds,
                                          powerMaster.PowerRegionId, out fault);
            if (null != fault)
            {
                return false;
            }
            string powerRegionName = powerRegionMaster?.RegionName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the PowerStatus Description in the CodeTable POWERUNITSTATUS 
            var codeTablePowerStatus = Util.Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.PowerUnitStatus, powerMaster.PowerStatus, out fault);
            if (null != fault)
            {
                return false;
            }
            string powerStatusDesc = codeTablePowerStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //If there is a host code, look up the Customer record to get the
            // customer information. 
            //Generally, for logins, this information should be null
            var powerCustomerMaster = new CustomerMaster();
            if (null != powerMaster.PowerCustHostCode)
            {
                powerCustomerMaster = Util.Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                      powerMaster.PowerCustHostCode, out fault);
                if (null != fault)
                {
                    return false;
                }
            }
            //////////////////////////////////////////////////////////////////////////////////////
            //If there is a customer type, look up the description in the CodeTable CUSTOMERTYPE
            //Generally, for logins, this information should be null
            var codeTableCustomerType = new CodeTable();
            if (null != powerMaster.PowerCustType)
            {
                codeTableCustomerType = Util.Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                      CodeTableNameConstants.CustomerType, powerMaster.PowerCustType, out fault);
                if (null != fault)
                {
                    return false;
                }
            }
            string powerCustTypeDesc = codeTableCustomerType?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the Basic Trip Type Description in the TripTypeBasic table 
            var tripTypeBasic = Util.Common.GetTripTypeBasic(dataService, settings, userCulture, userRoleIds,
                                powerMaster.PowerCurrentTripSegType, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripTypeBasicDesc = tripTypeBasic?.TripTypeDesc;

            var powerHistory = new PowerHistory()
            {
                PowerId = powerMaster.PowerId,
                PowerSeqNumber = powerSeqNo,
                PowerType = powerMaster.PowerType,
                PowerDesc = powerMaster.PowerDesc,
                PowerSize = powerMaster.PowerSize,
                PowerLength = powerMaster.PowerLength,
                PowerTareWeight = powerMaster.PowerTareWeight,
                PowerCustType = powerMaster.PowerCustType,
                PowerCustTypeDesc = powerCustTypeDesc,
                PowerTerminalId = powerMaster.PowerTerminalId,
                PowerTerminalName = powerTerminalName,
                PowerRegionId = powerMaster.PowerRegionId,
                PowerRegionName = powerRegionName,
                PowerLocation = powerMaster.PowerLocation,
                PowerStatus = powerMaster.PowerStatus,
                PowerDateOutOfService = powerMaster.PowerDateOutOfService,
                PowerDateInService = powerMaster.PowerDateInService,
                PowerDriverId = powerMaster.PowerDriverId,
                PowerDriverName = Common.GetDriverName(employeeMaster), 
                PowerOdometer = powerMaster.PowerOdometer,
                PowerComments = powerMaster.PowerComments,
                MdtId = powerMaster.MdtId,
                PrimaryPowerType = null,
                PowerCustHostCode = powerMaster.PowerCustHostCode,
                PowerCustName = powerCustomerMaster?.CustName,
                PowerCustAddress1 = powerCustomerMaster?.CustAddress1,
                PowerCustAddress2 = powerCustomerMaster?.CustAddress2,
                PowerCustCity = powerCustomerMaster?.CustCity,
                PowerCustState = powerCustomerMaster?.CustState,
                PowerCustZip = powerCustomerMaster?.CustZip,
                PowerCustCountry = powerCustomerMaster?.CustCountry,
                PowerCustCounty = powerCustomerMaster?.CustCounty,
                PowerCustTownship = powerCustomerMaster?.CustTownship,
                PowerCustPhone1 = powerCustomerMaster?.CustPhone1,
                PowerLastActionDateTime = powerMaster.PowerLastActionDateTime,
                PowerStatusDesc = powerStatusDesc,
                PowerCurrentTripNumber = powerMaster.PowerCurrentTripNumber,
                PowerCurrentTripSegNumber = powerMaster.PowerCurrentTripSegNumber,
                PowerCurrentTripSegType = powerMaster.PowerCurrentTripSegType,
                PowerCurrentTripSegTypeDesc = tripTypeBasicDesc
            };

            // Insert PowerHistory 
            var recordType = (PowerHistoryRecordType) dataService.RecordTypes.Single(x => x.TypeName == "PowerHistory");
            var changeSet = (ChangeSet<string, PowerHistory>) recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, powerHistory, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving PowerHistory - Login");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, powerHistory, log))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Insert a DriverHistory record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="driverStatus"></param>
        /// <param name="callCountThisTxn">Start with 1 and incremenet if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        private static bool InsertDriverHistory(IDataService dataService, ProcessChangeSetSettings settings,
            EmployeeMaster employeeMaster, DriverStatus driverStatus, int callCountThisTxn,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Note this is a commited snapshot read, a not dirty value, thus we have to keep do our own bookkeeping
            // to support multiple inserts in one txn: callCountThisTxn

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last driver history record for this trip and driver to get the last sequence number used
            var driverHistoryMax = Util.Common.GetDriverHistoryLast(dataService, settings, userCulture, userRoleIds,
                                    driverStatus.EmployeeId, driverStatus.TripNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            int driverSeqNo = callCountThisTxn;
            if (driverHistoryMax != null)
            {
                driverSeqNo = driverHistoryMax.DriverSeqNumber + callCountThisTxn;
            }
            //For testing
            log.Debug("SRTEST:Add DriverHistory");
            log.DebugFormat("SRTEST:Driver:{0} TripNumber:{1} Seg:{2} Status:{3} DateTime:{4} MaxSeq#:{5} callCountThisTxn:{6} Seq#:{7}",
                             driverStatus.EmployeeId,
                             driverStatus.TripNumber,
                             driverStatus.TripSegNumber,
                             driverStatus.Status,
                             driverStatus.ActionDateTime,
                             driverHistoryMax.DriverSeqNumber,
                             callCountThisTxn,
                             driverSeqNo);

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the DriverStatus Description in the CodeTable DRIVERSTATUS 
            var codeTableDriverStatus = Util.Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.DriverStatus, driverStatus.Status, out fault);
            if (null != fault)
            {
                return false;
            }
            string driverStatusDesc = codeTableDriverStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TerminalName in the TerminalMaster 
            var driverTerminalMaster = Util.Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          driverStatus.TerminalId, out fault);
            if (null != fault)
            {
                return false;
            }
            string driverTerminalName = driverTerminalMaster?.TerminalName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the RegionName in the RegionMaster 
            var driverRegionMaster = Util.Common.GetRegion(dataService, settings, userCulture, userRoleIds,
                                          driverStatus.RegionId, out fault);
            if (null != fault)
            {
                return false;
            }
            string driverRegionName = driverRegionMaster?.RegionName;

            //////////////////////////////////////////////////////////////////////////////////////
            //If there is a trip number and segment number, look up the TripSegment record to get the
            //destination customer information. (Instead of the cutomer master).
            //Unless the driver is logging in in the middle of the trip, we want this to be null
            var driverTripSegment = new TripSegment();
            if (null != driverStatus.TripNumber && null != driverStatus.TripSegNumber)
            {
                driverTripSegment = Util.Common.GetTripSegment(dataService, settings, userCulture, userRoleIds,
                                              driverStatus.TripNumber, driverStatus.TripSegNumber, out fault);
                if (null != fault)
                {
                    return false;
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////
            //if there is no trip number, construct a number to use that consists of the driver status + driverid
            string tripNumber = driverStatus.TripNumber;
            if (null == driverStatus.TripNumber)
            {
                tripNumber = driverStatus.Status + driverStatus.EmployeeId;
            }

            //////////////////////////////////////////////////////////////////////////////////////
            var driverHistory = new DriverHistory()
            {
                EmployeeId = driverStatus.EmployeeId,
                TripNumber = tripNumber,
                DriverSeqNumber = driverSeqNo,
                TripSegNumber = driverStatus.TripSegNumber,
                // TripSegType = tripSegment.TripSegType,
                // TripSegTypeDesc  = tripSegment.TripSegTypeDesc,
                // TripAssignStatus  = trip.TripAssignStatus,
                // TripAssignStatusDesc  = trip.TripAssignStatusDedsc,
                // TripStatus  = trip.TripStatus,
                // TripStatusDesc  = trip.TripStatusDesc,
                // TripSegStatus  = tripSegment.TripSegStatus,
                // TripSegStatusDesc = tripSegment.TripSegStatusDesc,
                DriverStatus = driverStatus.Status,
                DriverStatusDesc = driverStatusDesc,
                DriverName = Common.GetDriverName(employeeMaster),
                TerminalId = driverStatus.TerminalId,
                TerminalName = driverTerminalName,
                RegionId = driverStatus.RegionId,
                RegionName = driverRegionName,
                PowerId = driverStatus.PowerId,
                // DriverArea =,
                MDTId = driverStatus.MDTId,
                LoginDateTime = driverStatus.LoginDateTime,
                ActionDateTime = driverStatus.ActionDateTime,
                DriverCumMinutes = driverStatus.DriverCumMinutes,
                Odometer = driverStatus.Odometer,
                DestCustType = driverTripSegment.TripSegDestCustType,
                DestCustTypeDesc = driverTripSegment.TripSegDestCustTypeDesc,
                DestCustHostCode = driverTripSegment.TripSegDestCustHostCode,
                DestCustName = driverTripSegment.TripSegDestCustName,
                DestCustAddress1 = driverTripSegment.TripSegDestCustAddress1,
                DestCustAddress2 = driverTripSegment.TripSegDestCustAddress2,
                DestCustCity = driverTripSegment.TripSegDestCustCity,
                DestCustState = driverTripSegment.TripSegDestCustState,
                DestCustZip = driverTripSegment.TripSegDestCustZip,
                DestCustCountry = driverTripSegment.TripSegDestCustCountry,
                GPSAutoGeneratedFlag = driverStatus.GPSAutoGeneratedFlag,
                GPSXmitFlag = driverStatus.GPSXmitFlag,
                MdtVersion = driverStatus.MdtVersion
            };

            //////////////////////////////////////////////////////////////////////////////////////
            // Insert DriverHistory 
            var recordType = (DriverHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverHistory");
            var changeSet = (ChangeSet<string, DriverHistory>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, driverHistory, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving DriverHistory Record - Login");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, driverHistory, log))
            {
                return false;
            }
            return true;
        }

    }
}