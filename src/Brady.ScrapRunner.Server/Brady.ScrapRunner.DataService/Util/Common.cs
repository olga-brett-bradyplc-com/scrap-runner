using System;
using System.Linq;
using System.Collections.Generic;
using Brady.ScrapRunner.DataService.RecordTypes;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.PortableClients;
using log4net;
using Brady.ScrapRunner.Domain.Enums;

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
        /// <param name="regionId"></param>
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

            //Filter the results
            //If a region id is in the CodeDisp5 field, make sure it matches the region id for the user
            //Also for reason codes, do not send the reason code SR#
            var filteredcodetables =
                from entry in codetables
                where (entry.CodeDisp5 == regionId || entry.CodeDisp5 == null)
                && (entry.CodeValue != Constants.NOTAVLSCALREFNO)
                select entry;

            return filteredcodetables.Cast<CodeTable>().ToList();
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
        /// <param name="regionId"></param>
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
            codetables = queryResult.Records.Cast<CodeTable>().ToList();

            //Filter the results
            //If a region id is in the CodeDisp5 field, make sure it matches the region id for the user
            //Also for reason codes, do not send the reason code SR#
            var filteredcodetables = 
                from entry in codetables
                where (entry.CodeDisp5 == regionId || entry.CodeDisp5 == null)
                && (entry.CodeValue != Constants.NOTAVLSCALREFNO)
                select entry;

            return filteredcodetables.Cast<CodeTable>().ToList();
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
        /// <returns>An empty list if dateTime is null</returns>
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
        /// <returns>An empty list if dateTime or regionId is null</returns>
        public static List<ContainerChange> GetContainerChangesByRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<ContainerChange> containers = new List<ContainerChange>();
            if (null != dateTime && regionId != null)
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
        /// DRIVER Table and driver-related queries
        /// <summary>
        ///  Get an driver status record from the DriverStatus
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static DriverStatus GetDriverStatus(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            DriverStatus driver = new DriverStatus();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverStatus>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(driverId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return driver;
                }
                driver = (DriverStatus)queryResult.Records.Cast<DriverStatus>().FirstOrDefault();
            }
            return driver;
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
        /// EMPLOYEEMASTER Table queries
        /// <summary>
        ///  Get an employee record from the EmployeeMaster for a driver
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="employeeId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if employeeId is null</returns>
        public static EmployeeMaster GetEmployeeDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string employeeId, out DataServiceFault fault)
        {
            fault = null;
            EmployeeMaster employee = new EmployeeMaster();
            if (null != employeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(employeeId)
                        .And().Property(x => x.SecurityLevel).EqualTo(SecurityLevelConstants.Driver))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return employee;
                }
                employee = (EmployeeMaster)queryResult.Records.Cast<EmployeeMaster>().FirstOrDefault();
            }
            return employee;
        }
        /// POWERMASTER Table queries
        /// <summary>
        ///  Get a a power master record for a given power unit.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if powerId is null</returns>
        public static PowerMaster GetPowerUnit(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string powerId, out DataServiceFault fault)
        {
            fault = null;
            PowerMaster powerunit = new PowerMaster();
            if (null != powerId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerMaster>()
                    .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerunit;
                }
                powerunit = (PowerMaster)queryResult.Records.Cast<PowerMaster>().FirstOrDefault();
            }
            return powerunit;
        }
        /// POWERMASTER Table queries
        /// <summary>
        ///  Get a a power master record for a given power unit.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if powerId or regionId is null</returns>
        public static PowerMaster GetPowerUnitForRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string powerId, string regionId, out DataServiceFault fault)
        {
            fault = null;
            PowerMaster powerunit = new PowerMaster();
            if (null != powerId && null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerMaster>()
                    .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId)
                    .And().Property(x => x.PowerRegionId).EqualTo(regionId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerunit;
                }
                powerunit = (PowerMaster)queryResult.Records.Cast<PowerMaster>().FirstOrDefault();
            }
            return powerunit;
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
        /// <returns>An empty list if terminalId is null</returns>
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

        /// PREFERENCE Table queries
        /// <summary>
        ///  Get the preferencse by terminal.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns>>An empty list if terminalId is null</returns>
        public static List<Preference> GetPreferenceByTerminal(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        { 
            fault = null;
            List<Preference> preferences = new List<Preference>();

            if (null != terminalId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Preference>()
                        .Filter(y => y.Property(x => x.TerminalId).EqualTo(terminalId))
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return preferences;
                }

                preferences = queryResult.Records.Cast<Preference>().ToList();
            }
            return preferences;
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
        /// <returns>An empty list if dateTime or regionId is null</returns>
        public static List<TerminalChange> GetTerminalChangesByRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, string regionId, out DataServiceFault fault)
        {
            fault = null;
            List<TerminalChange> terminals = new List<TerminalChange>();
            if (null != dateTime && null != regionId)
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
        ///  Get a list of all terminal master updates after a given date time for a given area.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime or areaId is null</returns>
        public static List<TerminalChange> GetTerminalChangesByArea(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime dateTime, string areaId, out DataServiceFault fault)
        {
            fault = null;
            List<string> terminalsInArea = new List<string>();
            if (null != dateTime && null != areaId)
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
        /// TERMINALMASTER Table queries
        /// <summary>
        ///  Get a a terminal master record for a given terminal.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if terminalId is null</returns>
        public static TerminalMaster GetTerminal(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        {
            fault = null;

            TerminalMaster terminal = new TerminalMaster();
            if (null != terminalId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalMaster>()
                    .Filter(y => y.Property(x => x.TerminalId).EqualTo(terminalId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminal;
                }
                terminal = (TerminalMaster)queryResult.Records.Cast<TerminalMaster>().FirstOrDefault();
            }
            return terminal;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of all trips to be sent to a given driver at login time.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<Trip> GetTripInfoForDriverAtLogin(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            List<Trip> trips = new List<Trip>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver))
                    .OrderBy(x => x.TripSequenceNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trips;
                }
                trips = queryResult.Records.Cast<Trip>().ToList();
            }
            return trips;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of all trips to be sent to a given driver whenever a new trip is added or an existing trip is modified.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null</returns>
        public static List<Trip> GetTripInfoForDriver(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            List<Trip> trips = new List<Trip>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.Ready))
                    .OrderBy(x => x.TripSequenceNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trips;
                }
                trips = queryResult.Records.Cast<Trip>().ToList();
            }
            return trips;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of trip reference numbers to be sent to a given driver for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null</returns>
        public static List<TripReferenceNumber> GetTripReferenceNumbers(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            List<TripReferenceNumber> tripRefNums = new List<TripReferenceNumber>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripReferenceNumber>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber))
                    .OrderBy(x => x.TripRefNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripRefNums;
                }
                tripRefNums = queryResult.Records.Cast<TripReferenceNumber>().ToList();
            }
            return tripRefNums;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of trip segments to be sent to a given driver for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null</returns>
        public static List<TripSegment> GetTripSegments(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            List<TripSegment> tripSegments = new List<TripSegment>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegStatus).In(TripSegStatusConstants.Pending, TripSegStatusConstants.Missed))
                    .OrderBy(x => x.TripSegNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegments;
                }
                tripSegments = queryResult.Records.Cast<TripSegment>().ToList();
            }
            return tripSegments;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of trip segment containers to be sent to a given driver for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null</returns>
        public static List<TripSegmentContainer> GetTripContainers(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            List<TripSegmentContainer> tripSegmentContainers = new List<TripSegmentContainer>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber))
                    .OrderBy(x => x.TripSegContainerSeqNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentContainers;
                }
                tripSegmentContainers = queryResult.Records.Cast<TripSegmentContainer>().ToList();
            }
            return tripSegmentContainers;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of directions for each destination custhostcode to be sent to a given driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if custHostCode is null</returns>
        public static List<CustomerDirections> GetCustomerDirections(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            List<CustomerDirections> custDirections = new List<CustomerDirections>();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerDirections>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.DirectionsSeqNo)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return custDirections;
                }
                custDirections = queryResult.Records.Cast<CustomerDirections>().ToList();
            }
            return custDirections;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of commodities for each destination custhostcode to be sent to a given driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if custHostCode is null</returns>
        public static List<CustomerCommodity> GetCustomerCommodities(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            List<CustomerCommodity> custCommodities = new List<CustomerCommodity>();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerCommodity>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.CustCommodityDesc)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return custCommodities;
                }
                custCommodities = queryResult.Records.Cast<CustomerCommodity>().ToList();
            }
            return custCommodities;
        }
        /// TRIP Table and trip-related queries
        /// <summary>
        ///  Get a list of locations for each destination custhostcode to be sent to a given driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if custHostCode is null</returns>
        public static List<CustomerLocation> GetCustomerLocations(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            List<CustomerLocation> custLocations = new List<CustomerLocation>();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerLocation>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.CustLocation)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return custLocations;
                }
                custLocations = queryResult.Records.Cast<CustomerLocation>().ToList();
            }
            return custLocations;
        }
    }
}
