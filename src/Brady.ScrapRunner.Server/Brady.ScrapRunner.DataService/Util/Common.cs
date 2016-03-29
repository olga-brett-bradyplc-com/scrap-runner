using System;
using System.Linq;
using System.Collections.Generic;
using Brady.ScrapRunner.DataService.RecordTypes;
using Brady.ScrapRunner.Domain;
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

        /// AREAMASTER Table queries
        /// <summary>
        ///  Get a list of all terminals for a given area.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<string> GetTerminalsByArea(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string areaId, out DataServiceFault fault)
        {
            fault = null;
            List<AreaMaster> terminalsArea = new List<AreaMaster>();
            List<string> terminals = new List<string>();
            if (null != areaId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<AreaMaster>()
                    .Filter(y => y.Property(x => x.AreaId).EqualTo(areaId))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return null;
                }
                terminalsArea = queryResult.Records.Cast<AreaMaster>().ToList();
                if (null != terminalsArea)
                {
                    foreach (AreaMaster terminalInstance in terminalsArea)
                    {
                        terminals.Add(terminalInstance.TerminalId);
                    }
                }
            }
            return terminals;
        }
        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all codetable values that are sent to the driver at login.
        /// CONTAINERSIZE
        /// CONTAINERTYPE
        /// DELAYCODES 
        /// EXCEPTIONCODES
        /// REASONCODES
        /// Note: CONTAINERLEVEL is optional, based on a  preference:DEFUseContainerLevel
        /// This is the qery that does not include CONTAINERLEVEL
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<CodeTable> GetAllCodeTablesForDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<CodeTable> codetables = new List<CodeTable>();
            Query query = new Query
            {
                //Under construction...
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerType)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ExceptionCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ReasonCodes))
                    .OrderBy(x => x.CodeName)
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return codetables;
            }
            codetables = queryResult.Records.Cast<CodeTable>().ToList();
            return codetables;
        }
        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all codetable values that are sent to the driver at login.
        /// CONTAINERSIZE
        /// CONTAINERTYPE
        /// DELAYCODES 
        /// EXCEPTIONCODES
        /// REASONCODES
        /// CONTAINERLEVEL is optional, based on a  preference:DEFUseContainerLevel
        /// so this is the separate query that also returns CONTAINERLEVEL code table.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<CodeTable> GetAllCodeTablesIncLevelForDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<CodeTable> codetables = new List<CodeTable>();
            Query query = new Query
            {
                //Under construction...
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerType)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ExceptionCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ReasonCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerLevel))
                    .OrderBy(x => x.CodeName)
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return codetables;
            }

            return codetables;
        }
        
        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all CONTAINERLEVEL codetable values.
        /// The preference:DEFUseContainerLevel determines whether container level is used.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<CodeTable> GetContainerLevelCodes(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, out DataServiceFault fault)
        {
            fault = null;
            List<CodeTable> containerlevels = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerLevel))
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containerlevels;
            }
            containerlevels = queryResult.Records.Cast<CodeTable>().ToList();
            return containerlevels;
        }

        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all CONTAINERTYPE codetable values.
        /// RegionId, if present, is stored in CodeDisp5. If null the code is included for all regions.
        /// Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<CodeTable> GetContainerTypeCodes(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<CodeTable> containerTypes = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerType)
                    .And(x => x.CodeDisp5).EqualTo(regionId)
                    .Or(x => x.CodeDisp5).IsNull())
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containerTypes;
            }
            containerTypes = queryResult.Records.Cast<CodeTable>().ToList();
            return containerTypes;
        }

        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all CONTAINERSIZE codetable values.
        /// Although CONTAINERSIZE contains both type and size, some types that do not have sizes will not be present in the
        /// CONTAINERSIZE table, so both CONTAINERTYPE and CONTAINERSIZE tables need to be sent
        /// RegionId, if present, is stored in CodeDisp5. If null the code is included for all regions.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<CodeTable> GetContainerTypeSizeCodes(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<CodeTable> containerTypeSizes = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                    .And(x => x.CodeDisp5).EqualTo(regionId)
                    .Or(x => x.CodeDisp5).IsNull())
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containerTypeSizes;
            }
            containerTypeSizes = queryResult.Records.Cast<CodeTable>().ToList();
            return containerTypeSizes;
        }

        /// COMMODITYMASTER Table queries
        /// <summary>
        /// Get a list of all commodities with the universal flag set to Y.
        /// These will be sent to the driver at login..
        /// Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<CommodityMaster> GetMasterCommoditiesForDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, out DataServiceFault fault)
        {
            fault = null;
            List<CommodityMaster> commodityTypes = new List<CommodityMaster>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CommodityMaster>()
                    .Filter(y => y.Property(x => x.InactiveFlag).NotEqualTo(Constants.Yes)
                    .Or(x => x.InactiveFlag).IsNull()
                    .And(x => x.UniversalFlag).EqualTo(Constants.Yes))
                    .OrderBy(x => x.CommodityDesc)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return commodityTypes;
            }
            commodityTypes = queryResult.Records.Cast<CommodityMaster>().ToList();
            return commodityTypes;
        }

        /// CONTAINERCHANGE Table queries
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

        /// CONTAINERCHANGE Table queries
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

        /// EMPLOYEEMASTER Table queries
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

        /// PREFERENCE Table queries
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

        /// PREFERENCE Table queries
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

        /// TERMINALCHANGE Table queries
        /// <summary>
        ///  Get a list of all terminal master updates after a given date time for a given region.
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
        public static List<TerminalChange> GetTerminalChangesByRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<TerminalChange> terminals = new List<TerminalChange>();
            if (null != dateTime)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalChange>()
                    .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dateTime)
                    .And().Property(x => x.RegionId).EqualTo(regionId))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                terminals = queryResult.Records.Cast<TerminalChange>().ToList();
            }
            return terminals;
        }

        /// TERMINALCHANGE Table queries
        /// <summary>
        ///  Get a list of all terminal master updates after a given date time for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<TerminalChange> GetTerminalChangesByArea(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, string areaId, out DataServiceFault fault)
        {
            fault = null;
            List<string> terminalsInArea = new List<string>();
            if (null != dateTime)
            {
                terminalsInArea = Util.Common.GetTerminalsByArea
                    (dataService, settings, userCulture, userRoleIds, areaId, out fault);
            }
            List<TerminalChange> terminals = new List<TerminalChange>();
            if (null != dateTime)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalChange>()
                    .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dateTime)
                    .And().Property(x => x.TerminalId).In(terminalsInArea.ToArray()))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                terminals = queryResult.Records.Cast<TerminalChange>().ToList();
            }
            return terminals;
        }
    }
}
