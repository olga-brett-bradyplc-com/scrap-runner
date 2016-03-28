using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;
using BWF.DataServices.PortableClients.Builder;
using BWF.DataServices.PortableClients.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Brady.ScrapRunner.Domain.Enums;
using System.Collections.Generic;

namespace Brady.ScrapRunner.DataService.Tests
{

    /// <summary>
    /// Employee tests using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class EmployeeTests
    {

        private static TestContext _testContext;
        private static IDataServiceClient _client;

        [ClassInitialize]
        public static void BeforeAllTests(TestContext testContextInstance)
        {
            _testContext = testContextInstance;

            var hostUrl = ConfigurationManager.AppSettings["ExplorerHostUrl"];
            var hostUsername = ConfigurationManager.AppSettings["ExplorerHostUsername"];
            var hostPassword = ConfigurationManager.AppSettings["ExplorerHostPassword"];

            // Note since self-signed, we set server certificate validation callback to not complain.
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

            // The client implements IDisposable, so if not using a single instance remember to dispose your instances.
            _client = new DataServiceClient(hostUrl, hostUsername, hostPassword, Constants.ScrapRunner);
        }

        [ClassCleanup]
        public static void AfterAllTests()
        {
            // The client implements IDisposable, so if not using a single instance remember to dispose your instances.
            _client.Dispose();
        }

        /// <summary>
        /// Code to test RetrieveDispatcherListForArea, RetrieveDispatcherListForRegion,RetrieveDispatcherListAll
        /// At login time, we will send a list of users that have access to messaging
        /// If preference SendDispatchersForArea is set to Y
        /// then send only the users whose yard is in the driver's area
        /// otherwise send only the users for the driver's region
        /// RegionId and AreaId are optional arguments
        /// SecurityLevel <> 'DR' 
        /// AllowMessaging = 'Y'
        /// </summary>
        [TestMethod]
        public void RetrieveDispatcherListForDriver()
        {
            string areaid = "LI";
            string regionid = null;
            string DEFSendDispatchersForArea = "Y";
            QueryResult<EmployeeMaster> queryResult;

            if (DEFSendDispatchersForArea == Constants.Yes && areaid != null)
                queryResult = RetrieveDispatcherListForArea(areaid);
            else if (regionid != null)
                queryResult = RetrieveDispatcherListForRegion(regionid);
            else
                queryResult = RetrieveDispatcherListAll();

            foreach (EmployeeMaster employeeTableInstance in queryResult.Records)
            {
                Assert.AreNotEqual(employeeTableInstance.SecurityLevel, SecurityLevelConstants.Driver);
            }

            foreach (EmployeeMaster employeeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 employeeTableInstance.AreaId,
                                                 employeeTableInstance.TerminalId,
                                                 employeeTableInstance.SecurityLevel,
                                                 employeeTableInstance.AllowMessaging,
                                                 employeeTableInstance.EmployeeId,
                                                 employeeTableInstance.LastName,
                                                 employeeTableInstance.FirstName));
            }
        }
         /// <summary>
        /// Retrieves the list of dispatchers for a given area
        /// </summary>
        public QueryResult<EmployeeMaster> RetrieveDispatcherListForArea(string areaid)
        {
            List<string> terminals = BuildTerminalList(areaid);
            var employeeTableQuery = new QueryBuilder<EmployeeMaster>()
                .Filter(y => y.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes)
                .And().Property(x => x.TerminalId).In(terminals.ToArray()))
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.FirstName);
            string queryString = employeeTableQuery.GetQuery();
            QueryResult<EmployeeMaster> queryResult = _client.QueryAsync(employeeTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Retrieves the list of dispatchers for a given region
        /// </summary>
        public QueryResult<EmployeeMaster> RetrieveDispatcherListForRegion(string regionid)
        {
            var employeeTableQuery = new QueryBuilder<EmployeeMaster>()
                .Filter(y => y.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes)
                .And().Property(x => x.RegionId).EqualTo(regionid))
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.FirstName);
            string queryString = employeeTableQuery.GetQuery();
            QueryResult<EmployeeMaster> queryResult = _client.QueryAsync(employeeTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Retrieves the list of all dispatchers 
        /// </summary>
        public QueryResult<EmployeeMaster> RetrieveDispatcherListAll()
        {
            var employeeTableQuery = new QueryBuilder<EmployeeMaster>()
                .Filter(y => y.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes))
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.FirstName);
            string queryString = employeeTableQuery.GetQuery();
            QueryResult<EmployeeMaster> queryResult = _client.QueryAsync(employeeTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Builds a list of terminals for a given area 
        /// </summary>
        public List<string> BuildTerminalList(string areaid)
        {
            List<string> terminals = new List<string>();
            var areaTableQuery = new QueryBuilder<AreaMaster>()
                .Filter(y => y.Property(x => x.AreaId).EqualTo(areaid));
            string queryString = areaTableQuery.GetQuery();
            QueryResult<AreaMaster> queryResult = _client.QueryAsync(areaTableQuery).Result;
            foreach (AreaMaster areaTableInstance in queryResult.Records)
            {
                terminals.Add(areaTableInstance.TerminalId);
            }
            return terminals;
        }

        /// <summary>
        /// At login time, we should send a list of users from the EmployeeMaster table
        /// 
        /// If preference SendDispatchersForArea is set to Y
        /// then send only the users whose yard is in the driver's area
        /// otherwise send only the users for the driver's region
        /// 
        /// RegionId and AreaId are optional arguments
        /// SecurityLevel <> 'DR' 
        /// AllowMessaging = 'Y'
        /// 
        /// This method includes the conditionals and BuildFilterArea
        /// </summary>
        [TestMethod]
        public void RetrieveDispatcherListForDriverRS()
        {
            string regionid = null;
            //string areaid = null;
            string DefSendOnlyYardsForArea = "Y";

            string terminalString = null;

            //string regionid = "SDF";
            string areaid = "ALL";
            //string areaid = "LI";
            //string areaid = "NE";
            //string DefSendOnlyYardsForArea = "N";
            //string DefSendOnlyYardsForArea = null;

            //To use this query at login, provide the following:
            //if DefSendOnlyYardsForArea = "Y", provide driver's area
            //otherwise, provide driver's region

            //Specify the terminal field name in the EmployeeMaster table
            string terminalField = "TerminalId";
            //Specify the base
            string baseString = string.Format("EmployeeMasters?");

            //Build the filter string
            if (DefSendOnlyYardsForArea == Constants.Yes && areaid != null)
            {
                terminalString = BuildFilterArea(terminalField, areaid, null);
            }

            string filterString = "$filter=";
            filterString += string.Format("SecurityLevel!='{0}'", SecurityLevelConstants.Driver);
            filterString += string.Format(" and AllowMessaging='{0}'", Constants.Yes);
            if (regionid != null)
            {
                filterString += string.Format(" and RegionId='{0}'", regionid);
            }
            if (terminalString != null)
            {
                filterString += string.Format(" and {0}", terminalString);
            }
            //Build the order by string
            string orderString = "&$orderby = LastName,FirstName";

            //Build the query
            string queryString = baseString;
            queryString += filterString;
            queryString += orderString;

            QueryResult queryResult = _client.QueryAsync(queryString).Result;

            Console.WriteLine(string.Format("{0}", queryString));
            foreach (EmployeeMaster employeeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                                                 employeeTableInstance.RegionId,
                                                 employeeTableInstance.AreaId,
                                                 employeeTableInstance.TerminalId,
                                                 employeeTableInstance.SecurityLevel,
                                                 employeeTableInstance.AllowMessaging,
                                                 employeeTableInstance.EmployeeId,
                                                 employeeTableInstance.LastName,
                                                 employeeTableInstance.FirstName));
            }
        }
        /// <summary>
        /// Method to build a filter of all terminals for a user's area
        /// terminalField is required and consists of the database field name, i.e. "TerminalId"
        /// areaid is required
        /// terminalid is optional 
        /// returns a string to be used as a filter or part of a filter
        /// Ex:  (TerminalId = 'LI' or TerminalId = 'F1")
        /// </summary>
        public string BuildFilterArea(string terminalField, string areaid, string terminalid)
        {
            string areaFilter = null;

            if (terminalid != null)
            {
                areaFilter += string.Format("{0}='{1}'", terminalField, terminalid);
            }
            else
            {
                var areaTableQuery = new QueryBuilder<AreaMaster>()
                    .Filter(y => y.Property(x => x.AreaId).EqualTo(areaid));
                string queryString = areaTableQuery.GetQuery();
                QueryResult<AreaMaster> queryResult = _client.QueryAsync(areaTableQuery).Result;
                foreach (AreaMaster areaTableInstance in queryResult.Records)
                {
                    if (areaFilter != null) areaFilter += " or ";
                    areaFilter += string.Format("{0}='{1}'", terminalField, areaTableInstance.TerminalId);
                }
            }
            if (areaFilter == null)
            {
                areaFilter = string.Format("{0}=''", terminalField);
            }
            areaFilter = "(" + areaFilter + ")";

            return areaFilter;
        }
    }
}
