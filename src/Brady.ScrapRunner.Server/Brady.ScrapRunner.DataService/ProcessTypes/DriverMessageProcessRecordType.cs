﻿using AutoMapper;
using System;
using System.Collections.Generic;
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
using log4net;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver message.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverMessageProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverMessageProcess")]
    public class DriverMessageProcessRecordType : ChangeableRecordType
        <DriverMessageProcess, string, DriverMessageProcessValidator, DriverMessageProcessDeletionValidator>
    {
        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverMessageProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverMessageProcess, DriverMessageProcess>();
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
                        ChangeSet<string, DriverMessageProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the driver message processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverMessageProcess> changeSet, ProcessChangeSetSettings settings)
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

                    var driverMessageProcess = (DriverMessageProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverMessageProcess backfillDriverMessageProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverMessageProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverMessageProcess, driverMessageProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process message for DriverId: "
                            + driverMessageProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //DriverMessageProcess has been called
                    log.DebugFormat("SRTEST:DriverMessageProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverMessageProcess Driver:{0} DT:{1} SenderId:{2} ReceiverId:{3} MessageId:{4} Thread:{5}",
                                     driverMessageProcess.EmployeeId, driverMessageProcess.ActionDateTime,
                                     driverMessageProcess.SenderId, driverMessageProcess.ReceiverId,
                                     driverMessageProcess.MessageId, driverMessageProcess.MessageThread);


                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                    driverMessageProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverMessageProcess:Invalid DriverId: "
                                        + driverMessageProcess.EmployeeId));
                        break;
                    }

                    //If there is only the EmployeeId and nothing else, then we send.
                    if (driverMessageProcess.SenderId == null)
                    {
                        //Sending message(s)
                        if (!SendMessage(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                    driverMessageProcess))
                        {
                            break;
                        }
                    }
                    else
                    {
                        //Receiving message(s)
                        if (!ProcessMessage(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                    driverMessageProcess, employeeMaster))
                        {
                            break;
                        }
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
                log.Debug("SRTEST:Transaction Rollback - Driver Message");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Driver Message");
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
        /// Method to send messages to a driver
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverMessageProcess"></param>
        /// <returns></returns>
        private bool SendMessage(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, string msgKey, IEnumerable<long> userRoleIds, string userCulture,
           DriverMessageProcess driverMessageProcess)
        {
            DataServiceFault fault;

            //Process will return the following list.
            List<Messages> fullMessageList = new List<Messages>();

            ////////////////////////////////////////////////
            // Get any new messages for driver
            var newMessages = Common.GetMessagesForDriver(dataService, settings, userCulture, userRoleIds,
                                 driverMessageProcess.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            //Save the list for sending to driver
            fullMessageList.AddRange(newMessages);

            //Set the return value
            driverMessageProcess.Messages = fullMessageList;

            foreach (var message in newMessages)
            {
                //Set the processed flag to Y so that the will not be sent to the driver again.
                message.Processed = Constants.Yes;

                //Do the update
                ChangeSetResult<int> scratchChangeSetResult = Common.UpdateMessages(dataService, settings, message);
                log.DebugFormat("SRTEST:Saving Message sent to DriverId:{0} From:{1}-Message:{2}.",
                                message.ReceiverId.Trim(),message.SenderId.Trim(), message.MsgId);
                //Check for Messages failure.
                if (Common.LogChangeSetFailure(scratchChangeSetResult, message, log))
                {
                    var s = string.Format("DriverMessageProcess:Could not update messages for DriverId:{0}.",
                             driverMessageProcess.EmployeeId);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
            }
            return true;
        }

        private bool ProcessMessage(IDataService dataService, ProcessChangeSetSettings settings,
                   ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
                   DriverMessageProcess driverMessageProcess, EmployeeMaster employeeMaster)
        {
            DataServiceFault fault;

            if (driverMessageProcess.ReceiverId == null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("ReceiverId is required from Driver: "
                                + driverMessageProcess.EmployeeId));
                return false;
            }
            var receiverEmpMaster = Common.GetEmployeeMaster(dataService, settings, userCulture, userRoleIds,
                                 driverMessageProcess.ReceiverId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == receiverEmpMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverMessageProcess:Invalid DriverId: "
                                + driverMessageProcess.ReceiverId));
                return false;
            }
            if (driverMessageProcess.ActionDateTime == null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("ActionDateTime is required from Driver: "
                                + driverMessageProcess.EmployeeId));
                return false;
            }
            if (driverMessageProcess.MessageText == null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("MessageText is required from Driver: "
                                + driverMessageProcess.EmployeeId));
                return false;
            }
            if (driverMessageProcess.UrgentFlag == null)
            {
                driverMessageProcess.UrgentFlag = Constants.No;
            }

            ////////////////////////////////////////////////
            // Process the Message record
            var newMessage = new Messages();

            newMessage.MsgId = 0;
            newMessage.TerminalId = employeeMaster.TerminalId;
            newMessage.CreateDateTime = (DateTime)driverMessageProcess.ActionDateTime;
            newMessage.SenderId = driverMessageProcess.EmployeeId;
            newMessage.SenderName = Common.GetEmployeeName(employeeMaster);
            newMessage.ReceiverId = driverMessageProcess.ReceiverId;
            newMessage.ReceiverName = Common.GetEmployeeName(receiverEmpMaster);
            newMessage.MsgText = driverMessageProcess.MessageText;
            newMessage.Ack = Constants.No;
            newMessage.MsgThread = driverMessageProcess.MessageThread;
            //This field is a char(1) in the table. SR currently puts an * in this field.
            newMessage.Area = "*";
            newMessage.Urgent = driverMessageProcess.UrgentFlag;
            newMessage.Processed = Constants.No;
            newMessage.MsgSource = MessagesMsgSourceConstants.FromDriver;
            newMessage.DeleteFlag = Constants.No;

            if(!Common.InsertMessage(dataService, settings,userRoleIds,userCulture,log,newMessage,out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("InsertMessage failed: {0} during process message request: {1}", fault.Message, newMessage);
                return false;
            }
            return true;
        }
    }
}

