using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using NHibernate;
using NHibernate.Util;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [EditAction("DriverLoginProcess")]
    public class DriverLoginProcessRecordType :
         ChangeableRecordType<DriverLoginProcess, string, DriverLoginProcessValidator, DriverLoginProcessDeletionValidator>
    {


        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverLoginProcess, DriverLoginProcess>();
        }

     
        // This is the deprecated signature.
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService, string token, string username, ChangeSet<string, DriverLoginProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }


        // This is the "real" method
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

            // Running the base process changeset first in this case.
            // This should gain the benefit of validators, auditing, security, piplines, etc.
            // However, it looks like we are losing some user input in the base.ProcessChangeSet
            // from a reretrieve or the sparse mapping?
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() && !changeSetResult.FailedDeletions.Any())
            { 
                // We only log in one person at a time but in the more genreal cases we could be processing multiple records.
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {

                    DriverLoginProcess driverLoginProcess = (DriverLoginProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();
                    DataServiceFault fault;
                    string msgKey = key;

                    // It appers I must backfill user input values that were clobbered by the call to the base process method.
                    DriverLoginProcess backfillDriverLoginProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverLoginProcess))
                    {
                        // Use a mapper?  Can I set this to map a subset?
                        Mapper.Map(backfillDriverLoginProcess, driverLoginProcess);

                        // vs this minimalist set?
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
                        // login error?
                    }

                    //
                    // 1) Process information from handheld
                    // NOTE: We might want to validate (or backfill) something like Locale against configured dialects rather than a hardcoded list in validator
                    //

                    // 2) Validate driver id:  Get the EmployeeMaster - an inefficient, client side filtering sample
                    // Query query = new Query(){CurrentQuery = "EmployeeMasters"};
                    // var employeeMasters = dataService.Query(query, settings.Username, Enumerable.Empty<long>(), "en-GB", settings.Token, out fault);
                    // var employeeMaster = employeeMasters.Records.Cast<EmployeeMaster>().Where(x => x.EmployeeId == driverLoginProcess.EmployeeId).First();

                    //
                    // 2) Validate driver id
                    //
                    Query query = new Query() { CurrentQuery = string.Format("EmployeeMasters?$filter= EmployeeId='{0}'", driverLoginProcess.EmployeeId) };
                    var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                        break;
                    }
                    var employeeMaster = (EmployeeMaster) queryResult.Records.Cast<EmployeeMaster>().FirstOrNull();
                    if (employeeMaster == null)
                    {
                        // Trap for invalid driver id
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + driverLoginProcess.EmployeeId));
                        break;
                    }

                    //
                    // 3) Lookup preferences
                    //
                    query.CurrentQuery = string.Format("Preferences?$filter= TerminalId='{0}'", employeeMaster.TerminalId);
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                        break;
                    }
                    var preferences = queryResult.Records.Cast<Preference>();
                    driverLoginProcess.Preferences = new List<Preference>(preferences.ToArray());

                    //
                    // 4a) Validate PowerId
                    //
                    query.CurrentQuery = string.Format("PowerMasters?$filter= PowerId='{0}'", driverLoginProcess.PowerId);
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                        break;
                    }
                    var powerMaster = (PowerMaster)queryResult.Records.Cast<PowerMaster>().FirstOrNull();
                    if (powerMaster == null)
                    {
                        // Trap for invalid driver id
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Power ID " + driverLoginProcess.PowerId));
                        break;
                    }

                    // 4b) Check system pref "DEFAllowAnyPowerUnit".  If not found or "N" then check company ownership.
                    query.CurrentQuery = "Preferences?$filter= TerminalId='0000' and Parameter='DEFAllowAnyPowerUnit'" ;
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                        break;
                    }
                    var preference = (Preference)queryResult.Records.Cast<Preference>().FirstOrNull();
                    if (preference == null || preference.ParameterValue == Constants.No)
                    {
                        // TODO:  Can it be and what if employeeMaster.RegionId is null? 
                        if (powerMaster.PowerRegionId != null && powerMaster.PowerRegionId != employeeMaster.RegionId)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Power ID " + driverLoginProcess.PowerId));
                            break;
                        }
                    }

                    // 4c) Check power unit status.  Scheduled for the shop?  PS_SHOP = "S"
                    if (PowerStatusConstants.Shop == powerMaster.PowerStatus)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(string.Format("Do not use Power unit {0}.  It is scheduled for the shop. ", driverLoginProcess.PowerId)));
                        break;
                    }

                    // 4d) Is unit in use by another driver? PS_INUSE = "I"
                    if (powerMaster.PowerStatus == PowerStatusConstants.InUse && powerMaster.PowerDriverId != employeeMaster.EmployeeId)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(string.Format("Power unit {0} in use by another driver.  Change Power Unit Number or call Dispatch.", driverLoginProcess.PowerId)));
                        break;
                    }

                    // 4e) If no odometer record for this unit, accept as sent by the handheld.  
                    // 4f) overrride flag = "Y", accept as sent by the handheld.  
                    if (powerMaster.PowerOdometer == null || driverLoginProcess.OverrideFlag == Constants.Yes)
                    {
                        powerMaster.PowerOdometer = driverLoginProcess.Odometer;

                        // Update PowerMaster 
                        var powerMasterRecordType = (PowerMasterRecordType) dataService.RecordTypes.Single(x => x.TypeName == "PowerMaster");
                        var powerMasterChangeSet = (ChangeSet<String, PowerMaster>) powerMasterRecordType.GetNewChangeSet();
                        powerMasterChangeSet.AddUpdate(powerMaster.Id, powerMaster);
                        var powerMasterChangeSetResult = powerMasterRecordType.ProcessChangeSet(dataService, powerMasterChangeSet, settings);
                        if (powerMasterChangeSetResult.FailedCreates.Any() ||
                            powerMasterChangeSetResult.FailedUpdates.Any() ||
                            powerMasterChangeSetResult.FailedDeletions.Any())
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(string.Format("Could not update master for Power Unit {0}.", driverLoginProcess.PowerId)));
                            break;
                        }

                        // Add PowerHistory
                        // i) Next Sequential for PowerNumber
                        int powerSeqNo;
                        query.CurrentQuery = string.Format("PowerHistorys?$filter= PowerId='{0}'&$orderby=PowerSeqNumber desc", driverLoginProcess.PowerId);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var powerHistoryMax = (PowerHistory) queryResult.Records.Cast<PowerHistory>().FirstOrNull();
                        if (powerHistoryMax == null)
                        {
                            powerSeqNo = 1;
                        }
                        else
                        {
                            powerSeqNo = 1 + powerHistoryMax.PowerSeqNumber;
                        }

                        // ii) Power Cust Type Desc
                        query.CurrentQuery = string.Format("CodeTables?$filter= CodeName='CUSTOMERTYPE' and CodeValue='{0}'", powerMaster.PowerCustType);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var codeTableCustomerType = (CodeTable)queryResult.Records.Cast<CodeTable>().FirstOrNull();
                        string powerCustTypeDesc = codeTableCustomerType?.CodeDisp1;

                        // iii) Terminal Master
                        query.CurrentQuery = string.Format("TerminalMasters?$filter= TerminalId='{0}'", powerMaster.PowerTerminalId);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var terminalMaster = (TerminalMaster)queryResult.Records.Cast<TerminalMaster>().FirstOrNull();
                        string powerTerminalName = terminalMaster?.TerminalName;

                        // iv) Region Master
                        query.CurrentQuery = string.Format("RegionMasters?$filter= RegionId='{0}'", powerMaster.PowerRegionId);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var regionMaster = (RegionMaster)queryResult.Records.Cast<RegionMaster>().FirstOrNull();
                        string powerRegionName = regionMaster?.RegionName;

                        // v) Power Status Desc 
                        query.CurrentQuery = string.Format("CodeTables?$filter= CodeName='POWERUNITSTATUS' and CodeValue='{0}'", powerMaster.PowerStatus);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var codeTablePowerStatus = (CodeTable)queryResult.Records.Cast<CodeTable>().FirstOrNull();
                        string powerStatusDesc = codeTablePowerStatus?.CodeDisp1;

                        // vi) Customer Master
                        query.CurrentQuery = string.Format("CustomerMasters?$filter= CustHostCode='{0}'", powerMaster.PowerCustHostCode);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        CustomerMaster customerMaster = (CustomerMaster) queryResult.Records.Cast<CustomerMaster>().FirstOrNull();

                        // vii) Basic Trip Type
                        query.CurrentQuery = string.Format("TripTypeBasics?$filter= TripTypeCode='{0}'", powerMaster.PowerCurrentTripSegType);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var tripTypeBasic = (TripTypeBasic) queryResult.Records.Cast<TripTypeBasic>().FirstOrNull();
                        string tripTypeBasicDesc = tripTypeBasic?.TripTypeDesc;

                        PowerHistory powerHistory = new PowerHistory() { 
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
                            PowerDriverName  = string.Format("{0}, {1}", employeeMaster?.LastName, employeeMaster?.FirstName),
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
                        } ;

                        // Insert PowerHistory 
                        var powerHistoryRecordType = (PowerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerHistory");
                        var powerHistoryChangeSet = (ChangeSet<string, PowerHistory>)powerHistoryRecordType.GetNewChangeSet();
                        long arbitraryRecordRef = 1;
                        powerHistoryChangeSet.AddCreate(arbitraryRecordRef, powerHistory, userRoleIds, userRoleIds);
                        var powerHistoryChangeSetResult = powerHistoryRecordType.ProcessChangeSet(dataService, powerHistoryChangeSet, settings);
                        if (powerHistoryChangeSetResult.FailedCreates.Any() ||
                            powerHistoryChangeSetResult.FailedUpdates.Any() ||
                            powerHistoryChangeSetResult.FailedDeletions.Any())
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(string.Format("Could not insert power history for Power Unit {0}.", driverLoginProcess.PowerId)));
                            break;
                        }
                    }

                    // 4g) Odometer tolerance checks.
                    if (driverLoginProcess.OverrideFlag == null || driverLoginProcess.OverrideFlag == "N")
                    {
                        query.CurrentQuery = string.Format("Preferences?$filter= TerminalId='{0}' and Parameter='DEFOdomWarnRange'", employeeMaster.DefTerminalId);
                        queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture,
                            settings.Token, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                            break;
                        }
                        var odomPreference = (Preference) queryResult.Records.Cast<Preference>().FirstOrNull();
                        if (null != odomPreference?.ParameterValue)
                        {
                            int deltaMiles = int.Parse(odomPreference.ParameterValue);
                            if (driverLoginProcess.Odometer < powerMaster.PowerOdometer.Value - deltaMiles ||
                                driverLoginProcess.Odometer > powerMaster.PowerOdometer.Value + deltaMiles)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey,
                                    new MessageSet("Warning! Please check odometer and log in again."));
                                break;
                            }
                        }
                    }

                    query.CurrentQuery = String.Format("DriverStatuss?$filter= EmployeeId='{0}'", driverLoginProcess.EmployeeId);
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                        break;
                    }
                    var driverStatus = (DriverStatus)queryResult.Records.Cast<DriverStatus>().FirstOrNull();
                    // TODO:  What if null?

                    // 5) Validate Duplicate login?
                    //if (null != driverStatus)
                    //{
                    //    if (driverStatus.PowerId == driverLoginProcess.PowerId &&
                    //        driverStatus.Odometer == driverLoginProcess.Odometer)
                    //    {
                    //        // TODO: What/whoch DateTime form the handheld do we check?  Or do we backfill upon arrival?
                    //        var timeSpan = driverStatus.LoginDateTime - driverLoginProcess.????;
                    //        if (timeSpan < TimeSpan.FromSeconds(30) && timeSpan > TimeSpan.FromSeconds(-30))
                    //        {
                    //            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Duplicate Login"));
                    //            break;
                    //        }
                    //    }
                    //}

                    // 6) Determine current trip number, segment, and driver status
                    if (null != driverStatus)
                    {
                        if (null == driverStatus.TripNumber)
                        {
                            driverLoginProcess.TripNumber = null;
                            driverLoginProcess.TripSegNumber = null;
                            driverLoginProcess.DriverStatus = null;
                        }
                        else
                        {
                            driverLoginProcess.TripNumber = driverStatus.TripNumber;
                            driverLoginProcess.DriverStatus = driverStatus.PrevDriverStatus;
                            if (null == driverLoginProcess.DriverStatus)
                            {
                                driverLoginProcess.DriverStatus = driverStatus.Status;
                            }
                            if (DriverStatusSRConstants.Done == driverLoginProcess.DriverStatus)
                            {
                                query.CurrentQuery = string.Format(
                                    "TripSegments?$filter= TripNumber='{0}' and TripSegStatus in ['P';'M'] &$orderby=TripSegNumber desc",
                                    driverStatus.TripNumber);
                                queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                                if (null != fault)
                                {
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                                    break;
                                }
                                TripSegment tripSegment = (TripSegment) queryResult.Records.Cast<TripSegment>().FirstOrNull();
                                driverLoginProcess.TripSegNumber = tripSegment?.TripSegNumber;
                            }
                            else
                            {
                                driverLoginProcess.TripSegNumber = driverStatus.TripSegNumber;
                            }
                        }

                        if (DriverStatusSRConstants.EnRoute != driverLoginProcess.DriverStatus  && 
                            DriverStatusSRConstants.Arrive != driverLoginProcess.DriverStatus &&
                            DriverStatusSRConstants.StateCrossing != driverLoginProcess.DriverStatus)
                        {
                            driverLoginProcess.DriverStatus = null;
                        }
                    }

                    // 7) Update Trip in progress flag in the trip table
                    if (driverLoginProcess.TripNumber != null && driverLoginProcess.TripSegNumber == "01")
                    {
                        if (DriverStatusSRConstants.EnRoute  != driverLoginProcess.DriverStatus &&
                            DriverStatusSRConstants.Arrive != driverLoginProcess.DriverStatus  &&
                            DriverStatusSRConstants.StateCrossing != driverLoginProcess.DriverStatus &&
                            DriverStatusSRConstants.Done != driverLoginProcess.DriverStatus  &&
                            DriverStatusSRConstants.Delay != driverLoginProcess.DriverStatus &&
                            DriverStatusSRConstants.BackOnDuty != driverLoginProcess.DriverStatus &&
                            DriverStatusSRConstants.Fuel != driverLoginProcess.DriverStatus)
                        {
                            // Update Trip in progress flag in the trip table to 'N'
                            query.CurrentQuery = string.Format("Trips?$filter= TripNumber='{0}'", driverLoginProcess.TripNumber);
                            queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
                                break;
                            }
                            var trip = queryResult.Records.Cast<Trip>().First();
                            trip.TripInProgressFlag = Constants.No;
                            var tripRecordType = (TripRecordType)dataService.RecordTypes.Single(x => x.TypeName == "Trip");
                            var tripChangeSet = (ChangeSet<string, Trip>)tripRecordType.GetNewChangeSet();
                            tripChangeSet.AddUpdate(trip.Id, trip);
                            var tripChangeSetResult = tripRecordType.ProcessChangeSet(dataService, tripChangeSet, settings);
                            if (tripChangeSetResult.FailedCreates.Any() ||
                                tripChangeSetResult.FailedUpdates.Any() ||
                                tripChangeSetResult.FailedDeletions.Any())
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(string.Format("Could not update Trip for TripInProgressFlag: {0}.", Constants.No)));
                                break;
                            }
                        }
                    }

                    // 8) Check for open ended delays
                    query.CurrentQuery = string.Format("DriverDelays?$filter= DriverId='{0}' and DelayEndDateTime isnull &$orderby=DelaySeqNumber desc", driverLoginProcess.EmployeeId);
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault));
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

                        var driverDelayRecordType = (DriverDelayRecordType) dataService.RecordTypes.Single(x => x.TypeName == "DriverDelay");
                        var driverDelayChangeSet = (ChangeSet<string, DriverDelay>) driverDelayRecordType.GetNewChangeSet();
                        foreach (var driverDelay in delays)
                        {
                            driverDelayChangeSet.AddUpdate(driverDelay.Id, driverDelay);
                        }
                        var driverDelayChangeSetResult = driverDelayRecordType.ProcessChangeSet(dataService, driverDelayChangeSet, settings);
                        if (driverDelayChangeSetResult.FailedCreates.Any() ||
                            driverDelayChangeSetResult.FailedUpdates.Any() ||
                            driverDelayChangeSetResult.FailedDeletions.Any())
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Could not update open ended driver delays"));
                            break;
                        }
                    }

                    // 8b) Add "Back On Duty" flag  to DriverStatus and Driver History tables.


                    // 9) Check for power unit change for trip in progress

                    // 10) Add/update record to DriverStatus table.

                    // 11) Add record to DriverHistory table

                    // 12) update power master

                    // 13) Add record for PowerHistory table

                    // 14) Send container master updates

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
                dataService.NotifyOfExternalChangesToData(TypeName); 
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }

    }
}
