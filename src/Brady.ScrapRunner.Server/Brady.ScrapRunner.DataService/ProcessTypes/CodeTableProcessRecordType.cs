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
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.DataService.Util;
using log4net;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Get the relevant client code table values for the driver.  Call this process "withoutrequery".
    /// </summary>
    /// 
    /// Note this our business processes is relatively independent of the "trivial" backing query.  
    /// call using the form PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery"
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/CodeTableChangeProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    [EditAction("CodeTableProcess")]
    public class CodeTableProcessRecordType : ChangeableRecordType
                <CodeTableProcess, string, CodeTableProcessValidator, CodeTableProcessDeletionValidator>
    {

        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(CodeTableProcess));

        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<CodeTableProcess, CodeTableProcess>();
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
            ChangeSet<string, CodeTableProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
    
        /// <summary>
        /// This is the "real" method implementation.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
            ChangeSet<string, CodeTableProcess> changeSet, ProcessChangeSetSettings settings)
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

            // Running the base process changeset first in this case.
            // This should gain the benefit of validators, auditing, security, piplines, etc.
            // However, it looks like we are losing some user input in the base.ProcessChangeSet
            // from a reretrieve or the sparse NHibernate mapping?
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

                    var codetablesProcess = (CodeTableProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the gernal case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    CodeTableProcess backfillCodeTableProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillCodeTableProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillCodeTableProcess, codetablesProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process codetable for Driver ID " + codetablesProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //CodeTableProcess has been called
                    log.DebugFormat("SRTEST:CodeTableProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:CodeTableProcess Driver:{0}",
                                     codetablesProcess.EmployeeId);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         codetablesProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + codetablesProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Lookup Preference: DEFUseContainerLevel
                    string prefUseContainerLevel = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                   employeeMaster.TerminalId, PrefDriverConstants.DEFUseContainerLevel, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Lookup Preference: DEFCountry
                    string prefDefCountry = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                   employeeMaster.TerminalId, PrefDriverConstants.DEFCountry, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Lookup code tables.  
                    //This query does not include container level
                    var codeTableList = Common.GetCodeTablesForDriver(dataService, settings, userCulture, userRoleIds,
                                    employeeMaster.RegionId, prefDefCountry, prefUseContainerLevel,out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    // Don't forget to actually backfill the CodeTableProcess object contained within 
                    // the ChangeSetResult that exits this method and is returned to the caller.
                    codetablesProcess.CodeTables = codeTableList;
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

            // Capture details of outgoing response too and log at INFO level
            log.Info(RequestResponseUtil.CaptureResponse(changeSetResult, requestRespStrBld));
            return changeSetResult;
        }
    }
}

