using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using NHibernate;
using NHibernate.Util;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    /// <summary>
    /// Get the relevant client preferences for the driver's terminal.  Note this our business processes 
    /// is relatively independent of the "trivial" backing query.  As such, we temporarily need to invoke this
    /// service call using the form Put["/{dataServiceName}/{typeName}/{id}/withoutpersistance", true]
    /// (example: PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/PreferencesProcess/001/withoutpersistance) 
    /// this will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve 
    /// (getSingleAsync()) within the postSingleAsync().   This re-retrieve of a trival query clobbers our post-processed 
    /// ChangeSetResult
    /// </summary>
    [EditAction("PreferencesProcess")]
    public class PreferencesProcessRecordType : ChangeableRecordType
            <PreferencesProcess, string, PreferencesProcessValidator, PreferencesProcessDeletionValidator>
    {


        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<PreferencesProcess, PreferencesProcess>();

            // Note should we ever need to map the nested child list too, 
            // we would need to be more explicit.  see also: 
            // http://stackoverflow.com/questions/9394833/automapper-with-nested-child-list
            // Mapper.CreateMap<PreferencesProcess, PreferencesProcess>()
            //    .ForMember(dest => dest.Preferences, opts => opts.MapFrom(src => src.Preferences));
            // Mapper.CreateMap<Preference, Preference>();
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
            ChangeSet<string, PreferencesProcess> changeSet,
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
            ChangeSet<string, PreferencesProcess> changeSet, ProcessChangeSetSettings settings)
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
            // from a reretrieve or the sparse NHibernate mapping?
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only respond to one request at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() && !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    PreferencesProcess preferencesProcess = (PreferencesProcess) changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the gernal case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    PreferencesProcess backfillPreferencesProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillPreferencesProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillPreferencesProcess, preferencesProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process preferences for Driver ID " + preferencesProcess.EmployeeId));
                        break;
                    }

                    //
                    // Validate driver id / Get the EmployeeMaster
                    //
                    Query query = new Query()
                    {
                        CurrentQuery = string.Format("EmployeeMasters?$filter= EmployeeId='{0}'", preferencesProcess.EmployeeId)
                    };
                    var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (handleFault(changeSetResult, msgKey, fault, preferencesProcess)) { break; }
                    var employeeMaster = (EmployeeMaster) queryResult.Records.Cast<EmployeeMaster>().FirstOrNull();
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + preferencesProcess.EmployeeId));
                        break;
                    }

                    //
                    // Lookup preferences.  
                    //
                    query.CurrentQuery = string.Format("Preferences?$filter= TerminalId='{0}'", employeeMaster.TerminalId);
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (handleFault(changeSetResult, msgKey, fault, preferencesProcess)) { break; }
                    var preferences = queryResult.Records.Cast<Preference>().ToArray();

                    //
                    // Lookup TerminalMaster for two "additional" preferences 
                    //
                    query.CurrentQuery = string.Format("TerminalMasters?$filter= TerminalId='{0}'", employeeMaster.TerminalId);
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (handleFault(changeSetResult, msgKey, fault, preferencesProcess)) { break; }
                    var terminalMaster = (TerminalMaster)queryResult.Records.Cast<TerminalMaster>().FirstOrNull();

                    //
                    // Filter for the 30some properties of interst.
                    // Suppliment with the two additional timezone preferences
                    // Return preferences
                    //
                    string[] propNamesDesired = {
                        "DEFPrevChangContID", 
                        "DEFEnforceSeqProcess", 
                        "DEFUseMaterGrading", 
                        "DEFUseAutoArrive", 
                        "DEFUseDrvAutoDelay", 
                        "DEFDrvAutoDelayTime", 
                        "DEFContMasterValidation", 
                        "DEFCommodSelection", 
                        "DEFDriverAdd", 
                        "DEFDriverReceipt", 
                        "DEFContMasterScannedVal", 
                        "DEFOneContPerPowerUnit", 
                        "DEFDriverEntersGPS", 
                        "DEFDriverReceiptMask", 
                        "DEFDriverReceiptAllTrips", 
                        "DEFDriverWeights", 
                        "DEFShowHostCode", 
                        "DEFUseLitre", 
                        "DEFUseKM", 
                        "DEFUseContainerLevel", 
                        "DEFContainerValidationCount", 
                        "DEFReceiptValidationCount", 
                        "DEFDriverReference", 
                        "DEFDriverReferenceMask", 
                        "DEFReferenceValidationCount", 
                        "DEFCountry", 
                        "DEFAllowAddRT", 
                        "DEFAllowChangeRT", 
                        "DEFPromptRTMsg", 
                        "DEFReqScaleRefNo", 
                        "DEFNotAvlScalRefNo", 
                        "DEFConfirmTruckInvMsg", 
                        "DEFEnableImageCapture", 
                        "DEFAutoDropContainers", 
                        "DEFShowSimilarContainers", 
                        "DEFMinAutoTriggerDone", 
                        "DEFPrevChangContID", 
                        "DEFEnforceSeqProcess", 
                        "DEFUseMaterGrading", 
                        "DEFUseAutoArrive", 
                        "DEFUseDrvAutoDelay", 
                        "DEFDrvAutoDelayTime", 
                        "DEFContMasterValidation", 
                        "DEFCommodSelection", 
                        "DEFDriverAdd", 
                        "DEFDriverReceipt", 
                        "DEFContMasterScannedVal", 
                        "DEFOneContPerPowerUnit", 
                        "DEFDriverEntersGPS", 
                        "DEFDriverReceiptMask", 
                        "DEFDriverReceiptAllTrips", 
                        "DEFDriverWeights", 
                        "DEFShowHostCode", 
                        "DEFUseLitre", 
                        "DEFUseKM", 
                        "DEFUseContainerLevel", 
                        "DEFContainerValidationCount", 
                        "DEFReceiptValidationCount", 
                        "DEFDriverReference", 
                        "DEFDriverReferenceMask", 
                        "DEFReferenceValidationCount", 
                        "DEFCountry", 
                        "DEFAllowAddRT", 
                        "DEFAllowChangeRT", 
                        "DEFPromptRTMsg", 
                        "DEFReqScaleRefNo", 
                        "DEFNotAvlScalRefNo", 
                        "DEFConfirmTruckInvMsg", 
                        "DEFEnableImageCapture", 
                        "DEFAutoDropContainers", 
                        "DEFShowSimilarContainers", 
                        "DEFMinAutoTriggerDone"
                    };
                    var keepSet = new HashSet<string>(propNamesDesired);

                    //
                    // TODO:  Send a list of "PreferenceLite" objects akin to PARAMS_MSG("PX")
                    // 
                    var preferenceList = preferences.Where(p => keepSet.Contains(p.Parameter)).ToList();
                    if (null != terminalMaster?.TimeZoneFactor)
                    {
                        preferenceList.Add(new Preference()
                        {
                            TerminalId = terminalMaster.TerminalId,
                            Parameter = "TimeZoneFactor",
                            ParameterValue = terminalMaster.TimeZoneFactor.ToString(),
                            Description = "The TerminalMaster.TimeZoneFactor"
                        });
                    }
                    if (null != terminalMaster?.DaylightSavings)
                    {
                        preferenceList.Add(new Preference()
                        {
                            TerminalId = terminalMaster.TerminalId,
                            Parameter = "DaylightSavings",
                            ParameterValue = terminalMaster.DaylightSavings,
                            Description = "The TerminalMaster.DaylightSavings"
                        });
                    }
                    preferencesProcess.Preferences = preferenceList;
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
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }

    
        /// <summary>
        /// If any data service faults occur.  We want to log them, stop processing and return the error.
        /// </summary>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="fault"></param>
        /// <param name="preferencesProcess"></param>
        /// <returns></returns>
        private bool handleFault(ChangeSetResult<String> changeSetResult, String msgKey, DataServiceFault fault,
            PreferencesProcess preferencesProcess)
        {
            bool faultDetected = false;
            if (null != fault)
            {
                faultDetected = true;
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("Fault occured: {0} during login request: {1}", fault.Message, preferencesProcess);
            }
            return faultDetected;
        }
   
    }
}
