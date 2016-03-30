using AutoMapper;
using Brady.ScrapRunner.Domain;
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
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.DataService.RecordTypes
{
    /// <summary>
    /// Get the relevant client terminal changes for the driver based on the last action date.  
    /// Note this our business processes is relatively independent of the "trivial" backing query.  
    /// As such, we temporarily need to invoke this
    /// service call using the form Put["/{dataServiceName}/{typeName}/{id}/withoutpersistance", true]
    /// (example: PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/TerminalChangeProcess/001/withoutpersistance) 
    /// this will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve 
    /// (getSingleAsync()) within the postSingleAsync().   This re-retrieve of a trival query clobbers our post-processed 
    /// ChangeSetResult
    /// </summary>
    [EditAction("TerminalChangeProcess")]
    public class TerminalChangeProcessRecordType : ChangeableRecordType
            <TerminalChangeProcess, string, TerminalChangeProcessValidator, TerminalChangeProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TerminalChangeProcess, TerminalChangeProcess>();

            // Note should we ever need to map the nested child list too, 
            // we would need to be more explicit.  see also: 
            // http://stackoverflow.com/questions/9394833/automapper-with-nested-child-list
            // Mapper.CreateMap<TerminalChangeProcess, TerminalChangeProcess>()
            //    .ForMember(dest => dest.TerminalChange, opts => opts.MapFrom(src => src.TerminalChange));
            // Mapper.CreateMap<TerminalChange, TerminalChange>();
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
            ChangeSet<string, TerminalChangeProcess> changeSet,
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
            ChangeSet<string, TerminalChangeProcess> changeSet, ProcessChangeSetSettings settings)
        {

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
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    TerminalChangeProcess terminalsProcess = (TerminalChangeProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the gernal case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    TerminalChangeProcess backfillTerminalChangeProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillTerminalChangeProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillTerminalChangeProcess, terminalsProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process terminalchange for Driver ID " + terminalsProcess.EmployeeId));
                        break;
                    }

                    //
                    // Validate driver id / Get the EmployeeMaster record
                    //
                    EmployeeMaster employeeMaster = Util.Common.GetEmployee(dataService, settings, userCulture, userRoleIds,
                                                  terminalsProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + terminalsProcess.EmployeeId));
                        break;
                    }

                    //
                    // Lookup Preference: DEFSendOnlyYardsForArea
                    //
                    string prefSendOnlyYardsForArea = Util.Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                 employeeMaster.TerminalId, PrefDriverConstants.DEFSendOnlyYardsForArea, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    // Lookup container changes.  
                    //
                    List<TerminalChange> terminalchanges = new List<TerminalChange>();
                    if (prefSendOnlyYardsForArea == Constants.Yes)
                    {
                        terminalchanges = Util.Common.GetTerminalChangesByArea(dataService, settings, userCulture, userRoleIds,
                          terminalsProcess.LastTerminalChangeUpdate, employeeMaster.AreaId, out fault);
                    }
                    else
                    {
                        terminalchanges = Util.Common.GetTerminalChangesByRegion(dataService, settings, userCulture, userRoleIds,
                          terminalsProcess.LastTerminalChangeUpdate, employeeMaster.RegionId, out fault);
                    }
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //For testing
                    foreach (TerminalChange terminal in terminalchanges)
                    {
                        Console.WriteLine(terminal.TerminalId);
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

