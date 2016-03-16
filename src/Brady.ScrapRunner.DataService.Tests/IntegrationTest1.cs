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

        [TestMethod]
        public void TestCodeTableTopQuery()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Top(100)
                .OrderBy(x => x.CodeName)
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            Assert.AreEqual(100, queryResult.Records.Count);
            Assert.AreEqual(501, queryResult.TotalCount, queryString);
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

        /// <summary>
        /// Sample test of complete POST, PUT, GET, DELETE lifecycle using the 
        /// CommmodityCode table and objects.
        /// </summary>
        [TestMethod]
        public void TestCommodityCodeLifecycle()
        {

            //
            // Nothing via query, hopefully
            //

            var codeTableQuery = new QueryBuilder<CommodityMaster>()
                .Filter(y => y.Property(x => x.CommodityCode).EqualTo("INTEGTEST"));
            string queryString = codeTableQuery.GetQuery();

            QueryResult<CommodityMaster> queryResult = _client.QueryAsync(codeTableQuery).Result;
            Assert.AreEqual(0, queryResult.TotalCount, queryString);

            //
            // Similarly, expect null result when asking directly by PK
            //

            var objectResult = _client.GetAsync<string, CommodityMaster>("INTEGTEST").Result;
            Assert.IsNull(objectResult);

            //
            // Create one locally, insert it, and verify echoed properties
            //

            CommodityMaster newCommodityMaster = new CommodityMaster() {CommodityCode = "INTEGTEST" , CommodityDesc = "INTEGTEST"
                    , DestContainerLocation = "MAUMEE" , InactiveFlag = "Y", UniversalFlag = "N"
            };

            ChangeResultWithItem<CommodityMaster> changeResultWithItem = _client.CreateAsync(newCommodityMaster).Result;
            if (!changeResultWithItem.WasSuccessful)
            {
                Assert.Fail("CommodityMaster Create Async not successful");
            }

            CommodityMaster insertedCopy = changeResultWithItem.Item;
            Assert.IsNotNull(insertedCopy);
            Assert.AreEqual("INTEGTEST", insertedCopy.CommodityCode);
            Assert.AreEqual("INTEGTEST", insertedCopy.CommodityDesc);
            Assert.AreEqual("MAUMEE", insertedCopy.DestContainerLocation);
            Assert.AreEqual("Y", insertedCopy.InactiveFlag);
            Assert.AreEqual("N", insertedCopy.UniversalFlag);

            //
            // Pull back a second copy frmm the db and verify persisted properties
            //

            CommodityMaster duplicateCopy = _client.GetAsync<string, CommodityMaster>("INTEGTEST").Result;
            Assert.IsNotNull(duplicateCopy);
            Assert.AreEqual(insertedCopy.CommodityCode, duplicateCopy.CommodityCode);
            Assert.AreEqual(insertedCopy.CommodityDesc, duplicateCopy.CommodityDesc);
            Assert.AreEqual(insertedCopy.DestContainerLocation, duplicateCopy.DestContainerLocation);
            Assert.AreEqual(insertedCopy.InactiveFlag, duplicateCopy.InactiveFlag);
            Assert.AreEqual(insertedCopy.UniversalFlag, duplicateCopy.UniversalFlag);

            //
            // Update the now existing record.
            //

            newCommodityMaster.CommodityDesc = "INTEGTEST updated";
            changeResultWithItem = _client.UpdateAsync(newCommodityMaster).Result;
            if (!changeResultWithItem.WasSuccessful)
            {
                Assert.Fail("CommodityMaster Update Async not successful");
            }

            duplicateCopy = _client.GetAsync<string, CommodityMaster>("INTEGTEST").Result;
            Assert.IsNotNull(duplicateCopy);
            Assert.AreEqual(insertedCopy.CommodityCode, duplicateCopy.CommodityCode);
            Assert.AreNotEqual(insertedCopy.CommodityDesc, duplicateCopy.CommodityDesc);
            Assert.AreEqual(newCommodityMaster.CommodityDesc, duplicateCopy.CommodityDesc);
            Assert.AreEqual(insertedCopy.DestContainerLocation, duplicateCopy.DestContainerLocation);
            Assert.AreEqual(insertedCopy.InactiveFlag, duplicateCopy.InactiveFlag);
            Assert.AreEqual(insertedCopy.UniversalFlag, duplicateCopy.UniversalFlag);

            //
            // Lastly delete the record
            //

            ChangeResult changeResult = _client.DeleteAsync<String, CommodityMaster>("INTEGTEST").Result;
            if (!changeResult.WasSuccessful)
            {
                Assert.Fail("CommodityMaster Delete Async not successful");
            }

        }
    }
}