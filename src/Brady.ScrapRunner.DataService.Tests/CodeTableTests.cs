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
    /// Code Table test using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class CodeTableTests
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
        /// Code to retrieve all customer types from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveCustomerTypes()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.CustomerType))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.CustomerType, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1));
            }
        }
        /// <summary>
        /// Code to retrieve container levels from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveContainerLevelCodes()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerLevel))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.ContainerLevel, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1));
            }
        }       
        /// <summary>
        /// Code to retrieve all delay codes from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveDelayCodesAll()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.DelayCodes, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\tType:{2}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1, codeTableInstance.CodeDisp2));
            }
        }
        /// <summary>
        /// Code to retrieve all customer-type delay codes from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveDelayCodesCustomer()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                        .And().Property(x => x.CodeDisp2).EqualTo(DelayTypeConstants.Customer))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.DelayCodes, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\tType:{2}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1, codeTableInstance.CodeDisp2));
            }
        }
        /// <summary>
        /// Code to retrieve all yard-type delay codes from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveDelayCodesYard()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                        .And().Property(x => x.CodeDisp2).EqualTo(DelayTypeConstants.Yard))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.DelayCodes, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\tType:{2}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1, codeTableInstance.CodeDisp2));
            }
        }
        /// <summary>
        /// Code to retrieve all lunch/break-type delay codes from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveDelayCodesLunch()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                        .And().Property(x => x.CodeDisp2).EqualTo(DelayTypeConstants.LunchBreak))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.DelayCodes, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\tType:{2}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1, codeTableInstance.CodeDisp2));
            }
        }
        /// <summary>
        /// Code to retrieve all exception codes from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveExceptionCodes()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ExceptionCodes))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.ExceptionCodes, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1));
            }
        }
        /// <summary>
        /// Code to retrieve all exception codes from the CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveReasonCodes()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ReasonCodes))
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.ReasonCodes, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1));
            }
        }
    }
}
