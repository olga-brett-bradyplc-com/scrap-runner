using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.Support.NHibernate.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Util;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [EditAction("DriverLoginProcess")]
    public class DriverLoginProcessRecordType :
         ChangeableRecordType<DriverLoginProcess, string, DriverLoginProcessValidator, DriverLoginProcessDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverLoginProcess, DriverLoginProcess>();
        }

        public override DriverLoginProcess GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new DriverLoginProcess
            {
                EmployeeId = identityValues[0],
            };
        }

        public override Expression<Func<DriverLoginProcess, bool>> GetIdentityPredicate(DriverLoginProcess item)
        {
            return x => x.EmployeeId == item.EmployeeId;
        }

        public override Expression<Func<DriverLoginProcess, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.EmployeeId == identityValues[0];
        }

        // This is the deprecated signature.
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService, string token, string username, ChangeSet<string, DriverLoginProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        // This is the real method
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
            ChangeSet<string, DriverLoginProcess> changeSet, ProcessChangeSetSettings settings)
        {

            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted
            // then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository) base.repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.
            // This should gain the benefit of validators, auditing, security, piplines, etc.
            // However, it looks like we are losing some user input in the base.ProcessChangeSet
            // from a reretrieve or the sparse mapping?
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // TODO: Trap for DataServiceFault all around
            DataServiceFault fault = null;

            // If no problems, we are free to process.
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() && !changeSetResult.FailedDeletions.Any())
            { 
                // We only log in one person at a time but in the more genreal cases we could be processing multiple records.
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {

                    DriverLoginProcess driverLoginProcess = (DriverLoginProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>();

                    // It appers I must backfill user input values that were clobbered by the call to the base process method.
                    DriverLoginProcess backfillDriverLoginProcess = new DriverLoginProcess();
                    if (changeSet.Update.TryGetValue(key, out backfillDriverLoginProcess))
                    {
                        driverLoginProcess.CodeListVersion = backfillDriverLoginProcess.CodeListVersion;
                        driverLoginProcess.LastContainerMasterUpdate = backfillDriverLoginProcess.LastContainerMasterUpdate;
                        driverLoginProcess.LastTerminalMasterUpdate = backfillDriverLoginProcess.LastTerminalMasterUpdate;
                        driverLoginProcess.LocaleCode = backfillDriverLoginProcess.LocaleCode;
                        driverLoginProcess.Mdtid = backfillDriverLoginProcess.Mdtid;
                        driverLoginProcess.Odometer = backfillDriverLoginProcess.Odometer;
                        driverLoginProcess.OverrideFlag = backfillDriverLoginProcess.OverrideFlag;
                        driverLoginProcess.PndVer = backfillDriverLoginProcess.PndVer;
                        driverLoginProcess.PowerId = backfillDriverLoginProcess.PowerId;
                    }
                    else
                    {
                        // login error?
                    }

                    // 1) Process information form handheld
                    // We might want to validate somethign like Locale against configured dialects rather than ahardcoded list in validator
                    // Assume OK for now.



                    // 2) Validate driver id:  Get the EmployeeMaster - an inefficient, client side filtering sample
                    // Query query = new Query(){CurrentQuery = "EmployeeMasters"};
                    // var employeeMasters = dataService.Query(query, settings.Username, Enumerable.Empty<long>(), "en-GB", settings.Token, out fault);
                    // var employeeMaster = employeeMasters.Records.Cast<EmployeeMaster>().Where(x => x.EmployeeId == driverLoginProcess.EmployeeId).First();

                    // 2) Validate driver id
                    string queryStr = string.Format("EmployeeMasters?$filter= EmployeeId='{0}'", driverLoginProcess.EmployeeId);
                    Query query = new Query() { CurrentQuery = queryStr };
                    var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    var employeeMaster = (EmployeeMaster) queryResult.Records.Cast<EmployeeMaster>().FirstOrNull();
                    if (employeeMaster == null)
                    {
                        // Trap for invalid driver id
                        changeSetResult.FailedUpdates.Add(driverLoginProcess.EmployeeId, new MessageSet("Invalid Driver ID."));
                        break;
                    }

                    // 3) Lookup preferences
                    queryStr = string.Format("Preferences?$filter= TerminalId='{0}'", employeeMaster.TerminalId);
                    query.CurrentQuery = queryStr;
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    var preferences = queryResult.Records.Cast<Preference>();
                    driverLoginProcess.Preferences = new List<Preference>(preferences.ToArray());

                    // 4) Validate PowerId
                    queryStr = string.Format("PowerMasters?$filter= PowerId='{0}'", driverLoginProcess.PowerId);
                    query.CurrentQuery = queryStr;
                    queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    var powerMaster = (PowerMaster)queryResult.Records.Cast<PowerMaster>().FirstOrNull();
                    if (powerMaster == null)
                    {
                        // Trap for invalid driver id
                        changeSetResult.FailedUpdates.Add(driverLoginProcess.PowerId, new MessageSet("Invalid Power ID."));
                        break;
                    }

                    // 4) more to do here....

                    // 5) Validate Duplicate login?

                    // 6) Determine current trip number, segment, and driver status

                    // 7) Update Trip in progress flag

                    // 8) Check for open ended delays

                    // 9) Check for power unit change for trip in progress

                    // 10) Add/update record to DriverStatus table.

                    // 11) Add record to DriverHistory table

                    // 12) update power master

                    // 13) Add record for PowerHistory table

                    // 14) Send container master updates
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
                dataService.NotifyOfExternalChangesToData(TypeName); 
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;
      
            return changeSetResult;
        }

    }
}
