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
        ///Code to retrieve dispatcher list to be sent to a particular driver at login
        /// SQL Query:
        /// 	select * from EmployeeMaster where SecurityLevel <> 'DR' 
        /// 	AND AllowMessaging = 'Y'
        ///  if regionid is not null    AND RegionId = regionid 
        ///  include employees in all the terminals for the driver's area 
        /// </summary>
        [TestMethod]
        public void RetrieveDispatcherListForDriver()
        {
            var employeeTableQuery = new QueryBuilder<EmployeeMaster>()
                .Filter(y => y.Property(x => x.SecurityLevel).NotEqualTo("DR")
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes))            
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.FirstName);
            string queryString = employeeTableQuery.GetQuery();
            QueryResult<EmployeeMaster> queryResult = _client.QueryAsync(employeeTableQuery).Result;

            foreach (EmployeeMaster employeeTableInstance in queryResult.Records)
            {
                Assert.AreNotEqual(employeeTableInstance.SecurityLevel, "DR", queryString);
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
    }
}
