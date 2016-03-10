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

namespace Brady.ScrapRunner.DataService.Tests
{
    /// <summary>
    /// Container tests using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class ContainerTests
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
        /// Code to retrieve all containers from the ContainerMaster
        /// </summary>
        [TestMethod]
        public void RetrieveContainerMasterAll()
        {
            var containerTableQuery = new QueryBuilder<ContainerMaster>()
                .OrderBy(x => x.ContainerNumber);
            string queryString = containerTableQuery.GetQuery();
            QueryResult<ContainerMaster> queryResult = _client.QueryAsync(containerTableQuery).Result;

            foreach (ContainerMaster containerTableInstance in queryResult.Records)
            {
                //Assert.AreEqual(ContainerMaster.ContainerType, ContainerMaster.ContainerType, queryString);
            }

            foreach (ContainerMaster containerTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                                                 containerTableInstance.ContainerTerminalId,
                                                 containerTableInstance.ContainerNumber,
                                                 containerTableInstance.ContainerType,
                                                 containerTableInstance.ContainerSize,
                                                 containerTableInstance.ContainerBarCodeNo));
            }
        }
    }
}
