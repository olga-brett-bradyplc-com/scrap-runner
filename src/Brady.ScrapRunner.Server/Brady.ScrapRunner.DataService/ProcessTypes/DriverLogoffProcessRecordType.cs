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
using Brady.ScrapRunner.Domain.Enums;
using System.IO;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver logging off.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverLogoffProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverLogoffProcess")]
    public class DriverLogoffProcessRecordType : ChangeableRecordType
        <DriverLogoffProcess, string, DriverLogoffProcessValidator, DriverLogoffProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverLogoffProcess, DriverLogoffProcess>();
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
                        ChangeSet<string, DriverLogoffProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver logoff processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverLogoffProcess> changeSet, ProcessChangeSetSettings settings)
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
                    int powerHistoryInsertCount = 0;

                    var driverLogoffProcess = (DriverLogoffProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverLogoffProcess backfillDriverLogoffProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverLogoffProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverLogoffProcess, driverLogoffProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverLogoffProcess:Unable to process logoff for DriverId: " + driverLogoffProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //DriverLogoffProcess has been called
                    log.DebugFormat("SRTEST:DriverLogoffProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverLogoffProcess Driver:{0} DT:{1} PowerId:{2} Odom:{3}",
                                     driverLogoffProcess.EmployeeId,  driverLogoffProcess.ActionDateTime,
                                     driverLogoffProcess.PowerId, driverLogoffProcess.Odometer);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverLogoffProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverLogoffProcess:Invalid DriverId: "
                                        + driverLogoffProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the PowerMaster record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                    driverLogoffProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverLogoffProcess:Invalid PowerId: "
                                        + driverLogoffProcess.PowerId));
                        break;
                    }


                    ////////////////////////////////////////////////
                    //ToDo: Check for Open ended delay and close it
                    //ToDo: Delete any other open-ended delays found

                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. 
                    //Currently a logoff from the mobile app does not include any trip information.
                    //It is important to leave any existing trip information in the DriverStatus table.
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                       driverLogoffProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " + driverLogoffProcess.EmployeeId));
                        break;
                    }
                    //Save the original driver status in the previous driver status
                    driverStatus.PrevDriverStatus = driverStatus.Status;
                    //Set the current driver status to (O) Logged Out
                    driverStatus.Status = DriverStatusSRConstants.LoggedOut;
                    //Set the action date/time to the logged off date/time
                    driverStatus.ActionDateTime = driverLogoffProcess.ActionDateTime;
                    //Set the power id
                    driverStatus.PowerId = driverLogoffProcess.PowerId;
                    //If we receive an odometer and it is greater than the one we currently have, update it.
                    if (driverLogoffProcess.Odometer != null &&
                        driverLogoffProcess.Odometer > driverStatus.Odometer)
                    {
                        driverStatus.Odometer = driverLogoffProcess.Odometer;
                    }

                    //Remove MDTId
                    driverStatus.MDTId = null;
                    //Set GPSXmitFlag to N
                    driverStatus.GPSXmitFlag = Constants.No;
                    //Set GPSAutoGeneratedFlag to N
                    driverStatus.GPSAutoGeneratedFlag = Constants.No;
                    //Remove Delay Code
                    driverStatus.DelayCode = null;
                    //Set SendHHLogoffFlag to 0 (Not Ready)
                    driverStatus.SendHHLogoffFlag = DriverForceLogoffValue.NotReady;

                    //ToDo: Not sure what to do with this yet
                    //driverStatus.DriverCumMinutes;

                    TripSegment currentTripSegment = null;
                    if (driverStatus.TripNumber != null && driverStatus.TripSegNumber != null)
                    {
                        ////////////////////////////////////////////////
                        //Get a list of all segments for the trip
                        var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                                            driverStatus.TripNumber, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }

                        ////////////////////////////////////////////////
                        // Get the current TripSegment record
                        currentTripSegment = (from item in tripSegList
                                                  where item.TripSegNumber == driverStatus.TripSegNumber
                                                  select item).FirstOrDefault();
                        if (null == currentTripSegment)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverArriveProcess:Invalid TripSegment: " +
                                driverStatus.TripNumber + "-" + driverStatus.TripSegNumber));
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Add the logoff record to the DriverHistory table.
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during Logoff request: {1}", fault.Message, driverLogoffProcess);
                        break;
                    }
                    ////////////////////////////////////////////////////
                    // Check to see if the driver has more incomplete trips asigned to him
                    // Get the list of trips for driver
                    var tripList = Common.GetTripsForDriverAtLogin(dataService, settings, userCulture, userRoleIds,
                                   driverLogoffProcess.EmployeeId, out fault);

                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    if (tripList == null || tripList.Count() == 0)
                    {
                        //There are no more trips for this driver, so remove the driver from the DriverStatus table

                        //Do the delete. 
                        changeSetResult = Common.DeleteDriverStatus(dataService, settings, driverStatus);
                        log.DebugFormat("SRTEST:Deleting DriverDelay Record for DriverId:{0} - Logoff.",
                                        driverStatus.EmployeeId);
                        if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                        {
                            var s = string.Format("DriverLogoffProcess:Could not delete DriverStatus for DriverId:{0}.",
                                    driverStatus.EmployeeId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                    }
                    else
                    {
                        //Driver has more trips so change his status to ready
                        driverStatus.Status = DriverStatusSRConstants.Ready;

                        //Remove these values
                        driverStatus.PrevDriverStatus = null;
                        driverStatus.PowerId = null;
                        driverStatus.MDTId = null;
                        driverStatus.RFIDFlag = null;
                        driverStatus.RouteTo = null;
                        driverStatus.DriverLCID = null;

                        //Do the update
                        changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                        log.DebugFormat("SRTEST:DriverLogoffProcess:Saving DriverStatus Record for DriverId:{0} - Logoff.",
                                        driverStatus.EmployeeId);
                        if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                        {
                            var s = string.Format("DriverLogoffProcess:Could not update DriverStatus for DriverId:{0}.",
                                driverStatus.EmployeeId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        //Do not add the ready record to the DriverHistory table.
                        ////////////////////////////////////////////////

                    }


                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 
                    //Power Unit is now available fr use
                    powerMaster.PowerStatus = PowerStatusConstants.Available;
                    //Set the last action date/time
                    powerMaster.PowerLastActionDateTime = driverLogoffProcess.ActionDateTime;

                    //If we receive an odometer and it is greater than the one we currently have, update it.
                    if (driverLogoffProcess.Odometer != null &&
                        driverLogoffProcess.Odometer > driverStatus.Odometer)
                    {
                        powerMaster.PowerOdometer = driverLogoffProcess.Odometer;
                    }

                    //Remove these values
                    powerMaster.PowerDriverId = null;
                    powerMaster.PowerCurrentTripNumber = null;
                    powerMaster.PowerCurrentTripSegNumber = null;
                    powerMaster.PowerCurrentTripSegType = null;
                    powerMaster.PowerCustHostCode = null;
                    powerMaster.PowerCustType = null;
                    powerMaster.PowerLocation = null;
                    powerMaster.PowerComments = null;

                    //ToDo:And these ??
                    powerMaster.PowerDateInService = null;
                    powerMaster.PowerDateOutOfService = null;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:DriverLogoffProcess:Saving PowerMaster Record for PowerId:{0} - Logoff.",
                                    driverLogoffProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("DriverLogoffProcess:Could not update PowerMaster for PowerId:{0}.",
                                              driverLogoffProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add record to PowerHistory table. 
                    if (!Common.InsertPowerHistory(dataService, settings, powerMaster, employeeMaster, null,
                        ++powerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertPowerHistory failed: {0} during logoff request: {1}", fault.Message, driverLogoffProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Send END information to tracker
                    if (!SendToTracker(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                      driverLogoffProcess))
                    {
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Logoff. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverLogoff);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverLogoffProcess.EmployeeId);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverLogoffProcess.ActionDateTime);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverLogoffProcess.PowerId);
                    sbComment.Append(" Odom:");
                    sbComment.Append(driverLogoffProcess.Odometer);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverLogoffProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        //EventTripNumber = driverLogoffProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:DriverLogoffProcess:Saving EventLog Record - Logoff");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("DriverLogoffProcess:Could not update EventLog for Driver {0} {1}.",
                                driverLogoffProcess.EmployeeId, EventCommentConstants.ReceivedDriverLogoff);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
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
                log.Debug("SRTEST:DriverLogoffProcess:Transaction Rollback - Logoff");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:DriverLogoffProcess:Transaction Committed - Logoff");
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
        /// Send END packet to tracker
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverLogoffProcess"></param>
        /// <returns></returns>
        private bool SendToTracker(IDataService dataService, ProcessChangeSetSettings settings,
                   ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
                   DriverLogoffProcess driverLogoffProcess)
        {
            DataServiceFault fault;

            ////////////////////////////////////////////////////////
            // Lookup Preference: DEFRouterPath
            string prefRouterPath = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                          Constants.SystemTerminalId, PrefSystemConstants.DEFRouterPath, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
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
                string mdtId = driverLogoffProcess.EmployeeId;
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
                        // GV END DriverId

                        // Send GV Driver END Packet to tracker
                        //*T TO TRACKER FR MdtId
                        //*DS:3,GV 
                        //*DC:3,END
                        //*END
                        outputFile.WriteLine($"*T TO {Constants.Tracker} FR {mdtId}");
                        outputFile.WriteLine($"*DS:3,GV");
                        outputFile.WriteLine($"*DC:4,END");
                        outputFile.WriteLine($"*DC:10,{driverLogoffProcess.EmployeeId}");
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
                    log.DebugFormat("SRTEST:DriverLogoffProcess Write failed: {0}.", e.Message);
                }

            }//if (prefRouterPath != null)
            else
            {
                log.DebugFormat("SRTEST:DriverLogoffProcess Router Path no set up.");
            }
            return true;
        }
    }
}
