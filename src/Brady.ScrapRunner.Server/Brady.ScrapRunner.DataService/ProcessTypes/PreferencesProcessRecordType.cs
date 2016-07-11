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

namespace Brady.ScrapRunner.DataService.ProcessTypes
{

    /// <summary>
    /// Get the relevant client preferences for the driver's terminal.  Call this process "withoutrequery".
    /// </summary>
    /// 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example:
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/PreferencesProcess/001/withoutrequery
    /// Portable Client example:
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
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

            // Should we ever need to map the nested child list too, 
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

                    var preferencesProcess = (PreferencesProcess) changeSetResult.GetSuccessfulUpdateForId(key);

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
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

                    ////////////////////////////////////////////////
                    //PreferencesProcess has been called
                    log.DebugFormat("SRTEST:PreferencesProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:PreferencesProcess Driver:{0}",
                                     preferencesProcess.EmployeeId);

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster
                    EmployeeMaster employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  preferencesProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + preferencesProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Lookup preferences.  
                    var preferences = Common.GetPreferenceByTerminal(dataService, settings, userCulture, userRoleIds,
                                  employeeMaster.TerminalId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Lookup TerminalMaster for two "additional" preferences 
                    var terminalMaster = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                                  employeeMaster.TerminalId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Filter for the 30some properties of interst.
                    // Suppliment with the two additional timezone preferences
                    // Return preferences
                    string[] propNamesDesired = {
                        PrefDriverConstants.DEFEnforceSeqProcess,
                        PrefDriverConstants.DEFMinAutoTriggerDone,
                        PrefDriverConstants.DEFUseDrvAutoDelay,
                        PrefDriverConstants.DEFDrvAutoDelayTime,
                        PrefDriverConstants.DEFOneContPerPowerUnit,
                        PrefDriverConstants.DEFConfirmTruckInvMsg,
                        PrefDriverConstants.DEFPrevChangContID,
                        PrefDriverConstants.DEFAutoDropContainers,
                        PrefDriverConstants.DEFShowSimilarContainers,
                        PrefDriverConstants.DEFUseContainerLevel,
                        PrefDriverConstants.DEFContainerValidationCount,
                        PrefDriverConstants.DEFReqEntryContNumberForNB,
                        PrefDriverConstants.DEFCommodSelection,
                        PrefDriverConstants.DEFDriverReceipt,
                        PrefDriverConstants.DEFDriverReceiptAllTrips,
                        PrefDriverConstants.DEFDriverReceiptMask,
                        PrefDriverConstants.DEFReceiptValidationCount,
                        PrefDriverConstants.DEFDriverReference,
                        PrefDriverConstants.DEFReqScaleRefNo,
                        PrefDriverConstants.DEFDriverReferenceMask,
                        PrefDriverConstants.DEFReferenceValidationCount,
                        PrefDriverConstants.DEFNotAvlScalRefNo,
                        PrefDriverConstants.DEFDriverWeights,
                        PrefDriverConstants.DEFShowHostCode,
                        PrefDriverConstants.DEFAllowAddRT,
                        PrefDriverConstants.DEFAllowChangeRT,
                        PrefDriverConstants.DEFPromptRTMsg,
                        PrefDriverConstants.DEFEnableImageCapture,
                        PrefDriverConstants.DEFCountry,
                        PrefDriverConstants.DEFUseLitre,
                        PrefDriverConstants.DEFUseKM
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
                    //For testing
                    foreach (var preference in preferenceList)
                    {
                        log.DebugFormat("SRTEST:PreferenceProcess:TerminalId:{0} Parameter:{1} ParameterValue:{2}",
                                    preference.TerminalId,
                                    preference.Parameter,
                                    preference.ParameterValue);
                    }
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
    }
}
