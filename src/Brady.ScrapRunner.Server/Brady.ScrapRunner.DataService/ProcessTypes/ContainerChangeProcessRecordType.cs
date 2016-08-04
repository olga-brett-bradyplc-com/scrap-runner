using AutoMapper;
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
    /// Get the relevant client container changes for the driver based on the last action date.  Call this process "withoutrequery". 
    /// </summary>
    ///
    /// Call this service using the form PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/ContainerChangeProcess/001/withoutrequery 
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    [EditAction("ContainerChangeProcess")]
    public class ContainerChangeProcessRecordType : ChangeableRecordType
            <ContainerChangeProcess, string, ContainerChangeProcessValidator, ContainerChangeProcessDeletionValidator>
    {

        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(ContainerChangeProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<ContainerChangeProcess, ContainerChangeProcess>();
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
            ChangeSet<string, ContainerChangeProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// Perform the container change processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
            ChangeSet<string, ContainerChangeProcess> changeSet, ProcessChangeSetSettings settings)
        {
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted
            // then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository)repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security, pipelines, etc.  Note however, we will lose non-persisted user input values which 
            // will not be propagated through into the changeSetResult.
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only respond to one request at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() && !changeSetResult.FailedDeletions.Any())
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

                    ContainerChangeProcess containersProcess = (ContainerChangeProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    ContainerChangeProcess backfillContainerChangeProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillContainerChangeProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillContainerChangeProcess, containersProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process containerchange for Driver ID " + containersProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //ContainerChangeProcess has been called
                    log.DebugFormat("SRTEST:ContainerChangeProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:ContainerChangeProcess Driver:{0} ContainerLastActionDateTime:{1}",
                                     containersProcess.EmployeeId, containersProcess.LastContainerMasterUpdate);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  containersProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + containersProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////////////
                    // Lookup Preference: DEFAllowAnyContainer
                    string prefAllowAnyContainer = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                  Constants.SystemTerminalId, PrefSystemConstants.DEFAllowAnyContainer, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //To contain the list of container
                    List<ContainerChange> containerChangeList = new List<ContainerChange>();
                    ////////////////////////////////////////////////////////
                    // Lookup container changes or entire container master
                    if (containersProcess.LastContainerMasterUpdate == null)
                    {
                        //If no date is provided, get the entire Container list
                        // from the ContainerMaster table instead of the ContainerChange table.
                        //Since the date is provided, get the container changes since the last container update was provided to this driver.
                        List<ContainerMaster> containerMasterList;
                        if (prefAllowAnyContainer == Constants.Yes)
                        {
                            containerMasterList = Common.GetContainerMasterAll(dataService, settings, userCulture, userRoleIds,
                                                  out fault);
                        }
                        else
                        {
                            containerMasterList = Common.GetContainerMasterForRegion(dataService, settings, userCulture, userRoleIds,
                                                  employeeMaster.RegionId, out fault);
                        }
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }

                        foreach (var container in containerMasterList)
                        {
                            var containerChange = new ContainerChange();

                            containerChange.ContainerNumber = container.ContainerNumber;
                            containerChange.ContainerType = container.ContainerType;
                            containerChange.ContainerSize = container.ContainerSize;
                            containerChange.ActionDate = container.ContainerLastActionDateTime;
                            containerChange.ActionFlag = ContainerChangeConstants.Add;
                            containerChange.TerminalId = container.ContainerTerminalId;
                            containerChange.RegionId = container.ContainerRegionId;
                            containerChange.ContainerBarCodeNo = container.ContainerBarCodeNo;
                            containerChangeList.Add(containerChange);
                        }
                    }
                    else
                    {
                        //Since the date is provided, get the container changes since the last container update was provided to this driver.
                        if (prefAllowAnyContainer == Constants.Yes)
                        {
                            containerChangeList = Common.GetContainerChangesAll(dataService, settings, userCulture, userRoleIds,
                              containersProcess.LastContainerMasterUpdate, out fault);
                        }
                        else
                        {
                            containerChangeList = Common.GetContainerChangesForRegion(dataService, settings, userCulture, userRoleIds,
                              containersProcess.LastContainerMasterUpdate, employeeMaster.RegionId, out fault);
                        }
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                    }

                    // Don't forget to actually backfill the ContainerProcess object contained within 
                    // the ChangeSetResult that exits this method and is returned to the caller.
                    containersProcess.Containers = containerChangeList;

                    //Now using log4net.ILog implementation to test results of query.
                    log.DebugFormat("SRTEST:ContainerChangeProcess sending {0} containers.",
                                     containerChangeList.Count());
                   // foreach (var container in containerChangeList)
                   // {
                   //     log.DebugFormat("SRTEST:ContainerChangeProcess:ContainerNumber:{0} Type:{1} Size:{2} Date:{3} Flag:{4} TerminalId:{5} BarCode:{6}",
                   //                     container.ContainerNumber,
                   //                     container.ContainerType,
                   //                     container.ContainerSize,
                   //                     container.ActionDate,
                   //                     container.ActionFlag,
                   //                     container.TerminalId,
                   //                     container.ContainerBarCodeNo);
                   // }

                    ////////////////////////////////////////////////
                    //Now update the ContainerMasterDateTime in the DriverStatus record
                    //If there is no record yet, I guess we will have to add one.
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                containersProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    // If no driver status record is found create one to add
                    if (null == driverStatus)
                    {
                        driverStatus = new DriverStatus()
                        {
                            EmployeeId = containersProcess.EmployeeId,
                        };
                    }
                    //Now there is a driverStatus
                    driverStatus.ContainerMasterDateTime = DateTime.Now;
                    ChangeSetResult<string> scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                    log.DebugFormat("SRTEST:ContainerChangeProcess Saving DriverStatus Record: DriverId:{0} ContainerMasterDateTime:{1}",
                                     driverStatus.EmployeeId,
                                     driverStatus.ContainerMasterDateTime);

                    if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                    {
                        var s = string.Format("ContainerChangeProcess:Update DriverStatus failed for Driver {0}.",
                                               containersProcess.EmployeeId);
                        scratchChangeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
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
                log.Debug("SRTEST:ContainerChangeProcess:Transaction Rollback - Container Updates");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:ContainerChangeProcess:Transaction Committed - Container Updates");
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
