﻿using AutoMapper;
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
using log4net;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver sending new container information.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverNewContainerProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverNewContainerProcess")]
    public class DriverNewContainerProcessRecordType : ChangeableRecordType
        <DriverNewContainerProcess, string, DriverNewContainerProcessValidator, DriverNewContainerProcessDeletionValidator>
    {
        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverNewContainerProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverNewContainerProcess, DriverNewContainerProcess>();
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
                        ChangeSet<string, DriverNewContainerProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver new container processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverNewContainerProcess> changeSet, ProcessChangeSetSettings settings)
        {
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
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

                    var driverNewContainerProcess = (DriverNewContainerProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverNewContainerProcess backfillDriverNewContainerProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverNewContainerProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverNewContainerProcess, driverNewContainerProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process new container for DriverId: "
                            + driverNewContainerProcess.EmployeeId + "Container: " + driverNewContainerProcess.ContainerNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //DriverNewContainerProcess has been called
                    log.DebugFormat("SRTEST:DriverNewContainerProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverNewContainerProcess Driver:{0} DT:{1} Container#:{2} Type:{3} Size:{4} BarCode:{5}",
                                     driverNewContainerProcess.EmployeeId, driverNewContainerProcess.ActionDateTime,
                                     driverNewContainerProcess.ContainerNumber, driverNewContainerProcess.ContainerType,
                                     driverNewContainerProcess.ContainerSize, driverNewContainerProcess.ContainerBarcode);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverNewContainerProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: "
                                        + driverNewContainerProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Container record. Container Number in the table is the incoming bar code number.
                    // We are about to change the Container number to the incoming container number.
                    var containerMaster = Common.GetContainer(dataService, settings, userCulture, userRoleIds,
                                                    driverNewContainerProcess.ContainerBarcode, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == containerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ContainerNumber: "
                                        + driverNewContainerProcess.ContainerNumber));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //If container number is the same, then driver is only setting type and size
                    //The preference to set container number is set to N
                    if (containerMaster.ContainerNumber == driverNewContainerProcess.ContainerNumber)
                    {
                        //If all we are doing is changing type and size, we can update the record
                        containerMaster.ContainerType = driverNewContainerProcess.ContainerType;
                        containerMaster.ContainerSize = driverNewContainerProcess.ContainerSize;

                        //Do the update
                        changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                        log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - NewContainer.",
                                        driverNewContainerProcess.ContainerNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                        {
                            var s = string.Format("DriverNewContainerProcess:Could not update ContainerMaster for ContainerNumber:{0}.",
                                     driverNewContainerProcess.ContainerNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                        ////////////////////////////////////////////////
                        // Get any Container History records. Container Number in the table is the incoming bar code number.
                        // We are about to change the Container number to the incoming container number.
                        var containerHistory = Common.GetContainerHistory(dataService, settings, userCulture, userRoleIds,
                                                driverNewContainerProcess.ContainerBarcode, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null != containerHistory && containerHistory.Count() > 0)
                        {
                            foreach (var container in containerHistory)
                            {
                                //If all we are doing is changing type and size, we can update the record
                                container.ContainerType = driverNewContainerProcess.ContainerType;
                                container.ContainerSize = driverNewContainerProcess.ContainerSize;

                                //Do the update
                                changeSetResult = Common.UpdateContainerHistory(dataService, settings, container);
                                log.DebugFormat("SRTEST:Saving ContainerHistory Record for ContainerNumber:{0} Seq:{1} - NewContainer.",
                                                container.ContainerNumber, container.ContainerSeqNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                                {
                                    var s = string.Format("DriverNewContainerProcess:Could not update ContainerHistory for ContainerNumber:{0} Seq:{1}.",
                                             container.ContainerNumber, container.ContainerSeqNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }

                            }//end of foreach (var container in containerHistory)
                        }//end of if (containerHistory.Count() > 0)
                    }//end of if (null == driverNewContainerProcess.ContainerNumber)
                    else
                    {
                        ////////////////////////////////////////////////
                        //In the case where container number has changed, since it is the primary key, we must
                        //insert a new record and delete the old.
                        ////////////////////////////////////////////////
                        //Delete the original record.
                        changeSetResult = Common.DeleteContainerMaster(dataService, settings, containerMaster);
                        log.DebugFormat("SRTEST:Deleting ContainerMaster Record for Container:{0}- NewContainer.",
                                        containerMaster.ContainerNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                        {
                            var s = string.Format("Could not delete ContainerMaster for Container:{0}.",
                                    containerMaster.ContainerNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }


                        ////////////////////////////////////////////////
                        //Insert a new record
                        string originalContainerNumber = containerMaster.ContainerNumber;
                        //Set the newly assigned ContainerNumber
                        containerMaster.ContainerNumber = driverNewContainerProcess.ContainerNumber;
                        //Set the newly assigned Type and size
                        containerMaster.ContainerType = driverNewContainerProcess.ContainerType;
                        containerMaster.ContainerSize = driverNewContainerProcess.ContainerSize;

                        //Set the comments. This is new.
                        containerMaster.ContainerComments = string.Format("Changed from NB# {0}", originalContainerNumber);

                        //Do the insert
                        log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - NewContainer.",
                                         containerMaster.ContainerNumber);
                        if (!Common.InsertContainerMaster(dataService, settings, userRoleIds, userCulture, log, containerMaster))
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to insert Container Master"));
                            log.ErrorFormat("InsertContainerMaster failed during new container request: {0}", 
                                             driverNewContainerProcess);
                            break;
                        }

                        ////////////////////////////////////////////////
                        //Do not need to add a container history record even though current SR does it.
                        //Container history is supposed to show movement of the container, not changes.
                        //If we did add one, it would have be done before we changed the number, in order to 
                        //obtain the next sequence number since nothing is commited to the database until the end 
                        //and we will be changing container history records as well.
                        //The event log will show the changes.

                        ////////////////////////////////////////////////
                        // Get any Container History records. Container Number in the table is the incoming bar code number.
                        // We are about to change the Container number to the incoming container number.
                        var containerHistory = Common.GetContainerHistory(dataService, settings, userCulture, userRoleIds,
                                                driverNewContainerProcess.ContainerBarcode, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null != containerHistory && containerHistory.Count() > 0)
                        {
                            foreach (var container in containerHistory)
                            {
                                ////////////////////////////////////////////////
                                //Do the delete. Deleting records with composite keys is now fixed
                                changeSetResult = Common.DeleteContainerHistory(dataService, settings, container);
                                log.DebugFormat("SRTEST:Deleting ContainerHistory Record for Container:{0} Seq:{1}- NewContainer.",
                                                container.ContainerNumber, container.ContainerSeqNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, container, log))
                                {
                                    var s = string.Format("Could not delete ContainerHistory for Container:{0} Seq:{1}.",
                                            container.ContainerNumber, container.ContainerSeqNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }

                                //Set the newly assigned ContainerNumber
                                container.ContainerNumber = driverNewContainerProcess.ContainerNumber;
                                //Set the newly assigned Type and size
                                container.ContainerType = driverNewContainerProcess.ContainerType;
                                container.ContainerSize = driverNewContainerProcess.ContainerSize;

                                //Set the comments. This is new.
                                container.ContainerComments = string.Format("Changed from NB# {0}", originalContainerNumber);

                                //Do the insert
                                log.DebugFormat("SRTEST:Saving ContainerHistory Record for ContainerNumber:{0} Seq:{1} - NewContainer.",
                                               container.ContainerNumber, container.ContainerSeqNumber);
                                if (!Common.InsertContainerHistory(dataService, settings, userRoleIds, userCulture, log, container))
                                {
                                    var s = string.Format("Could not insert ContainerHistory for ContainerNumber:{0}.",
                                        container.ContainerNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }

                            }//end of foreach (var container in containerHistory)
                        }//end of if (containerHistory.Count() > 0)
                    }////end of else (null == driverNewContainerProcess.ContainerNumber)

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – New Container. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverNewContainer);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverNewContainerProcess.ActionDateTime);
                    sbComment.Append(" Cont:");
                    sbComment.Append(containerMaster.ContainerNumber);
                    sbComment.Append(" ");
                    sbComment.Append(driverNewContainerProcess.ContainerType);
                    sbComment.Append("-");
                    sbComment.Append(driverNewContainerProcess.ContainerSize);
                    sbComment.Append(" Barcode:");
                    sbComment.Append(driverNewContainerProcess.ContainerBarcode);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverNewContainerProcess.EmployeeId);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverNewContainerProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = null,
                        // EventEmployeeName = null,
                        // EventTripNumber = null,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - New Container");
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("Could not update EventLog for Driver {0} Container {1}.",
                                driverNewContainerProcess.EmployeeId, driverNewContainerProcess.ContainerNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }


                }//end of foreach...
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
                log.Debug("SRTEST:Transaction Rollback - New Container");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - New Container");
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
