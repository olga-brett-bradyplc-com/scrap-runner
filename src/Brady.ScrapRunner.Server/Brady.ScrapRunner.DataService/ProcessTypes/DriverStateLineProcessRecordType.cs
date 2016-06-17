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
    /// Processing for a driver state line crossing.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverStateLineProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverStateLineProcess")]
    public class DriverStateLineProcessRecordType : ChangeableRecordType
        <DriverStateLineProcess, string, DriverStateLineProcessValidator, DriverStateLineProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverStateLineProcess, DriverStateLineProcess>();
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
                        ChangeSet<string, DriverStateLineProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
        /// <summary>
        /// Perform the driver state line crossing processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverStateLineProcess> changeSet, ProcessChangeSetSettings settings)
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

                    int tripSegmentMileageCount = 0;
                    int driverHistoryInsertCount = 0;
                    int powerHistoryInsertCount = 0;

                    var driverStateLineProcess = (DriverStateLineProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverStateLineProcess backfillDriverStateLineProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverStateLineProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverStateLineProcess, driverStateLineProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process state line crossing for DriverId: " 
                                          + driverStateLineProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //DriverStateLineProcess has been called
                    log.DebugFormat("SRTEST:DriverStateLineProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverStateLineProcess Driver:{0} DT:{1} Trip:{2}-{3} PowerId:{4} Odom:{5} ST:{6} Ctry:{7} GPSAuto:{8} Lat:{9} Lon:{10} MDT:{11}",
                                     driverStateLineProcess.EmployeeId, driverStateLineProcess.ActionDateTime,
                                     driverStateLineProcess.TripNumber, driverStateLineProcess.TripSegNumber,
                                     driverStateLineProcess.PowerId, driverStateLineProcess.Odometer,
                                     driverStateLineProcess.State, driverStateLineProcess.Country,
                                     driverStateLineProcess.GPSAutoFlag, driverStateLineProcess.Latitude,
                                     driverStateLineProcess.Longitude, driverStateLineProcess.Mdtid);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverStateLineProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid DriverId: "
                                        + driverStateLineProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverStateLineProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == currentTrip)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid TripNumber: "
                                        + driverStateLineProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(currentTrip))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. StateLine processing ends.",
                                        driverStateLineProcess.TripNumber);
                        break;
                    }


                    ////////////////////////////////////////////////
                    //Get a list of all  segments for the trip
                    var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                                        driverStateLineProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var currentTripSegment = (from item in tripSegList
                                              where item.TripSegNumber == driverStateLineProcess.TripSegNumber
                                              select item).FirstOrDefault();
                    if (null == currentTripSegment)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid TripSegment: " +
                            driverStateLineProcess.TripNumber + "-" + driverStateLineProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Customer record for the destination cust host code
                    var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                             currentTripSegment.TripSegDestCustHostCode, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == destCustomerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid CustHostCode: "
                                        + currentTripSegment.TripSegDestCustHostCode));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Adjust odometer based on previously recorded odometer. 
                    //If odometer from mobile device (driverStateLineProcess.Odometer) is less than PowerMaster.PowerOdometer, 
                    //use the PowerMaster.PowerOdometer instead of the odometer from the mobile device.
                    //Adjust odometer here before we start using driverStateLineProcess.Odometer
                    ////////////////////////////////////////////////
                    // Get the PowerMaster record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                      driverStateLineProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid PowerId: "
                                        + driverStateLineProcess.PowerId));
                        break;
                    }

                    //Do not use the odometer from the driver if it is less than the last recorded 
                    //odometer stored in the PowerMaster.
                    if (powerMaster.PowerOdometer != null)
                    {
                        if (driverStateLineProcess.Odometer < powerMaster.PowerOdometer)
                        {
                            driverStateLineProcess.Odometer = (int)powerMaster.PowerOdometer;
                        }
                    }
                    ////////////////////////////////////////////////////////
                    //If the MDTId is not provided by the mobile app, build it using the MDT Prefix (if it exists) plus the employee id.
                    if (driverStateLineProcess.Mdtid == null)
                    {
                        // Lookup Preference: DEFMDTPrefix
                        string prefMDTPrefix = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                      Constants.SystemTerminalId, PrefSystemConstants.DEFMDTPrefix, out fault);
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        driverStateLineProcess.Mdtid = prefMDTPrefix + driverStateLineProcess.EmployeeId;
                    }
                    ////////////////////////////////////////////////
                    //First validate country
                    var codeTableCountry = new CodeTable();
                    if (null != driverStateLineProcess.Country)
                    {
                        codeTableCountry = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                              CodeTableNameConstants.Countries, driverStateLineProcess.Country, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == codeTableCountry)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid Country: "
                                            + driverStateLineProcess.Country));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Now validate state and country combination
                    var codeTableStateCountry = new CodeTable();
                    if (null != driverStateLineProcess.State && 
                        null != driverStateLineProcess.Country)
                    {
                        codeTableStateCountry = Common.GetCodeTableEntryForStateCountry(dataService, settings, userCulture, userRoleIds,
                                              CodeTableNameConstants.States, driverStateLineProcess.State, driverStateLineProcess.Country, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == codeTableStateCountry)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid State/Country: "
                                            + driverStateLineProcess.State + "/" + driverStateLineProcess.Country));
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    // If the GPS Auto flag is not provided default it to N
                    if (driverStateLineProcess.GPSAutoFlag == null)
                    {
                        driverStateLineProcess.GPSAutoFlag = Constants.No;
                    }

                    //Note that we may not have any container numbers at the state line crossing because the driver may not have
                    //loaded any containers (on the app) before he left the yard.
                    //The power id field in the ContainerMaster determines what containers are on the power unit.
                    var containersOnPowerId = Common.GetContainersForPowerId(dataService, settings, userCulture, userRoleIds,
                                                  driverStateLineProcess.PowerId, out fault);
                    ////////////////////////////////////////////////
                    //Normally for state line crossings, there is an open-ended mileage record, which was started when the driver went enroute.
                    //Get the last open-ended trip segment mileage 
                    var tripSegmentMileage = Common.GetTripSegmentMileageOpenEndedLast(dataService, settings, userCulture, userRoleIds,
                                             driverStateLineProcess.TripNumber, driverStateLineProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //Before we update start a new TripSegmentMileage record
                    var newTripSegmentMileage = new TripSegmentMileage();
                    newTripSegmentMileage.TripNumber = currentTripSegment.TripNumber;
                    newTripSegmentMileage.TripSegNumber = currentTripSegment.TripSegNumber;
                    newTripSegmentMileage.TripSegMileageState = driverStateLineProcess.State;
                    newTripSegmentMileage.TripSegMileageCountry = driverStateLineProcess.Country;
                    newTripSegmentMileage.TripSegMileageOdometerStart = driverStateLineProcess.Odometer;

                    newTripSegmentMileage.TripSegLoadedFlag = Common.GetSegmentLoadedFlag(currentTripSegment, containersOnPowerId);
                    newTripSegmentMileage.TripSegMileagePowerId = driverStateLineProcess.PowerId;
                    newTripSegmentMileage.TripSegMileageDriverId = driverStateLineProcess.EmployeeId;
                    newTripSegmentMileage.TripSegMileageDriverName = Common.GetEmployeeName(employeeMaster);

                    //Pass in false to not update ending odometer. 
                    log.DebugFormat("SRTEST:Adding TripSegmentMileage Record for Trip:{0}-{1} State:{2} - StateLine.",
                                    driverStateLineProcess.TripNumber, driverStateLineProcess.TripSegNumber, driverStateLineProcess.State);
                    if(!Common.InsertTripSegmentMileage(dataService, settings, userRoleIds, userCulture, log,
                        newTripSegmentMileage, ++tripSegmentMileageCount, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertTripSegmentMileage failed: {0} during state line request: {1}", fault.Message, driverStateLineProcess);
                        break;
                    }
                    //Now do the update on the open ended delay
                    if (null != tripSegmentMileage)
                    {
                        //For now always close out the open ended delay
                        //State must be different than previously recorded.
                        //if (tripSegmentMileage.TripSegMileageState != driverStateLineProcess.State)
                        //{
                        tripSegmentMileage.TripSegMileageOdometerEnd = driverStateLineProcess.Odometer;

                        //Do the update
                        //ToDo: This throws the Exception getting single or default.
                        changeSetResult = Common.UpdateTripSegmentMileage(dataService, settings, tripSegmentMileage);
                        log.DebugFormat("SRTEST:Saving TripSegmentMileage Record for Trip:{0}-{1} State:{2} - StateLine.",
                                        driverStateLineProcess.TripNumber, driverStateLineProcess.TripSegNumber, driverStateLineProcess.State);
                        if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
                        {
                            var s = string.Format("DriverStateLineProcess:Could not update TripSegmentMileage for Trip:{0}-{1} State:{2}.",
                                  driverStateLineProcess.TripNumber, driverStateLineProcess.TripSegNumber, driverStateLineProcess.State);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                        //}
                    }

                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. 
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                  driverStateLineProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverStateLineProcess:Invalid DriverId: " + driverStateLineProcess.EmployeeId));
                        break;
                    }
                    driverStatus.Status = DriverStatusSRConstants.StateLine;
                    driverStatus.PrevDriverStatus = driverStatus.Status;
                    driverStatus.TripNumber = driverStateLineProcess.TripNumber;
                    driverStatus.TripSegNumber = driverStateLineProcess.TripSegNumber;
                    driverStatus.TripSegType = currentTripSegment.TripSegType;
                    driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                    driverStatus.TripStatus = currentTrip.TripStatus;
                    driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                    driverStatus.PowerId = driverStateLineProcess.PowerId;
                    driverStatus.MDTId = driverStateLineProcess.Mdtid;
                    driverStatus.ActionDateTime = driverStateLineProcess.ActionDateTime;
                    driverStatus.Odometer = driverStateLineProcess.Odometer;
                    driverStatus.GPSAutoGeneratedFlag = driverStateLineProcess.GPSAutoFlag;
                    if (null == driverStateLineProcess.Latitude || null == driverStateLineProcess.Longitude)
                    {
                        driverStatus.GPSXmitFlag = Constants.No;
                    }
                    else
                    {
                        driverStatus.GPSXmitFlag = Constants.Yes;
                    }

                    //Do the update
                    changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                    log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - StateLine.",
                                    driverStatus.EmployeeId);
                    if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                    {
                        var s = string.Format("DriverStateLineProcess:Could not update DriverStatus for DriverId:{0}.",
                            driverStatus.EmployeeId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }


                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table.
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during StateLine request: {1}", fault.Message, driverStateLineProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 
                    powerMaster.PowerOdometer = driverStateLineProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverStateLineProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverStateLineProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverStateLineProcess.TripSegNumber;
                    powerMaster.PowerCurrentTripSegType = currentTripSegment.TripSegType;

                    //Set these to the destination. Power Unit is on the move.
                    powerMaster.PowerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = currentTripSegment.TripSegDestCustType;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - StateLine.",
                                    driverStateLineProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("DriverStateLineProcess:Could not update PowerMaster for PowerId:{0}.",
                                              driverStateLineProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, destCustomerMaster,
                        ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerHistory failed: {0} during enorute request: {1}", fault.Message, driverStateLineProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – StateLine. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverStateLine);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverStateLineProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverStateLineProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverStateLineProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverStateLineProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverStateLineProcess.PowerId);
                    sbComment.Append(" State:");
                    sbComment.Append(driverStateLineProcess.State);
                    sbComment.Append(" Odom:");
                    sbComment.Append(driverStateLineProcess.Odometer);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverStateLineProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverStateLineProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - StateLine");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("DriverStateLineProcess:Could not update EventLog for Driver {0} {1}.",
                                driverStateLineProcess.EmployeeId, EventCommentConstants.ReceivedDriverStateLine);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // TODO: Check if trip is out of order. Is this necessary?
                    // If trip is out or order, resequence his trips.
                    // Normally we would then send a resequence trips message to  driver.


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
                log.Debug("SRTEST:Transaction Rollback - StateLine");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - StateLine");
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
