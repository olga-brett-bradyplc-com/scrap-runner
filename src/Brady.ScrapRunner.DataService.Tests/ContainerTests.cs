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
        /// Code to retrieve containers updated since a particular date from the ContainerChange table
        /// </summary>
        [TestMethod]
        public void RetrieveContainerChangeUpdates()
        {
            DateTime dt  = new DateTime(2016, 01, 01);
            var containerTableQuery = new QueryBuilder<ContainerChange>()
                //.Filter(y => y.Property(x => x.ActionDate).GreaterThan(dt))
                .OrderBy(x => x.ContainerNumber);
            string queryString = containerTableQuery.GetQuery();
            QueryResult<ContainerChange> queryResult = _client.QueryAsync(containerTableQuery).Result;

            foreach (ContainerChange containerTableInstance in queryResult.Records)
            {
                Assert.AreEqual(new DateTime(2016, 01, 01), dt);
                //Assert.IsTrue(containerTableInstance.ActionDate > dt);
            }

            foreach (ContainerChange containerTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                                 containerTableInstance.TerminalId,
                                                 containerTableInstance.ContainerNumber,
                                                 containerTableInstance.ContainerType,
                                                 containerTableInstance.ContainerSize,
                                                 containerTableInstance.ActionDate,
                                                 containerTableInstance.ContainerBarCodeNo));
            }
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
                //Assert.AreEqual(containerTableInstance.ContainerNumber, containerTableInstance.ContainerNumber, queryString);
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

        /// <summary>
        /// Code to retrieve containers that are currently on a particular truck
        /// </summary>
        [TestMethod]
        public void RetrieveContainersOnTruck()
        {
            string powerid = "601";
            var containerTableQuery = new QueryBuilder<ContainerMaster>()
                .Filter(y => y.Property(x => x.ContainerPowerId).EqualTo(powerid))
                .OrderBy(x => x.ContainerNumber);
            string queryString = containerTableQuery.GetQuery();
            QueryResult<ContainerMaster> queryResult = _client.QueryAsync(containerTableQuery).Result;

            foreach (ContainerMaster containerTableInstance in queryResult.Records)
            {
                Assert.AreEqual(containerTableInstance.ContainerPowerId, powerid, queryString);
            }

            foreach (ContainerMaster containerTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                                                 containerTableInstance.ContainerTerminalId,
                                                 containerTableInstance.ContainerNumber,
                                                 containerTableInstance.ContainerType,
                                                 containerTableInstance.ContainerSize,
                                                 containerTableInstance.ContainerContents));
            }
        }
    }
}
