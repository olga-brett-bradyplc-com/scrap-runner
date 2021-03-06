﻿using AutoMapper;
using System;
using System.Collections.Generic;
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
using Brady.ScrapRunner.DataService.RecordTypes;
using System.IO;
using log4net;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{

    /// <summary>
    /// The process for logging in a driver.  Call this process "withoutrequery".
    /// </summary>
    /// The mobile device will also call these processes:
    ///    ContainerChangeProcess for container master updates.
    ///    TerminalChangeProcess for terminal master updates.
    ///    CommodityMasterProcess for universal commodities.
    ///    PreferencesProcess for driver preferences.
    ///    CodeTableProcess for code table values.
    ///    TripInfoProcess for trips dispatched to the driver.
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
        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverLoginProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
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
                        ChangeSet<string, DriverLoginProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver login processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverLoginProcess> changeSet, ProcessChangeSetSettings settings)
        {
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository) repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security checks, etc.  Note however, we will lose non-persisted user input values which 
            // will not be propagated through into the changeSetResult.

            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.   Note we only log in one person at a time but in the more 
            // general cases we could be processing multiple incoming records.
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

                // This is coded as a loop but there will only be one login at a time.
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    string msgKey = key;

                    DriverLoginProcess driverLoginProcess = (DriverLoginProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // In our case we are doing all the processing rather than a bit of light post processing.
                    // So we want all initial values passed in by the driver.  Thus we grab the DriverLoginProcess
                    // record from the submitted ChangeSet rather than the one from the ChangeSetResult.
                    //DriverLoginProcess driverLoginProcess = (DriverLoginProcess) changeSetResult.GetSuccessfulUpdateForId(key);
                    DriverLoginProcess backfillDriverLoginProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverLoginProcess))
                    {
                        Mapper.Map(backfillDriverLoginProcess, driverLoginProcess);
                    }
                    else
                    {
                        var str = "Unable to process login for Driver ID.  Cannot find base request object for " +
                                  driverLoginProcess.EmployeeId;
                        log.Error(str);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(str));
                        break;
                    }

                    try
                    {
                        DataServiceFault fault;
                        ChangeSetResult<string> scratchChangeSetResult;

                        int powerHistoryInsertCount = 0;
                        int driverHistoryInsertCount = 0;

                        //TripInfoProcess will return the following lists.
                        List<ContainerMaster> containersOnPowerIdList = new List<ContainerMaster>();
                        List<EmployeeMaster> usersForMessagingList = new List<EmployeeMaster>();

                        ////////////////////////////////////////////////
                        //DriverEnrouteProcess has been called
                        log.DebugFormat("SRTEST:DriverLoginProcess Called by {0}", key);
                        log.DebugFormat("SRTEST:DriverLoginProcess Driver:{0} DT:{1} PowerId:{2} Odom:{3} MDT:{4}",
                                         driverLoginProcess.EmployeeId, driverLoginProcess.LoginDateTime,
                                         driverLoginProcess.PowerId, driverLoginProcess.Odometer,
                                         driverLoginProcess.Mdtid);
                        ////////////////////////////////////////////////
                        // Validate driver id / Get the EmployeeMaster
                        // UserId must be a valid EmployeeId in the EmployeeMaster and SecurityLevel must be DR for driver.
                        var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                driverLoginProcess.EmployeeId, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        if (null == employeeMaster)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + driverLoginProcess.EmployeeId));
                            break;
                        }
                        driverLoginProcess.TermId = employeeMaster.TerminalId;
                        driverLoginProcess.RegionId = employeeMaster.RegionId;
                        driverLoginProcess.AreaId = employeeMaster.AreaId;

                        ////////////////////////////////////////////////
                        // Preferences:  Lookup the system preference "DEFAllowAnyPowerUnit".  
                        // If preference prefAllowAnyPowerUnit is set to Y, then allow any power unit to be valid
                        // Otherwise the power unit's region must match the driver's region. This is the norm.
                        string prefAllowAnyPowerUnit = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                       Constants.SystemTerminalId, PrefSystemConstants.DEFAllowAnyPowerUnit, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Get the PowerMaster record
                        // PowerId must be a valid PowerId in the PowerMaster 
                        PowerMaster powerMaster;
                        if (prefAllowAnyPowerUnit == Constants.Yes)
                        {
                            powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.PowerId, out fault);
                        }
                        else
                        {
                            powerMaster = Common.GetPowerUnitForRegion(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.PowerId, employeeMaster.RegionId, out fault);
                        }
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        if (null == powerMaster)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Power ID " + driverLoginProcess.PowerId));
                            break;
                        }
                        ////////////////////////////////////////////////
                        // Check power unit status: Scheduled for the shop? PS_SHOP = "S"
                        if (PowerStatusConstants.Shop == powerMaster.PowerStatus)
                        {
                            var s = string.Format("Do not use Power unit {0}.  It is scheduled for the shop. ", driverLoginProcess.PowerId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Is unit in use by another driver? PS_INUSE = "I"
                        if (powerMaster.PowerStatus == PowerStatusConstants.InUse &&
                            powerMaster.PowerDriverId != employeeMaster.EmployeeId)
                        {
                            var s = string.Format("Power unit {0} in use by another driver.  Change power unit number or call Dispatch.",
                                driverLoginProcess.PowerId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // If no odometer record for this unit, accept as sent by the handheld.  
                        // If overrride flag = "Y", accept as sent by the handheld.  
                        // Otherwise lookup the preference to determine the acceptable range of values for the tolerance check.
                        // If the override flag is not set, and the odometer is within the range preference then accept the odometer 
                        if (driverLoginProcess.OverrideFlag != Constants.Yes && powerMaster.PowerOdometer != null)
                        {
                            //Get the preference DEFOdomWarnRange which is an integer representing the + or - range, typically 5 
                            string prefOdomWarnRange = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                        employeeMaster.TerminalId, PrefDriverConstants.DEFOdomWarnRange, out fault);
                            if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                            {
                                break;
                            }
                            if (null != prefOdomWarnRange)
                            {

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
                        //Do not use the MDTId from the mobile app. Build it using the MDT Prefix (if it exists) plus the employee id.
                        // Lookup Preference: DEFMDTPrefix
                        string prefMdtPrefix = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                        Constants.SystemTerminalId, PrefSystemConstants.DEFMDTPrefix, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        driverLoginProcess.Mdtid = prefMdtPrefix + driverLoginProcess.EmployeeId;

                        ////////////////////////////////////////////////
                        //Get the driver status record for the driver. If one does not exist (which is common), add one.
                        var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                    driverLoginProcess.EmployeeId, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
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

                        ////////////////////////////////////////////////
                        // Validate Duplicate login?  Assuming we don't need this at this time.
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
                        //Determine current trip number, segment, and driver status
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
                                var nextTripSegment = Common.GetNextIncompleteTripSegment(dataService, settings, userCulture, userRoleIds,
                                                              driverStatus.TripNumber, out fault);
                                if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                                {
                                    break;
                                }
                                driverLoginProcess.TripSegNumber = nextTripSegment.TripSegNumber;
                                //We can't leave the status as done. 
                                //No need to do this after all, since we will be removing the driverstatus.
                                //driverLoginProcess.DriverStatus = DriverStatusSRConstants.Available;

                          }
                            //For all other statuses just use the current seg number in the driver status table.
                            else
                            {
                                driverLoginProcess.TripSegNumber = driverStatus.TripSegNumber;
                            }

                            //Remove the driver status except for enroutes,arrives, and state line crossings
                            //Other statuses that the driver might have been in (delay, back on duty, fuel, done) don't matter anymore.
                            if (DriverStatusSRConstants.Enroute != driverStatus.Status &&
                                DriverStatusSRConstants.Arrive != driverStatus.Status &&
                                DriverStatusSRConstants.StateLine != driverStatus.Status)
                            {
                                driverLoginProcess.DriverStatus = null;
                            }

                            //I think this covers the case where the driver might have been enroute, but then disconnected.
                            //The enroute is stored in the previous driver status, while disconnect is now the driver status.
                            //The enroute is the one we want to use.
                            //If there is a previous driver status, use it. Otherwise use driver status.
                            driverLoginProcess.DriverStatus = driverStatus.PrevDriverStatus ?? driverStatus.Status;

                            ////////////////////////////////////////////////
                            //Update Trip Record: Set the "trip in progress" flag in the trip table
                            if (driverStatus.TripNumber != null &&
                                driverStatus.TripSegNumber == Constants.FirstSegment)
                            {
                                if (DriverStatusSRConstants.Enroute != driverStatus.Status &&
                                    DriverStatusSRConstants.Arrive != driverStatus.Status &&
                                    DriverStatusSRConstants.StateLine != driverStatus.Status &&
                                    DriverStatusSRConstants.Done != driverStatus.Status &&
                                    DriverStatusSRConstants.Delay != driverStatus.Status &&
                                    DriverStatusSRConstants.BackOnDuty != driverStatus.Status &&
                                    DriverStatusSRConstants.Fuel != driverStatus.Status)
                                {
                                    // Update Trip in progress flag in the trip table to 'N'
                                    var trip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                                driverStatus.TripNumber, out fault);
                                    if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                                    {
                                        break;
                                    }

                                    //Set to N only if it is not N already
                                    if (trip.TripInProgressFlag != Constants.No)
                                    {
                                        trip.TripInProgressFlag = Constants.No;

                                        // Use scratchChangeSetResult so information can still be returned to the mobile app
                                        scratchChangeSetResult = Common.UpdateTrip(dataService, settings, trip);
                                        log.Debug("SRTEST:Saving Trip Record - TripInProgressFlag");
                                        if (Common.LogChangeSetFailure(scratchChangeSetResult, trip, log))
                                        {
                                            var s = string.Format("DriverLoginProcess:Could not update Trip for TripInProgressFlag: {0}.", Constants.No);
                                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                            break;
                                        }
                                    }
                                }
                            }//end of Update Trip in progress flag in the trip table

                        }//end of else if (null == driverStatus.TripNumber)

                        
                        ////////////////////////////////////////////////
                        // Check for power unit change
                        // There has to be a trip number and trip segment number...
                        if (null != driverLoginProcess.TripNumber && null != driverLoginProcess.TripSegNumber)
                        {
                            CheckForPowerIdChange(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                                driverLoginProcess, employeeMaster);
                        }

                        ////////////////////////////////////////////////
                        // Get the Trip record
                        var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.TripNumber, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        if (null == currentTrip)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverLoginProcess:Invalid TripNumber: " 
                                           + driverLoginProcess.TripNumber));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Get the TripSegment record
                        var currentTripSegment = Common.GetTripSegment(dataService, settings, userCulture, userRoleIds,
                                                      driverLoginProcess.TripNumber, driverLoginProcess.TripSegNumber,out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        if (null == currentTripSegment)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverLoginProcess:Invalid TripNumber: "
                                + driverLoginProcess.TripNumber + "-" + driverLoginProcess.TripSegNumber));
                            break;
                        }
                        ////////////////////////////////////////////////
                        // Get the Customer record for the destination cust host code
                        var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                                 currentTripSegment.TripSegDestCustHostCode, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        if (null == destCustomerMaster)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverLoginProcess:Invalid CustHostCode: "
                                            + currentTripSegment.TripSegDestCustHostCode));
                            break;
                        }
                        ////////////////////////////////////////////////
                        //Check for open ended delays 
                        //CheckForOpenEndedDelays(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                        //             driverLoginProcess, employeeMaster, driverStatus, ref driverHistoryInsertCount);

                        if (CheckForOpenEndedDelays(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                     driverLoginProcess))
                        {
                            // Add "Back On Duty" flag to Driver History table.
                            // For the last open-ended delay that we added an end time, record a back on duty status in the DriverHistory table.
                            // The driver is logging in, so in effect he is back on duty, no longer on delay.
                            // No need to actually add it to the DriverStatus table since we will be adding a
                            // status of login very shortly and he did not actually click on back on duty. 
                            // Users viewing the driver history wanted to know where the back on duty was.
                            // We still need to do this even if there was no trip number because the driver can go on delay without being on a trip. 
                            // In that case the trip number in the DriverDelay table is #driverid
                            driverStatus.Status = DriverStatusSRConstants.BackOnDuty;
                            //Add 1 second to the action date time, so the BackOnDuty will display after the Login on the Driver History report
                            var tSpan = new TimeSpan(0, 0, 0, 1);
                            driverStatus.ActionDateTime = driverLoginProcess.LoginDateTime + tSpan;
                            //The delay code is stored in the DriverStatus table but not in the DriverHistory table, so it is not needed.
                            log.Debug("SRTEST:Add BackOnDuty to DriverHistory");

                            if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                                ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                            {
                                if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                                {
                                    break;
                                }
                                var s = "DriverLoginProcess:Could not update driver status";
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }

                        ////////////////////////////////////////////////
                        //Update the DriverStatus table. 
                        //These fields are not affected by the login process:
                        //     DriverCumMinutes, GPSAutoGeneratedFlag, DelayCode, PrevDriverStatus, GPSXmitFlag, SendHHLogoffFlag
                        //This field is used by router and is no longer needed: RouteTo

                        // Set the new values in the driverstatus table:
                        driverStatus.Status = DriverStatusSRConstants.LoggedIn;
                        driverStatus.TripNumber = driverLoginProcess.TripNumber;
                        driverStatus.TripSegNumber = driverLoginProcess.TripSegNumber;
                        driverStatus.TripStatus = currentTrip.TripStatus;
                        driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                        driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                        driverStatus.TripSegType = currentTripSegment.TripSegType;
                        driverStatus.TerminalId = employeeMaster.TerminalId;
                        driverStatus.RegionId = employeeMaster.RegionId;
                        driverStatus.PowerId = driverLoginProcess.PowerId;
                        driverStatus.MDTId = driverLoginProcess.Mdtid;
                        driverStatus.ActionDateTime = driverLoginProcess.LoginDateTime;
                        driverStatus.LoginDateTime = driverLoginProcess.LoginDateTime;
                        driverStatus.Odometer = driverLoginProcess.Odometer;
                        driverStatus.LoginProcessedDateTime = DateTime.Now;
                        driverStatus.MdtVersion = driverLoginProcess.PndVer;
                        driverStatus.DriverLCID = driverLoginProcess.LocaleCode;
                        driverStatus.ServicesFlag = Constants.Yes;

                        //These will be updated by different processes.
                        // driverStatus.ContainerMasterDateTime = driverLoginProcess.ContainerMasterDateTime;
                        // driverStatus.TerminalMasterDateTime = driverLoginProcess.TerminalMasterDateTime;

                        ////////////////////////////////////////////////
                        //Calculate driver's cumulative time which is the sum of standard drive and stop minutes
                        //for all incomplete trip segments 
                        var incompleteTripSegList = Common.GetTripSegmentsIncompleteForDriver(dataService, settings, userCulture, userRoleIds,
                                          driverLoginProcess.EmployeeId, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        if (incompleteTripSegList != null)
                        {
                            driverStatus.DriverCumMinutes = Common.GetDriverCumulativeTime(incompleteTripSegList,null);
                        }
                        else
                        {
                            driverStatus.DriverCumMinutes = 0;
                        }

                        //Do the update
                        // Use scratchChangeSetResult so information can still be returned to the mobile app
                        scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                        log.Debug("SRTEST:Saving DriverStatus Record - Login");
                        if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                        {
                            var s = string.Format("DriverLoginProcess:Could not update DriverStatus for Driver {0}.",
                                driverLoginProcess.EmployeeId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Add record to DriverHistory table
                        if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                                ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                        {
                            if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                            {
                                break;
                            }
                            changeSetResult.FailedUpdates.Add(msgKey,
                                new MessageSet(string.Format("DriverLoginProcess:Could not insert Driver History for Driver {0}.",
                                    driverLoginProcess.EmployeeId)));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Update PowerMaster table
                        powerMaster.PowerStatus = PowerStatusConstants.InUse;
                        powerMaster.PowerDriverId = driverLoginProcess.EmployeeId;
                        powerMaster.PowerOdometer = driverLoginProcess.Odometer;
                        powerMaster.PowerLastActionDateTime = driverLoginProcess.LoginDateTime;

                        // Trip Number and segment number are not populated at login. Only when driver goes enroute.
                        // In fact, we need to remove this information if it exists.
                        powerMaster.PowerCurrentTripNumber = null;
                        powerMaster.PowerCurrentTripSegNumber = null;
                        powerMaster.PowerCurrentTripSegType = null;

                        // Use scratchChangeSetResult so information can still be returned to the mobile app
                        scratchChangeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                        log.Debug("SRTEST:Saving PowerMaster Record - Login");
                        if (Common.LogChangeSetFailure(scratchChangeSetResult, powerMaster, log))
                        {
                            var s = string.Format("DriverLoginProcess:Could not update PowerMaster for PowerId:{0}.",
                                driverLoginProcess.PowerId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Add record to PowerHistory table
                        if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, destCustomerMaster,
                                ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                        {
                            if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                            {
                                break;
                            }
                            changeSetResult.FailedUpdates.Add(msgKey,
                                new MessageSet(string.Format("DriverLoginProcess:Could not insert Power History for Power Unit {0}.",
                                    driverLoginProcess.PowerId)));
                            break;
                        }


                        ////////////////////////////////////////////////
                        // Send start-up information to tracker
                        if(!SendToTracker(dataService, settings, changeSetResult,msgKey,userRoleIds,userCulture,
                                          driverLoginProcess, employeeMaster))
                        {
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Add entry to Event Log - Login Received.
                        StringBuilder sbComment = new StringBuilder();
                        sbComment.Append(EventCommentConstants.ReceivedDriverLogin);
                        sbComment.Append(" HH:");
                        sbComment.Append(driverLoginProcess.LoginDateTime);
                        if (null != driverLoginProcess.TripNumber)
                        { 
                            sbComment.Append(" Trip:");
                            sbComment.Append(driverLoginProcess.TripNumber);
                            sbComment.Append("-");
                            sbComment.Append(driverLoginProcess.TripSegNumber);
                        }
                        sbComment.Append(" Drv:");
                        sbComment.Append(driverLoginProcess.EmployeeId);
                        sbComment.Append(" Pwr:");
                        sbComment.Append(driverLoginProcess.PowerId);
                        sbComment.Append(" Odom:");
                        sbComment.Append(driverLoginProcess.Odometer);
                        string comment = sbComment.ToString().Trim();

                        var eventLog = new EventLog()
                        {
                            EventDateTime = driverLoginProcess.LoginDateTime,
                            EventSeqNo = 0,
                            EventTerminalId = employeeMaster.TerminalId,
                            EventRegionId = employeeMaster.RegionId,
                            //These are not populated in the current system.
                            // EventEmployeeId = driverLoginProcess.EmployeeId,
                            // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                            EventTripNumber = driverLoginProcess.TripNumber,
                            EventProgram = EventProgramConstants.Services,
                            //These are not populated in the current system.
                            //EventScreen = null,
                            //EventAction = null,
                            EventComment = comment,
                        };

                        ChangeSetResult<int> eventChangeSetResult;
                        eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                        log.Debug("SRTEST:Saving EventLog Record - Login");
                        //Check for EventLog failure.
                        if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                        {
                            var s = string.Format("DriverLoginProcess:Could not update EventLog for Driver {0} {1}.",
                                    driverLoginProcess.EmployeeId, EventCommentConstants.ReceivedDriverLogin);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Send Container Inventory
                        //Get a list of container on the power id
                        var containersOnPowerId = Common.GetContainersForPowerId(dataService, settings, userCulture, userRoleIds,
                                                  driverLoginProcess.PowerId, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        //This will be sent back to the driver
                        containersOnPowerIdList.AddRange(containersOnPowerId);

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
                        // Send dispatcher list for messaging
                        // Check system preference: "DEFSendDispatchersForArea".  
                        // If preference DEFSendDispatchersForArea is set to Y, then send a list of dispatchers whose terminal id is in the driver's area. 
                        // Otherwise send a list of dispatchers whose region matches the driver's region.
                        string prefSendDispatchersForArea = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                       employeeMaster.TerminalId, PrefDriverConstants.DEFSendDispatchersForArea, out fault);
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }

                        //Get a list of users that can access messaging. This isn't just dispatchers.
                        List<EmployeeMaster> usersForMessaging;
                        if (prefSendDispatchersForArea == Constants.Yes)
                        {
                            //Get just the users whose default terminal id is in the driver's area.
                            usersForMessaging = Common.GetDispatcherListForArea(dataService, settings, userCulture, userRoleIds,
                                                       employeeMaster.AreaId, out fault);
                        }
                        else
                        {
                            //Get just the users whose region id is the same as the driver's region.
                            usersForMessaging = Common.GetDispatcherListForRegion(dataService, settings, userCulture, userRoleIds,
                                                       employeeMaster.RegionId, out fault);
                        }
                        if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        //This will be sent back to the driver
                        usersForMessagingList.AddRange(usersForMessaging);

                        //For testing
                        log.DebugFormat("SRTEST:GetDispatcherList: Sending:{0} Dispatchers", usersForMessaging.Count);

                        ////////////////////////////////////////////////
                        // Populate and send the Login message back to the mobile device.
                        driverLoginProcess.UsersForMessaging = usersForMessagingList;
                        driverLoginProcess.ContainersOnPowerId = containersOnPowerIdList;

                        //For testing... this is information filled in by this driverLoginProcess and should be sent back to the mobile device.
                        log.DebugFormat("SRTEST:Driver:{0} TripNumber:{1}-{2} DriverStatus:{3} Region:{4} Terminal:{5} Area:{6}",
                                         driverLoginProcess.EmployeeId,
                                         driverLoginProcess.TripNumber,
                                         driverLoginProcess.TripSegNumber,
                                         driverLoginProcess.DriverStatus,
                                         driverLoginProcess.RegionId,
                                         driverLoginProcess.TermId,
                                         driverLoginProcess.AreaId);

                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Unhandled general exception: {0} within driver login: {1}.", ex, driverLoginProcess);
                        changeSetResult.FailedUpdates.Add(msgKey,
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
                // Capture details of outgoing response too and log at INFO level
                log.Info(RequestResponseUtil.CaptureResponse(changeSetResult, requestRespStrBld));
                return changeSetResult;
            }

            if (changeSetResult.FailedCreates.Any() || changeSetResult.FailedUpdates.Any() ||
                changeSetResult.FailedDeletions.Any())
            {
                transaction.Rollback();
                log.Debug("SRTEST:Transaction Rollback - DriverLoginProcess");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - DriverLoginProcess");
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
        /// Check for open ended delays for this driver
        /// </summary>
        /// 
        private bool CheckForOpenEndedDelays(IDataService dataService, ProcessChangeSetSettings settings,
               ChangeSetResult<string> changeSetResult, String msgKey, IEnumerable<long> userRoleIds, string userCulture,
               DriverLoginProcess driverLoginProcess)
        {
            DataServiceFault fault;
            var driverDelays = Common.GetDriverDelaysOpenEnded(dataService, settings, userCulture, userRoleIds,
                                             driverLoginProcess.EmployeeId, out fault);
            if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
            {
                return false;
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
                var tSpan = new TimeSpan(0, 0, 0, 1);
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
                    var s = "DriverLoginProcess:Could not update open ended driver delays";
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
                return true;
            }
            //None found
            return false;
        }

        /// <summary>
        /// Check for power unit change in the middle of a segment.
        /// If trip segment is partially completed & power unit id from the login is different that the
        /// power unit in the trip segment table, then we want to use the new power unit
        /// Also check for an open ended entry in the TripSegmentMileage table.
        /// There should only be one, but there may be more.
        /// Here I think, we just want to get the last one, since our purpose here is to adjust the power id
        /// not to close open-ended entries.
        /// </summary>
        private void CheckForPowerIdChange(IDataService dataService, ProcessChangeSetSettings settings,
                   ChangeSetResult<string> changeSetResult,String msgKey, long[] userRoleIds, string userCulture,
                   DriverLoginProcess driverLoginProcess, EmployeeMaster employeeMaster)
        {
            ChangeSetResult<string> scratchChangeSetResult;
            DataServiceFault fault;
            //Get the current trip segment if open-ended
            var tripSegment = Common.GetTripSegmentOpenEnded(dataService, settings, userCulture, userRoleIds,
                                    driverLoginProcess.TripNumber, driverLoginProcess.TripSegNumber, out fault);
            if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
            {
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
                        var s = string.Format("DriverLoginProcess:Could not update TripSegment for TripNumber {0}-{1}.",
                                               tripSegment.TripNumber, tripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return;
                    }
                }
            }
            //Get the last open-ended trip segment mileage 
            var tripSegmentMileage = Common.GetTripSegmentMileageOpenEndedLast(dataService, settings, userCulture, userRoleIds,
                                    driverLoginProcess.TripNumber, driverLoginProcess.TripSegNumber, out fault);
            if (FaultHandled(changeSetResult, msgKey, fault, driverLoginProcess))
            {
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
                    tripSegmentMileage.TripSegMileageDriverName = Common.GetEmployeeName(employeeMaster);

                    //Update the TripSegmentMileage record
                    scratchChangeSetResult = Common.UpdateTripSegmentMileage(dataService, settings, tripSegmentMileage);
                    log.Debug("SRTEST:Saving TripSegmentMileage Record - Login (Power Unit Change)");
                    if (Common.LogChangeSetFailure(scratchChangeSetResult, tripSegment, log))
                    {
                        var s = string.Format("DriverLoginProcess:Could not update TripSegmentMileage for TripNumber {0}-{1} Seq#{2}.",
                                tripSegmentMileage.TripNumber, tripSegmentMileage.TripSegNumber, tripSegmentMileage.TripSegMileageSeqNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
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
        private bool FaultHandled(ChangeSetResult<String> changeSetResult, String msgKey, DataServiceFault fault,
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
        /// Send message to tracker.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverLoginProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <returns></returns>
        private bool SendToTracker(IDataService dataService, ProcessChangeSetSettings settings,
                   ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture, 
                   DriverLoginProcess driverLoginProcess, EmployeeMaster employeeMaster)
        {
            DataServiceFault fault;

            ////////////////////////////////////////////////////////
            // Lookup Preference: DEFRouterPath
            string prefRouterPath = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                          Constants.SystemTerminalId, PrefSystemConstants.DEFRouterPath, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("Fault occured: {0} during login request: {1}", fault.Message, driverLoginProcess);
                return false;
            }
            ////////////////////////////////////////////////////////
            // Lookup Preference: DEFMDTPrefix
            string prefMdtPrefix = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                          Constants.SystemTerminalId, PrefSystemConstants.DEFMDTPrefix, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            //Router path must be specified in system preference or we cannot send the packet.
            if (prefRouterPath != null)
            {
                //Build the mdtId
                string mdtId = driverLoginProcess.EmployeeId;
                if (!string.IsNullOrEmpty(prefMdtPrefix))
                {
                    mdtId = prefMdtPrefix + mdtId;
                }

                // Build the complete path.
                if (0 != prefRouterPath[prefRouterPath.Length - 1].CompareTo('\\'))
                {
                    prefRouterPath += @"\";
                }
                string fullRouterPathFileName = prefRouterPath + Constants.Send + @"\" + Constants.GPSFileName;

                try
                {
                    // Write the lines to a new file or append to an existing file (true does this).
                    using (StreamWriter outputFile = new StreamWriter(fullRouterPathFileName + Constants.GPSFileExt, true))
                    {
                        // Send GPS Company Info to tracker
                        //*T TO TRACKER FR MdtId
                        //*DC:2,GA
                        //*DI:2,1
                        //*DC:7,TerminalId
                        //*DC:11,RegionId
                        //*END
                        outputFile.WriteLine($"*T TO {Constants.Tracker} FR {mdtId}");
                        outputFile.WriteLine($"*DC:2,GA");
                        outputFile.WriteLine($"*DI:2,1");
                        outputFile.WriteLine($"*DC:7,{driverLoginProcess.TermId}");
                        outputFile.WriteLine($"*DC:11,{driverLoginProcess.RegionId}");
                        outputFile.WriteLine($"*END");

                        // Send GV Driver START Packet to tracker
                        //*T TO TRACKER FR MdtId
                        //*DS:3,GV 
                        //*DC:5,START
                        //*END
                        outputFile.WriteLine($"*T TO {Constants.Tracker} FR {mdtId}");
                        outputFile.WriteLine($"*DS:3,GV");
                        outputFile.WriteLine($"*DC:5,START");
                        outputFile.WriteLine($"*END");

                        // Send GV Driver CODE Packet to tracker
                        //*T TO TRACKER FR MdtId
                        //*DS:3,GV 
                        //*DS:5,CODE 
                        //*DC:10,TerminalId
                        //*END
                        outputFile.WriteLine($"*T TO {Constants.Tracker} FR {mdtId}");
                        outputFile.WriteLine($"*DS:3,GV");
                        outputFile.WriteLine($"*DS:5,CODE");
                        outputFile.WriteLine($"*DC:10,{driverLoginProcess.TermId}");
                        outputFile.WriteLine($"*END");

                        // Send GV Driver NAME Packet to tracker
                        //*T TO TRACKER FR MdtId
                        //*DS:3,GV 
                        //*DS:5,NAME 
                        //*DC:41,LastName,FirstName
                        //*END
                        string driverName = Common.GetEmployeeName(employeeMaster);
                        outputFile.WriteLine($"*T TO {Constants.Tracker} FR {mdtId}");
                        outputFile.WriteLine($"*DS:3,GV");
                        outputFile.WriteLine($"*DS:5,NAME");
                        outputFile.WriteLine($"*DC:41,{driverName}");
                        outputFile.WriteLine($"*END");

                    }
                    //If the file GPSScrapPkt does not exist, the rename the GPSScrapPkt.x to GPSScrapPkt (no extension)
                    if (!File.Exists(fullRouterPathFileName))
                    {
                        if (File.Exists(fullRouterPathFileName + Constants.GPSFileExt))
                        {
                            File.Move(fullRouterPathFileName + Constants.GPSFileExt, fullRouterPathFileName);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.DebugFormat("SRTEST:DriverGPSLocationProcess Write failed: {0}.", e.Message);
                }

            }//if (prefRouterPath != null)
            else
            {
                log.DebugFormat("SRTEST:DriverGPSLocationProcess Router Path no set up.");
            }
            return true;
        }

    }
}