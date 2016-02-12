using System;
using System.Runtime.CompilerServices;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.PortableClients;
using BWF.DataServices.PortableClients.Builder;
using BWF.DataServices.PortableClients.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brady.ScrapRunner.DataService.Tests
{

    /// <summary>
    /// A rudimentatry integration test using the BWF DataServices PortableClient.
    /// This illustrates it use. 
    /// </summary>
    [TestClass]
    public class IntegrationTest1
    {

        private static TestContext _testContext;
        private static IDataServiceClient _client;

        [ClassInitialize]
        public static void BeforeAllTests(TestContext testContextInstance)
        {
            _testContext = testContextInstance;

            // Note since self-signed, we set server certificate validation callback to not complain.
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                  new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

            // The client implements IDisposable, so if not using a single instance remember to dispose your instances.
            _client = new DataServiceClient("https://localhost:7776", "admin", "mem_2014", Constants.ScrapRunner);
        }

        [ClassCleanup]
        public static void AfterAllTests()
        {
            // The client implements IDisposable, so if not using a single instance remember to dispose your instances.
            _client.Dispose();
        }

        [TestMethod]
        public void TestCodeTableTopQuery()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Top(100)
                .OrderBy(x => x.CodeName)
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            Assert.AreEqual(100, queryResult.TotalCount, queryString);
        }

        [TestMethod]
        public void TestCodeTableFilterQuery()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo("CUSTOMERTYPE"))
                .OrderBy(x => x.CodeName)
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;
      
            Assert.AreEqual(7, queryResult.TotalCount, queryString);
            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual("CUSTOMERTYPE", codeTableInstance.CodeName, queryString);
            }
        }
    }
}
