using AutoMapper;
using System;
using System.Linq;
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
using log4net;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver arriving.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverGPSLocationProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverGPSLocationProcess")]
    public class DriverGPSLocationProcessRecordType : ChangeableRecordType
        <DriverGPSLocationProcess, string, DriverGPSLocationProcessValidator, DriverGPSLocationProcessDeletionValidator>
    {
        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverGPSLocationProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverGPSLocationProcess, DriverGPSLocationProcess>();
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
                        ChangeSet<string, DriverGPSLocationProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver arrive processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverGPSLocationProcess> changeSet, ProcessChangeSetSettings settings)
        {
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
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

                    var driverGPSLocationProcess = (DriverGPSLocationProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverGPSLocationProcess backfillDriverGPSLocationProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverGPSLocationProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverGPSLocationProcess, driverGPSLocationProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process arrive for DriverId: "
                                        + driverGPSLocationProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //DriverGPSLocationProcess has been called
                    log.DebugFormat("SRTEST:DriverGPSLocationProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverGPSLocationProcess Driver:{0} GPSID:{1} DT:{2} Lat:{3} Lon:{4} Speed:{5} Heading:{6}",
                                     driverGPSLocationProcess.EmployeeId, driverGPSLocationProcess.GPSID,
                                     driverGPSLocationProcess.ActionDateTime, driverGPSLocationProcess.Latitude,
                                     driverGPSLocationProcess.Longitude, driverGPSLocationProcess.Speed,
                                     driverGPSLocationProcess.Heading);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverGPSLocationProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverGPSLocationProcess:Invalid DriverId: "
                                        + driverGPSLocationProcess.EmployeeId));
                        break;
                    }


                    ////////////////////////////////////////////////////////
                    // Lookup Preference: DEFRouterPath
                    string prefRouterPath = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                  Constants.SystemTerminalId, PrefSystemConstants.DEFRouterPath, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    ////////////////////////////////////////////////////////
                    // Lookup Preference: DEFMDTPrefix
                    string prefMdtPrefix = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                  Constants.SystemTerminalId, PrefSystemConstants.DEFMDTPrefix, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    var gpsLocation = new GPSLocation();

                    gpsLocation.EmployeeId = driverGPSLocationProcess.EmployeeId;
                    gpsLocation.TerminalId = employeeMaster.TerminalId;
                    gpsLocation.RegionId = employeeMaster.RegionId;
                    gpsLocation.GPSID = driverGPSLocationProcess.GPSID;
                    gpsLocation.GPSDateTime = driverGPSLocationProcess.ActionDateTime;
                    gpsLocation.GPSLatitude = driverGPSLocationProcess.Latitude;
                    gpsLocation.GPSLongitude = driverGPSLocationProcess.Longitude;
                    gpsLocation.GPSSpeed = driverGPSLocationProcess.Speed;
                    gpsLocation.GPSHeading = driverGPSLocationProcess.Heading;
                    //Set the flag to show that the packet is ready to be sent to tracker
                    gpsLocation.GPSSendFlag = GPSSendFlagValue.Ready;

                    //Router path must be specified in system preference or we cannot send the packet.
                    if (prefRouterPath != null)
                    {
                        //Create packet to send GPS Location to tracker:
                        //*T TO TRACKER FR MdtId
                        //*DC:2,GG
                        //*DI:2,1
                        //*DI:1,year(2 Char) from GPSDateTime
                        //*DI:1,month from GPSDateTime
                        //*DI:1,day from GPSDateTime
                        //*DI:1,hour from GPSDateTime
                        //*DI:1,minute from GPSDateTime
                        //*DI:1,second from GPSDateTime
                        //*DI:4,GPSLatitude
                        //*DI:4,GPSLongitude
                        //*DI:2,GPSSpeed
                        //*DI:2,GPSHeading
                        //*END

                        //Build the mdtId
                        string mdtId = driverGPSLocationProcess.EmployeeId;
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
                                outputFile.WriteLine($"*T TO {Constants.Tracker} FR {mdtId}");
                                outputFile.WriteLine($"*DC:2,GG");
                                outputFile.WriteLine($"*DI:2,1");
                                outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.ActionDateTime.Year.ToString().Substring(2)}");
                                outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.ActionDateTime.Month.ToString()}");
                                outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.ActionDateTime.Day.ToString()}");
                                outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.ActionDateTime.Hour.ToString()}");
                                outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.ActionDateTime.Minute.ToString()}");
                                outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.ActionDateTime.Second.ToString()}");
                                outputFile.WriteLine($"*DI:4,{driverGPSLocationProcess.Latitude.ToString()}");
                                outputFile.WriteLine($"*DI:4,{driverGPSLocationProcess.Longitude.ToString()}");
                                //if there is no value for speed, tracker expects a -1 
                                if (driverGPSLocationProcess.Speed == null)
                                {
                                    outputFile.WriteLine($"*DI:1,-1");
                                }
                                else
                                {
                                    outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.Speed.ToString()}");
                                }
                                //if there is no value for Heading, tracker expects a -1 
                                if (driverGPSLocationProcess.Heading == null)
                                {
                                    outputFile.WriteLine($"*DI:1,-1");
                                }
                                else
                                {
                                    outputFile.WriteLine($"*DI:1,{driverGPSLocationProcess.Heading.ToString()}");
                                }
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
                            //Set the flag to show that the packet was sent to tracker
                            gpsLocation.GPSSendFlag = GPSSendFlagValue.SentToTracker;
                        }
                        catch (Exception e)
                        {
                            log.DebugFormat("SRTEST:DriverGPSLocationProcess Write failed: {0}.", e.Message);
                            //Set the flag to show that the packet could not be sent to tracker
                            gpsLocation.GPSSendFlag = GPSSendFlagValue.SendFailed;
                        }

                    }//if (prefRouterPath != null)
                    else
                    {
                        log.DebugFormat("SRTEST:DriverGPSLocationProcess Router Path no set up.");
                        //Set the flag to show that the packet could not be sent to tracker
                        gpsLocation.GPSSendFlag = GPSSendFlagValue.SendFailed;
                    }

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateGPSLocation(dataService, settings, gpsLocation);
                    log.Debug("SRTEST:Saving GPSLocation Record");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, gpsLocation, log))
                    {
                        var s = string.Format("DriverGPSLocationProcess:Could not update GPSLocation for Driver {0} {1}.",
                                driverGPSLocationProcess.EmployeeId, driverGPSLocationProcess.ActionDateTime);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                } //end of foreach...
            }//end of if (!changeSetResult.Failed...

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
                log.Debug("SRTEST:Transaction Rollback - GPSLocation");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - GPSLocation");
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
    }
}