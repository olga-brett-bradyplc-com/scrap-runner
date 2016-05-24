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
    /// Processing for a driver entering fuel.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverFuelEntryProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverFuelEntryProcess")]
    public class DriverFuelEntryProcessRecordType : ChangeableRecordType
        <DriverFuelEntryProcess, string, DriverFuelEntryProcessValidator, DriverFuelEntryProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverFuelEntryProcess, DriverFuelEntryProcess>();
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
                        ChangeSet<string, DriverFuelEntryProcess> changeSet, bool persistChanges)
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
                        ChangeSet<string, DriverFuelEntryProcess> changeSet, ProcessChangeSetSettings settings)
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

                    int powerFuelInsertCount = 0;
                    int driverHistoryInsertCount = 0;
                    int powerHistoryInsertCount = 0;

                    var driverFuelEntryProcess = (DriverFuelEntryProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverFuelEntryProcess backfillDriverFuelEntryProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverFuelEntryProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverFuelEntryProcess, driverFuelEntryProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process fuel entry for DriverId: "
                            + driverFuelEntryProcess.EmployeeId + "Trip: " + driverFuelEntryProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverFuelEntryProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: "
                                        + driverFuelEntryProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Power record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                      driverFuelEntryProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: "
                                        + driverFuelEntryProcess.PowerId));
                        break;
                    }
                    //Do not use the odometer from the driver if it is less than the last recorded 
                    //odometer stored in the PowerMaster.
                    if (powerMaster.PowerOdometer != null)
                    {
                        if (driverFuelEntryProcess.Odometer < powerMaster.PowerOdometer)
                        {
                            driverFuelEntryProcess.Odometer = (int)powerMaster.PowerOdometer;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Only associated delay with trip if the trip is in progress
                    var currentTrip = new Trip();
                    var currentTripSegment = new TripSegment();

                    if (driverFuelEntryProcess.TripNumber != null &&
                        driverFuelEntryProcess.TripSegNumber != null)
                    {
                        ////////////////////////////////////////////////
                        // Get the Trip record
                        currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                      driverFuelEntryProcess.TripNumber, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == currentTrip)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                            + driverFuelEntryProcess.TripNumber));
                            break;
                        }
                        if (currentTrip.TripInProgressFlag == Constants.No ||
                            currentTrip.TripInProgressFlag == null)
                        {
                            driverFuelEntryProcess.TripNumber = null;
                            driverFuelEntryProcess.TripSegNumber = null;
                        }
                        if (driverFuelEntryProcess.TripNumber != null &&
                            driverFuelEntryProcess.TripSegNumber != null)
                        {
                            ////////////////////////////////////////////////
                            // Get the Trip segment record
                            currentTripSegment = Common.GetTripSegment(dataService, settings, userCulture, userRoleIds,
                                   driverFuelEntryProcess.TripNumber, driverFuelEntryProcess.TripSegNumber, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            if (null == currentTrip)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                   + driverFuelEntryProcess.TripNumber + "-" + driverFuelEntryProcess.TripSegNumber));
                                break;
                            }
                        }
                    }

                    ////////////////////////////////////////////////
                    //Add a new PowerFuel Record
                    var powerFuel = new PowerFuel();

                    powerFuel.PowerId = driverFuelEntryProcess.PowerId;

                    //if Trip Number is empty then put the driver id in the field
                    if (driverFuelEntryProcess.TripNumber == null)
                    {
                        powerFuel.TripNumber = driverFuelEntryProcess.EmployeeId;
                    }
                    else
                    {
                        powerFuel.TripNumber = driverFuelEntryProcess.TripNumber;
                    }

                    if (driverFuelEntryProcess.TripNumber != null)
                    {
                        powerFuel.TripSegNumber = driverFuelEntryProcess.TripSegNumber;
                    }
                    powerFuel.TripTerminalId = employeeMaster.TerminalId;
                    powerFuel.TripRegionId = employeeMaster.RegionId;

                    powerFuel.TripDriverId = driverFuelEntryProcess.EmployeeId;
                    powerFuel.TripDriverName = Common.GetEmployeeName(employeeMaster);

                    powerFuel.PowerDateOfFuel = driverFuelEntryProcess.ActionDateTime;

                    powerFuel.PowerState = driverFuelEntryProcess.State;
                    powerFuel.PowerCountry = driverFuelEntryProcess.Country;

                    powerFuel.PowerOdometer = driverFuelEntryProcess.Odometer;
                    powerFuel.PowerGallons = driverFuelEntryProcess.FuelAmount;

                    //Do the insert
                    //The insert function will calculate the PowerFuelSeqNumber
                    log.DebugFormat("SRTEST:Saving PowerFuel Record for PowerId:{0} - Fuel Entry.",
                                    powerFuel.PowerId);
                    if (!Common.InsertPowerFuel(dataService, settings, userRoleIds, userCulture, log,
                                           powerFuel, ++powerFuelInsertCount, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerFuel failed: {0} during fuel entry request: {1}", fault.Message, driverFuelEntryProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update PowerMaster to set trip and segment number
                    //Update the following:
                    powerMaster.PowerDriverId = driverFuelEntryProcess.PowerId;
                    powerMaster.PowerOdometer = driverFuelEntryProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverFuelEntryProcess.ActionDateTime;
                    if (driverFuelEntryProcess.TripNumber != null)
                    {
                        powerMaster.PowerCurrentTripNumber = driverFuelEntryProcess.TripNumber;
                        powerMaster.PowerCurrentTripSegNumber = driverFuelEntryProcess.TripSegNumber;
                        powerMaster.PowerCurrentTripSegType = currentTripSegment.TripSegType;
                        powerMaster.PowerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                        powerMaster.PowerCustType = currentTripSegment.TripSegDestCustType;
                    }
                    else
                    {
                        powerMaster.PowerCurrentTripNumber = null;
                        powerMaster.PowerCurrentTripSegNumber = null;
                        powerMaster.PowerCurrentTripSegType = null;
                        powerMaster.PowerCustHostCode = null;
                        powerMaster.PowerCustType = null;
                    }
                    powerMaster.PowerLocation = null;
                    powerMaster.PowerComments = null;

                    //No need to set these:
                    //PowerRegionId
                    //PowerTerminalId

                    //Do the PowerMaster update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - Fuel Entry.",
                                    powerMaster.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("Could not update PowerMaster for PowerId:{0}.",
                                powerMaster.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster,
                        ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerHistory failed: {0} during fuel entry request: {1}", fault.Message, driverFuelEntryProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update DriverStatus
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                       driverFuelEntryProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " + driverFuelEntryProcess.EmployeeId));
                        break;
                    }

                    //Set the field values for driver status
                    //Save the status in the previous driver status field, so it can be restored.
                    if (driverStatus.Status != DriverStatusSRConstants.Fuel)
                    {
                        driverStatus.PrevDriverStatus = driverStatus.Status;
                    }
                    driverStatus.Status = DriverStatusSRConstants.Fuel;
                    //Do not want to remove previous driver status 
                    if (driverFuelEntryProcess.TripNumber != null)
                    {
                        driverStatus.TripNumber = driverFuelEntryProcess.TripNumber;
                        driverStatus.TripSegNumber = driverFuelEntryProcess.TripSegNumber;
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
                    driverStatus.PowerId = driverFuelEntryProcess.PowerId;
                    driverStatus.MDTId = driverFuelEntryProcess.Mdtid;
                    driverStatus.ActionDateTime = driverFuelEntryProcess.ActionDateTime;
                    driverStatus.Odometer = driverFuelEntryProcess.Odometer;

                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table. 
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during Fuel Entry request: {1}", fault.Message, driverFuelEntryProcess);
                        break;
                    }

                    //Set the driver status back to the previous status
                    driverStatus.Status = driverStatus.PrevDriverStatus;

                    //Do the DriverStatus update
                    changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                    log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Fuel Entry.",
                                    driverStatus.EmployeeId);
                    if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                    {
                        var s = string.Format("Could not update DriverStatus for DriverId:{0}.",
                            driverStatus.EmployeeId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }


                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Fuel Entry. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverFuel);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverFuelEntryProcess.ActionDateTime);
                    if (driverFuelEntryProcess.TripNumber != null)
                    { 
                        sbComment.Append(" Trip:");
                        sbComment.Append(driverFuelEntryProcess.TripNumber);
                        sbComment.Append("-");
                        sbComment.Append(driverFuelEntryProcess.TripSegNumber);
                    }
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverFuelEntryProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverFuelEntryProcess.PowerId);
                    sbComment.Append(" State:");
                    sbComment.Append(driverFuelEntryProcess.State);
                    sbComment.Append(" Odom:");
                    sbComment.Append(driverFuelEntryProcess.Odometer);
                    string comment = sbComment.ToString().Trim();
                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverFuelEntryProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverFuelEntryProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Fuel Entry");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                                driverFuelEntryProcess.EmployeeId, EventCommentConstants.ReceivedDriverFuel);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
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
                log.Debug("SRTEST:Transaction Rollback - FuelEntry");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - FuelEntry");
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
