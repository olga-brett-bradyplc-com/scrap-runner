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

                    var driverMessageProcess = (DriverMessageProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

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
                    //ToDo: How do we know if this is a request from the driver for messages,
                    //or an actual message from a driver?
                    //Perhaps if there is only the EmployeeId and nothing else, then we send.
                    if (driverMessageProcess.SenderId == null)
                    {
                        //Sending message(s)
                        if (!SendMessage(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                    driverMessageProcess, employeeMaster))
                        {
                            var s = string.Format("DriverMessageProcess:Could not Send Message for Driver:{0}.",
                                                  driverMessageProcess.EmployeeId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    else
                    {
                        //Receiving message(s)
                        if (!ProcessMessage(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                    driverMessageProcess, employeeMaster))
                        {
                            var s = string.Format("DriverMessageProcess:Could not Process Message for Driver:{0}.",
                                                  driverMessageProcess.EmployeeId);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
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

            return changeSetResult;
        }
        /// <summary>
        /// Method to send messages to a driver
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="key"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverMessageProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <returns></returns>
        public bool SendMessage(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, string key, IEnumerable<long> userRoleIds, string userCulture,
           DriverMessageProcess driverMessageProcess, EmployeeMaster employeeMaster)
        {
            DataServiceFault fault = null;
            string msgKey = key;
            ChangeSetResult<int> scratchChangeSetResult;

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
                scratchChangeSetResult = Common.UpdateMessages(dataService, settings, message);
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
        public bool ProcessMessage(IDataService dataService, ProcessChangeSetSettings settings,
                   ChangeSetResult<string> changeSetResult, String key, IEnumerable<long> userRoleIds, string userCulture,
                   DriverMessageProcess driverMessageProcess, EmployeeMaster employeeMaster)
        {
            DataServiceFault fault = null;
            string msgKey = key;

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

            ////////////////////////////////////////////////
            // Process the Message record
            var newMessage = new Messages();
            newMessage.MsgId = 0;
            newMessage.TerminalId = employeeMaster.TerminalId;
            newMessage.CreateDateTime = driverMessageProcess.ActionDateTime;
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

            Common.InsertMessage(dataService, settings,
                                 userRoleIds,userCulture,log,newMessage,out fault);
            log.DebugFormat("SRTEST:Saving message received from DriverId:{0} To:{1}-Message:{2}.",
                            newMessage.SenderId.Trim(),newMessage.ReceiverId.Trim(), newMessage.MsgId);
            //ToDo: Check for Message Record failure.
            return true;
        }
    }
}

