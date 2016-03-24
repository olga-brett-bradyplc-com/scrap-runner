using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Brady.ScrapRunner.DataService.Interfaces;
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

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    /// <summary>
    /// The process for logging in a driver.
    /// </summary>
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
            ChangeSetResult<string> scratchChangeSetResult;

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

                        // TODO: determine these on a case by case basis.
                        string userCulture = "en-GB";
                        IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                        // It appears in the general case I must backfill non-persisted user input values from the initial change set.
                        // They will not be propagated through into the changeSetResult of base base.ProcessChangeSet() method.
                        DriverLoginProcess backfillDriverLoginProcess;
                        if (changeSet.Update.TryGetValue(key, out backfillDriverLoginProcess))
                        {
                            // Use a mapper?  This might not always be the best approach.  
                            // But these are static, can I set this to map a subset?
                            Mapper.Map(backfillDriverLoginProcess, driverLoginProcess);

                            // OR do I just simply bsckfill the minimalist set?
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

                        // 2) Validate driver id:  Get the EmployeeMaster - an inefficient, client side filtering sample
                        // Query query = new Query{CurrentQuery = "EmployeeMasters"};
                        // var employeeMasters = dataService.Query(query, settings.Username, Enumerable.Empty<long>(), "en-GB", settings.Token, out fault);
                        // var employeeMaster = employeeMasters.Records.Cast<EmployeeMaster>().Where(x => x.EmployeeId == driverLoginProcess.EmployeeId).First();
                        //
                        // 2) Validate driver id:  Get the EmployeeMaster
                        //

                        Query query = new Query
                        {
                            //"EmployeeMasters?$filter= EmployeeId='{0}'", driverLoginProcess.EmployeeId
                            CurrentQuery = new QueryBuilder<EmployeeMaster>().Filter(t => 
                                t.Property(p => p.EmployeeId).EqualTo(driverLoginProcess.EmployeeId))
                                .GetQuery()
                        };
                        var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                            settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        var employeeMaster = (EmployeeMaster) queryResult.Records.Cast<EmployeeMaster>().FirstOrNull();
                        if (employeeMaster == null)
                        {
                            var s = "Invalid Driver ID " + driverLoginProcess.EmployeeId;
                            log.Error(s);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        //
                        // 3) Lookup preferences.  
                        // Not implemented see PreferencesProcess.
                        //

                        //
                        // 4a) Validate PowerId
                        //
                        query = new Query
                        {
                            //"PowerMasters?$filter= PowerId='{0}'", driverLoginProcess.PowerId
                            CurrentQuery = new QueryBuilder<PowerMaster>().Filter(t =>
                                t.Property(p => p.PowerId).EqualTo(driverLoginProcess.PowerId))
                                 .GetQuery()
                        };
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                            settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        var powerMaster = (PowerMaster) queryResult.Records.Cast<PowerMaster>().FirstOrNull();
                        if (powerMaster == null)
                        {
                            var s = "Invalid Power ID " + driverLoginProcess.PowerId;
                            log.Error(s);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        // 4b) Check system pref "DEFAllowAnyPowerUnit".  If not found or "N" then check company ownership.
                        query = new Query
                        {
                            //"Preferences?$filter= TerminalId='0000' and Parameter='DEFAllowAnyPowerUnit'"
                            CurrentQuery = new QueryBuilder<Preference>().Filter(t =>
                                t.Property(p => p.TerminalId).EqualTo("0000").And()
                                 .Property(p => p.Parameter).EqualTo("DEFAllowAnyPowerUnit"))
                                 .GetQuery()
                        };
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                            settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        var preference = (Preference) queryResult.Records.Cast<Preference>().FirstOrNull();
                        if (preference == null || preference.ParameterValue == Constants.No)
                        {
                            // TODO:  Can it be and what if employeeMaster.RegionId is null? 
                            if (powerMaster.PowerRegionId != null &&
                                powerMaster.PowerRegionId != employeeMaster.RegionId)
                            {
                                var s = "Invalid Power ID " + driverLoginProcess.PowerId;
                                log.Error(s);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }

                        // 4c) Check power unit status: Scheduled for the shop? PS_SHOP = "S"
                        if (PowerStatusConstants.Shop == powerMaster.PowerStatus)
                        {
                            var s = string.Format("Do not use Power unit {0}.  It is scheduled for the shop. ", driverLoginProcess.PowerId);
                            log.Error(s);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

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

                        // 4e) If no odometer record for this unit, accept as sent by the handheld.  
                        // 4f) If overrride flag = "Y", accept as sent by the handheld.  
                        if (powerMaster.PowerOdometer == null || driverLoginProcess.OverrideFlag == Constants.Yes)
                        {
                            powerMaster.PowerOdometer = driverLoginProcess.Odometer;

                            // Update PowerMaster 
                            scratchChangeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                            if (Common.LogChangeSetFailure(scratchChangeSetResult, powerMaster, log))
                            {
                                var s = string.Format("Unable to update power unit {0}.", driverLoginProcess.PowerId);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }

                            // Add PowerHistory
                            if (!InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, userRoleIds,
                                userCulture, out fault))
                            {
                                if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                                {
                                    break;
                                }
                                var s = string.Format("Could not insert Power History for Power Unit {0}.", driverLoginProcess.PowerId);
                                log.Error(s);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet());
                                break;
                            }
                        }

                        // 4g) Odometer tolerance checks.
                        if (driverLoginProcess.OverrideFlag == null || driverLoginProcess.OverrideFlag == "N")
                        {
                            query = new Query
                            {
                                //"Preferences?$filter= TerminalId='{0}' and Parameter='DEFOdomWarnRange'"
                                CurrentQuery = new QueryBuilder<Preference>().Filter(t => 
                                    t.Property(p => p.TerminalId).EqualTo(employeeMaster.DefTerminalId).And()
                                     .Property(p => p.Parameter).EqualTo("DEFOdomWarnRange"))
                                     .GetQuery()
                            };
                            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                                settings.Token, out fault);
                            if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                            {
                                break;
                            }
                            var odomPreference = (Preference) queryResult.Records.Cast<Preference>().FirstOrNull();
                            if (null != odomPreference?.ParameterValue)
                            {
                                var deltaMiles = int.Parse(odomPreference.ParameterValue);
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

                        query = new Query
                        {
                            //"DriverStatuss?$filter= EmployeeId='{0}'", driverLoginProcess.EmployeeId
                            CurrentQuery = new QueryBuilder<DriverStatus>().Filter(t =>
                                t.Property(p => p.EmployeeId).EqualTo(driverLoginProcess.EmployeeId))
                                 .GetQuery()
                        };
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                            settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        var driverStatus = (DriverStatus) queryResult.Records.Cast<DriverStatus>().FirstOrNull();

// TODO:  What if null?  Create one?
//                    if (null == driverStatus)
//                    {
//                        driverStatus = new DriverStatus()
//                        {
//                            EmployeeId = driverLoginProcess.EmployeeId,
//                            Status = DriverStatusSRConstants.LoggedIn,
//                            LoginDateTime = driverLoginProcess.LoginDateTime,
//                            ActionDateTime = driverLoginProcess.LoginDateTime,
//                            Odometer = driverLoginProcess.Odometer,
//                        };
//                    }

                        if (null != driverStatus)
                        {

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

                            // 6) Determine current trip number, segment, and driver status
                            if (null == driverStatus.TripNumber)
                            {
                                driverLoginProcess.TripNumber = null;
                                driverLoginProcess.TripSegNumber = null;
                                driverLoginProcess.DriverStatus = null;
                            }
                            else
                            {
                                driverLoginProcess.TripNumber = driverStatus.TripNumber;
                                driverLoginProcess.DriverStatus = driverStatus.PrevDriverStatus ?? driverStatus.Status;
                                if (DriverStatusSRConstants.Done == driverLoginProcess.DriverStatus)
                                {
                                    //"TripSegments?$filter= TripNumber='{0}' and TripSegStatus in ['P';'M'] &$orderby=TripSegNumber"
                                    //, driverStatus.TripNumber
                                    query = new Query
                                    {
                                        CurrentQuery = new QueryBuilder<TripSegment>().Filter(t => 
                                            t.Property(p => p.TripNumber).EqualTo(driverStatus.TripNumber).And()
                                            .Property(p => p.TripSegStatus).In("P", "M"))
                                            .OrderBy(p => p.TripSegNumber)
                                            .GetQuery()
                                    };
                                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                                        settings.Token, out fault);
                                    if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                                    {
                                        break;
                                    }
                                    var tripSegmentCurrent = (TripSegment) queryResult.Records.Cast<TripSegment>().FirstOrNull();
                                    driverLoginProcess.TripSegNumber = tripSegmentCurrent?.TripSegNumber;
                                }
                                else
                                {
                                    driverLoginProcess.TripSegNumber = driverStatus.TripSegNumber;
                                }
                            }

                            if (DriverStatusSRConstants.EnRoute != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.Arrive != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.StateCrossing != driverLoginProcess.DriverStatus)
                            {
                                driverLoginProcess.DriverStatus = null;
                            }
                        }
                        // 7) Update Trip in progress flag in the trip table
                        if (driverLoginProcess.TripNumber != null && driverLoginProcess.TripSegNumber == "01")
                        {
                            if (DriverStatusSRConstants.EnRoute != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.Arrive != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.StateCrossing != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.Done != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.Delay != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.BackOnDuty != driverLoginProcess.DriverStatus &&
                                DriverStatusSRConstants.Fuel != driverLoginProcess.DriverStatus)
                            {
                                // Update Trip in progress flag in the trip table to 'N'
                                query = new Query
                                {
                                    //"Trips?$filter= TripNumber='{0}'", driverLoginProcess.TripNumber
                                    CurrentQuery = new QueryBuilder<Trip>().Filter(t => 
                                        t.Property(p => p.TripNumber).EqualTo(driverLoginProcess.TripNumber))
                                         .GetQuery()
                                };
                                queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                                    settings.Token, out fault);
                                if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                                {
                                    break;
                                }
                                var trip = (Trip) queryResult.Records.Cast<Trip>().FirstOrNull();
                                if (null == trip)
                                {
                                    // TODO:  What if null?  Hard Error
                                }
                                trip.TripInProgressFlag = Constants.No;
                                var tripRecordType = (TripRecordType) dataService.RecordTypes.Single(x => x.TypeName == "Trip");
                                var tripChangeSet = (ChangeSet<string, Trip>) tripRecordType.GetNewChangeSet();
                                tripChangeSet.AddUpdate(trip.Id, trip);
                                var tripChangeSetResult = tripRecordType.ProcessChangeSet(dataService, tripChangeSet, settings);
                                if (Common.LogChangeSetFailure(tripChangeSetResult, tripChangeSet, log))
                                {
                                    var s = string.Format("Could not update Trip for TripInProgressFlag: {0}.", Constants.No);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }
                            }
                        }

                        // 8) Check for open ended delays
                        query = new Query
                        {
                            // "DriverDelays?$filter= DriverId='{0}' and DelayEndDateTime isnull &$orderby=DelaySeqNumber desc"
                            //, driverLoginProcess.EmployeeId);
                            CurrentQuery = new QueryBuilder<DriverDelay>().Filter(t =>
                                t.Property(p => p.DriverId).EqualTo(driverLoginProcess.EmployeeId).And()
                                .Property(p => p.DelayEndDateTime).IsNull())
                                .OrderBy(p => p.DelaySeqNumber, Direction.Descending)
                                .GetQuery()
                        };
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                            settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess))
                        {
                            break;
                        }
                        var delays = queryResult.Records.Cast<DriverDelay>().ToArray();
                        if (delays.Length > 0)
                        {
                            delays[0].DelayEndDateTime = driverLoginProcess.LoginDateTime;
                            for (int i = 1; i < delays.Length; i++)
                            {
                                delays[i].DelayEndDateTime = delays[i - 1].DelayStartDateTime;
                            }

                            var driverDelayRecordType = (DriverDelayRecordType)
                                dataService.RecordTypes.Single(x => x.TypeName == "DriverDelay");
                            var driverDelayChangeSet = (ChangeSet<string, DriverDelay>)
                                driverDelayRecordType.GetNewChangeSet();
                            foreach (var driverDelay in delays)
                            {
                                driverDelayChangeSet.AddUpdate(driverDelay.Id, driverDelay);
                            }
                            var driverDelayChangeSetResult = driverDelayRecordType.ProcessChangeSet(dataService,
                                driverDelayChangeSet, settings);
                            if (driverDelayChangeSetResult.FailedCreates.Any() ||
                                driverDelayChangeSetResult.FailedUpdates.Any() ||
                                driverDelayChangeSetResult.FailedDeletions.Any())
                            {
                                var s = "Could not update open ended driver delays";
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }

                            // 8b) Add "Back On Duty" flag  to DriverStatus and Driver History tables.
                            // Actually update them 2 steps later?
                            // TODO: Do we skip all this if there is no TripNumber?
                            driverStatus.Status = DriverStatusSRConstants.BackOnDuty;
                            scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                            if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                            {
                                var s = "Could not update driver status";
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                            if (!InsertDriverHistory(dataService, settings, employeeMaster, driverStatus, userRoleIds,
                                userCulture, out fault))
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

                        // 9) Check for power unit change for trip in progress
                        // TODO:  If trip is partially completed & power unit id has changed (from what?)
                        // TODO:  use the new power id and starting odometer to update trip segment and trip segment milage tables
                        // TODO:  What if more than one TripSegmentMileage?
                        // TODO:        Don't we really want the first (sort ASC)?
                        query = new Query
                        {
                            // "TripSegments?$filter= TripNumber='{0}' and TripSegNumber='{1}' and TripSegOdometerStart isnotnull 
                            // and TripSegOdometerEnd is null ", driverStatus.TripNumber, driverStatus.TripSegNumber
                            CurrentQuery = new QueryBuilder<TripSegment>().Filter(t =>
                                t.Property(p => p.TripNumber).EqualTo(driverStatus.TripNumber).And()
                                .Property(p => p.TripSegNumber).EqualTo(driverStatus.TripSegNumber).And()
                                .Property(p => p.TripSegOdometerStart).NotEqualTo(null).And()
                                .Property(p => p.TripSegOdometerEnd).EqualTo(null))
                                .GetQuery()
                        };
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
                        var tripSegment = (TripSegment) queryResult.Records.Cast<TripSegment>().FirstOrNull();
                        if (null != tripSegment)
                        {
                            tripSegment.TripSegPowerId = driverLoginProcess.PowerId;
                            tripSegment.TripSegOdometerStart = driverLoginProcess.Odometer;
                        }

                        query = new Query
                        {
                            // "TripSegmentMileages?$filter= TripNumber='{0}' and TripSegNumber='{1}' and TripSegMileageOdometerStart isnotnull 
                            // and TripSegMileageOdometerEnd is null &$orderby=TripSegMileageSeqNumber desc",
                            // driverStatus.TripNumber, driverStatus.TripSegNumber
                            CurrentQuery = new QueryBuilder<TripSegmentMileage>().Filter(t =>
                                t.Property(p => p.TripNumber).EqualTo(driverStatus.TripNumber).And()
                                .Property(p => p.TripSegNumber).EqualTo(driverStatus.TripSegNumber).And()
                                .Property(p => p.TripSegMileageOdometerStart).NotEqualTo(null).And()
                                .Property(p => p.TripSegMileageOdometerEnd).EqualTo(null))
                                .OrderBy(p => p.TripSegMileageSeqNumber, Direction.Descending)
                                .GetQuery()
                        };
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
                        var tripSegmentMileage = (TripSegmentMileage)queryResult.Records.Cast<TripSegmentMileage>().FirstOrNull();
                        if (null != tripSegmentMileage)
                        {
                            tripSegmentMileage.TripSegMileagePowerId = driverLoginProcess.PowerId;
                            tripSegmentMileage.TripSegMileageOdometerStart = driverLoginProcess.Odometer;
                            tripSegmentMileage.TripSegMileageDriverId = driverLoginProcess.EmployeeId;
                            tripSegmentMileage.TripSegMileageDriverName = Common.GetDriverName(employeeMaster);
                        }

                        // 10) Add/update record to DriverStatus table.

                        // 11) Add record to DriverHistory table

                        // 12) update power master
                        powerMaster.PowerStatus = PowerStatusConstants.InUse;
                        powerMaster.PowerDriverId = driverLoginProcess.EmployeeId;
                        powerMaster.PowerOdometer = driverLoginProcess.Odometer;
                        powerMaster.PowerLastActionDateTime = driverLoginProcess.LoginDateTime;
                        // TODO:  Are these always populated at this point?
                        powerMaster.PowerCurrentTripNumber = driverLoginProcess.TripNumber;
                        powerMaster.PowerCurrentTripSegNumber = driverLoginProcess.TripSegNumber;

                        scratchChangeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                        if (Common.LogChangeSetFailure(scratchChangeSetResult, powerMaster, log))
                        {
                            var s = string.Format("Could not update Power Master for Power Unit {0}.",
                                driverLoginProcess.PowerId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        // 13) Add record for PowerHistory table
                        if (!InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, userRoleIds,
                                userCulture, out fault))
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
                        // Not implemented see ContainerMasterProcess.

                        // 15) Send terminal master updates
                        // Not implemented see TerminalMasterProcess.

                        // 16) Send Universal Commodities
                        // Not implemented see UniversalCommoditiesProcess.

                        // 17) Send preferences (after some filtering)
                        // Not implemented see PreferencesProcess.

                        // 18) Send Code list
                        // Not implemented see CodeListProcess.

                        // 19) Send Trips

                        // 20) Send Container Inventory

                        // 21) Send dispatcher list for messaging

                        // 22) populate and send the Login message back to the handheld

                        // 23) Send GPS Company Info to tracker

                        // 24) Send GV Driver START Packet to tracker

                        // 25) Send GV Driver CODE Packet to tracker

                        // 26) Send GV Driver NAME Packet to tracker

                        // 27) Add entry to Event Log - Login Received.

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
        //  Note:  caller must handle faults.  E.G. if( handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerMaster"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        private bool InsertPowerHistory(IDataService dataService, ProcessChangeSetSettings settings,
            PowerMaster powerMaster, EmployeeMaster employeeMaster,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // i) Next Sequential for PowerNumber
            Query query = new Query
            {
                // "PowerHistorys?$filter= PowerId='{0}' &$orderby=PowerSeqNumber desc", powerMaster.PowerId
                CurrentQuery = new QueryBuilder<PowerHistory>().Top(1).Filter(t => 
                    t.Property(p => p.PowerId).EqualTo(powerMaster.PowerId))
                     .OrderBy(p => p.PowerSeqNumber, Direction.Descending)
                     .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            int powerSeqNo = 1;
            var powerHistoryMax = (PowerHistory) queryResult.Records.Cast<PowerHistory>().FirstOrNull();
            if (powerHistoryMax != null)
            {
                powerSeqNo = 1 + powerHistoryMax.PowerSeqNumber;
            }

            // ii) Power Cust Type Desc
            query = new Query
            {
                // CodeTables?$filter= CodeName='CUSTOMERTYPE' and CodeValue='{0}'", powerMaster.PowerCustType
                CurrentQuery = new QueryBuilder<CodeTable>().Filter(t => 
                    t.Property(p => p.CodeName).EqualTo("CUSTOMERTYPE").And()
                    .Property(p => p.CodeValue).EqualTo(powerMaster.PowerCustType))
                    .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var codeTableCustomerType = (CodeTable) queryResult.Records.Cast<CodeTable>().FirstOrNull();
            string powerCustTypeDesc = codeTableCustomerType?.CodeDisp1;

            // iii) Terminal Master
            query = new Query
            {
                //"TerminalMasters?$filter= TerminalId='{0}'", powerMaster.PowerTerminalId);
                CurrentQuery = new QueryBuilder<TerminalMaster>().Filter(t =>
                    t.Property(p => p.TerminalId).EqualTo(powerMaster.PowerTerminalId))
                     .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var terminalMaster = (TerminalMaster) queryResult.Records.Cast<TerminalMaster>().FirstOrNull();
            string powerTerminalName = terminalMaster?.TerminalName;

            // iv) Region Master
            query = new Query
            {
                // RegionMasters?$filter= RegionId='{0}'", powerMaster.PowerRegionId
                CurrentQuery = new QueryBuilder<RegionMaster>().Filter(t =>
                    t.Property(p => p.RegionId).EqualTo(powerMaster.PowerRegionId))
                     .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var regionMaster = (RegionMaster) queryResult.Records.Cast<RegionMaster>().FirstOrNull();
            string powerRegionName = regionMaster?.RegionName;

            // v) Power Status Desc 
            query = new Query
            {
                // CodeTables?$filter= CodeName='POWERUNITSTATUS' and CodeValue='{0}'", powerMaster.PowerStatus
                CurrentQuery = new QueryBuilder<CodeTable>().Filter(t =>
                    t.Property(p => p.CodeName).EqualTo("POWERUNITSTATUS").And()
                    .Property(p => p.CodeValue).EqualTo(powerMaster.PowerStatus))
                    .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var codeTablePowerStatus = (CodeTable) queryResult.Records.Cast<CodeTable>().FirstOrNull();
            string powerStatusDesc = codeTablePowerStatus?.CodeDisp1;

            // vi) Customer Master
            query = new Query
            {
                // CustomerMasters?$filter= CustHostCode='{0}'", powerMaster.PowerCustHostCode`
                CurrentQuery = new QueryBuilder<CustomerMaster>().Filter(t =>
                    t.Property(p => p.CustHostCode).EqualTo(powerMaster.PowerCustHostCode))
                     .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            CustomerMaster customerMaster = (CustomerMaster) queryResult.Records.Cast<CustomerMaster>().FirstOrNull();

            // vii) Basic Trip Type
            query = new Query
            {
                // "TripTypeBasics?$filter= TripTypeCode='{0}'", powerMaster.PowerCurrentTripSegType
                CurrentQuery = new QueryBuilder<TripTypeBasic>().Filter(t =>
                    t.Property(p => p.TripTypeCode).EqualTo(powerMaster.PowerCurrentTripSegType))
                     .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var tripTypeBasic = (TripTypeBasic) queryResult.Records.Cast<TripTypeBasic>().FirstOrNull();
            string tripTypeBasicDesc = tripTypeBasic?.TripTypeDesc;

            PowerHistory powerHistory = new PowerHistory()
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
                PowerCustName = customerMaster?.CustName,
                PowerCustAddress1 = customerMaster?.CustAddress1,
                PowerCustAddress2 = customerMaster?.CustAddress2,
                PowerCustCity = customerMaster?.CustCity,
                PowerCustState = customerMaster?.CustState,
                PowerCustZip = customerMaster?.CustZip,
                PowerCustCountry = customerMaster?.CustCountry,
                PowerCustCounty = customerMaster?.CustCounty,
                PowerCustTownship = customerMaster?.CustTownship,
                PowerCustPhone1 = customerMaster?.CustPhone1,
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
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        private bool InsertDriverHistory(IDataService dataService, ProcessChangeSetSettings settings,
            EmployeeMaster employeeMaster, DriverStatus driverStatus,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // lookup max(DriverHistory.DriverSeqNumber) + 1
            int driverSeqNo = 1;
            var query = new Query
            {
                //"DriverHistorys?$filter= EmployeeId='{0}' and TripNumber='{1}' &$orderby=DriverSeqNumber desc", employeeMaster.EmployeeId, driverStatus.TripNumber
                CurrentQuery = new QueryBuilder<DriverHistory>().Top(1).Filter(t =>
                        t.Property(p => p.EmployeeId).EqualTo(employeeMaster.EmployeeId).And()
                         .Property(p => p.TripNumber).EqualTo(driverStatus.TripNumber))
                         .OrderBy(p => p.DriverSeqNumber, Direction.Descending)
                         .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var driverHistoryMax = (DriverHistory) queryResult.Records.Cast<PowerHistory>().FirstOrNull();
            if (driverHistoryMax != null)
            {
                driverSeqNo = 1 + driverHistoryMax.DriverSeqNumber;
            }

            // lookup code DRIVERSTATUS
            query = new Query
            {
                // "CodeTables?$filter= CodeName='DRIVERSTATUS' and CodeValue='{0}'", driverStatus.Status
                CurrentQuery = new QueryBuilder<CodeTable>().Filter(t =>
                    t.Property(p => p.CodeName).EqualTo("DRIVERSTATUS").And()
                     .Property(p => p.CodeValue).EqualTo(driverStatus.Status))
                     .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var codeTableDriverStatus = (CodeTable) queryResult.Records.Cast<CodeTable>().FirstOrNull();
            string driverStatusDesc = codeTableDriverStatus?.CodeDisp1;

            // lookup Terminal Master
            query = new Query
            {
                //"TerminalMasters?$filter= TerminalId='{0}'", driverStatus.TerminalId
                CurrentQuery = new QueryBuilder<TerminalMaster>().Filter(t =>
                    t.Property(p => p.TerminalId).EqualTo(driverStatus.TerminalId))
                    .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var driverTerminalMaster = (TerminalMaster) queryResult.Records.Cast<TerminalMaster>().FirstOrNull();
            string driverTerminalName = driverTerminalMaster?.TerminalName;

            // lookup RegionMaster
            query = new Query
            {
                // "RegionMasters?$filter= RegionId='{0}'", driverStatus.RegionId
                CurrentQuery = new QueryBuilder<RegionMaster>().Filter(t =>
                    t.Property(p => p.RegionId).EqualTo(driverStatus.RegionId))
                    .GetQuery()
            };
            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                out fault);
            if (null != fault)
            {
                return false;
            }
            var driverRegionMaster = (RegionMaster) queryResult.Records.Cast<RegionMaster>().FirstOrNull();
            string driverRegionName = driverRegionMaster?.RegionName;

            // lookup customerType
            //query = new Query
            //{
            //    //"CodeTables?$filter= CodeName='CUSTOMERTYPE' and CodeValue='{0}'", driverStatus.DestCustType);
            //    CurrentQuery = new QueryBuilder<CodeTable>().Filter(t =>
            //        t.Property(p => p.CodeName).EqualTo("CUSTOMERTYPE").And()
            //         .Property(p => p.CodeValue).EqualTo( ??????? Cust type form where ?????  )
            //         .GetQuery()
            //};
            // queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            // if (handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
            // var codeTableDestCustomerType = (CodeTable) queryResult.Records.Cast<CodeTable>().FirstOrNull();
            // string destCustTypeDesc = codeTableDestCustomerType?.CodeDisp1;

            var driverHistory = new DriverHistory()
            {
                EmployeeId = driverStatus.EmployeeId,
                TripNumber = driverStatus.TripNumber,
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
                // TODO:  There is no DriverStatus.DescCustType
                // DestCustType = ,
                // DestCustTypeDesc  =,
                // TODO:  There is driverStatus.DestCustHostCode
                // DestCustHostCode  = driverStatus.DestCustHostCode,
                // TODO:  Customer lookup based on?
                // DestCustName  = driverStatus.D,
                // DestCustAddress1  =,
                // DestCustAddress2  =,
                // DestCustCity  =,
                // DestCustState  =,
                // DestCustZip  =,
                // DestCustCountry  =,
                GPSAutoGeneratedFlag = driverStatus.GPSAutoGeneratedFlag,
                GPSXmitFlag = driverStatus.GPSXmitFlag,
                MdtVersion = driverStatus.MdtVersion
            };

            // Insert DriverHistory 
            var recordType = (DriverHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverHistory");
            var changeSet = (ChangeSet<string, DriverHistory>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, driverHistory, userRoleIds, userRoleIds);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, driverHistory, log))
            {
                return false;
            }
            return true;
        }


    }
}