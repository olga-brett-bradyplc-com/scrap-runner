using System;
using System.Linq;
using System.Collections.Generic;
using Brady.ScrapRunner.DataService.RecordTypes;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.DataService.Util;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.PortableClients;
using log4net;

namespace Brady.ScrapRunner.DataService.Util
{


    /// <summary>
    /// A collection of common utilty classes.  Typically to support ChangeableRecordType classes and the like.
    /// </summary>
    public class Common
    {


        /// <summary>
        /// Return "LastName, FirstName" or as much as possible from the provided EmployeeMaster object.
        /// </summary>
        /// <param name="employeeMaster"></param>
        /// <returns>null if employeeMaster is null or both name components are null</returns>
        public static string GetDriverName(EmployeeMaster employeeMaster)
        {
            string driverName = null;
            if (null != employeeMaster)
            {
                if (null != employeeMaster.LastName)
                {
                    driverName = employeeMaster.LastName;
                }
                if (null != employeeMaster.FirstName)
                {
                    driverName += ", " + employeeMaster.FirstName;
                }
            }
            return driverName;
        }


        /// <summary>
        /// Log an entry into the specified logger if a fault is detected.
        /// </summary>
        /// <param name="query">The originating query.</param>
        /// <param name="fault">The possibly resultant fault.</param>
        /// <param name="log">Referece to the caller's logger</param>
        /// <returns>true if a fault is detected</returns>
        public static bool LogFault(Query query, DataServiceFault fault, ILog log)
        {
            var faultDetected = false;
            if (null != fault)
            {
                faultDetected = true;
                log.ErrorFormat("Fault occured: {0} during login query: {1}", fault.Message, query.CurrentQuery);
            }
            return faultDetected;
        }


        /// <summary>
        /// Log an entry into the specified logger for every detected failure within a changeSetResult.
        /// </summary>
        /// <param name="changeSetResult">Resulting change set from an update</param>
        /// <param name="requestObject">Data that caused the error.  It should have a meaningful ToString() defined.</param>
        /// <param name="log">Referece to the caller's logger</param>
        /// <returns>true if a failure is detected</returns>
        public static bool LogChangeSetFailure(ChangeSetResult<string> changeSetResult, object requestObject, ILog log)
        {
            var errorsDetected = false;
            if (changeSetResult.FailedCreates.Any())
            {
                errorsDetected = true;
                foreach (long key in changeSetResult.FailedCreates.Keys)
                {
                    var failedChange = changeSetResult.GetFailedCreateForRef(key);
                    log.ErrorFormat("ChangeSet create error occured: {0},  Request object: {1}", failedChange, requestObject);
                }
            }
            if (changeSetResult.FailedUpdates.Any())
            {
                errorsDetected = true;
                foreach (string key in changeSetResult.FailedUpdates.Keys)
                {
                    var failedChange = changeSetResult.GetFailedUpdateForId(key);
                    log.ErrorFormat("ChangeSet update error occured: {0}, Request object: {1}", failedChange, requestObject);
                }
            }
            if (changeSetResult.FailedDeletions.Any())
            {
                errorsDetected = true;
                foreach (string key in changeSetResult.FailedDeletions.Keys)
                {
                    var failedChange = changeSetResult.GetFailedDeleteForId(key);
                    //log.ErrorFormat("ChangeSet delete error occured.  Summary: {0} during request: {1}", failedChange.Summary, requestObject);
                    log.ErrorFormat("ChangeSet delete error occured: {0}, Request object: {1}", failedChange, requestObject);
                }
            }
            return errorsDetected;
        }


        /// <summary>
        /// Update a PowerMaster record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerMaster"></param>
        /// <returns>The chageSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdatePowerMaster(IDataService dataService, ProcessChangeSetSettings settings,
            PowerMaster powerMaster)
        {
            var recordType = (PowerMasterRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerMaster");
            var changeSet = (ChangeSet<string, PowerMaster>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(powerMaster.Id, powerMaster);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }


        /// <summary>
        /// Update a DriverStatus record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverStatus"></param>
        /// <returns>The chageSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateDriverStatus(IDataService dataService, ProcessChangeSetSettings settings,
            DriverStatus driverStatus)
        {
            var recordType = (DriverStatusRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverStatus");
            var changeSet = (ChangeSet<string, DriverStatus>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(driverStatus.Id, driverStatus);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

        /// <summary>
        ///  Get a simple list of all preferences for a terminalId.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if termianlId is null</returns>
        public static List<Preference> GetPreferences(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        {
            fault = null;
            List<Preference> preferences = new List<Preference>();
            if (null != terminalId)
            {
                Query query = new Query
                {
                    // "Preferences?$filter= TerminalId='{0}'", terminalId
                    CurrentQuery = new QueryBuilder<Preference>()
                         .Filter(t => t.Property(p => p.TerminalId).EqualTo(terminalId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return preferences;
                }
                preferences = queryResult.Records.Cast<Preference>().ToList();
            }
            return preferences;
        }

        /// <summary>
        ///  Get an employee record from the EmployeeMaster
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="employeeId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if employeeId is null</returns>
        public static EmployeeMaster GetEmployee(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string employeeId, out DataServiceFault fault)
        {
            fault = null;
            EmployeeMaster employee = new EmployeeMaster();
            if (null != employeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(employeeId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return employee;
                }
                employee = (EmployeeMaster) queryResult.Records.Cast<EmployeeMaster>().FirstOrDefault();
            }
            return employee;
        }

        /// <summary>
        ///  Get a list of all container master updates after a given date time.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<ContainerChange> GetContainerChanges(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, out DataServiceFault fault)
        {
            fault = null;
            List<ContainerChange> containers = new List<ContainerChange>();
            if (null != dateTime)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerChange>()
                        .Filter(y => y.Property(x => x.ActionDate).GreaterThan(dateTime))
                        .OrderBy(x => x.ContainerNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerChange>().ToList();
            }
            return containers;
        }

        /// <summary>
        ///  Get a list of all container master updates after a given date time for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<ContainerChange> GetContainerChangesByRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<ContainerChange> containers = new List<ContainerChange>();
            if (null != dateTime)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerChange>()
                        .Filter(y => y.Property(x => x.ActionDate).GreaterThan(dateTime)
                        .And().Property(x => x.RegionId).EqualTo(regionId))
                        .OrderBy(x => x.ContainerNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerChange>().ToList();
            }
            return containers;
        }

        /// <summary>
        ///  Get the preference by parameter.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="parameter"></param>
        /// <param name="fault"></param>
        /// <returns>the parameter value or null</returns>
        public static string GetPreferenceByParameter(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, string parameter,  out DataServiceFault fault)
        {
            fault = null;
            string parametervalue = null;

            if (null != parameter)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Preference>()
                        .Filter(y => y.Property(x => x.TerminalId).EqualTo(terminalId)
                        .And().Property(x => x.Parameter).EqualTo(parameter))
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return parametervalue;
                }
                Preference preference = (Preference)queryResult.Records.Cast<Preference>().FirstOrDefault();

                if (null != preference)
                {
                    parametervalue = preference.ParameterValue;

                }
            }
            return parametervalue;
        }
    }
}
