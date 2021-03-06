﻿using System;
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
using System.Linq;

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
            Assert.AreEqual(509, queryResult.TotalCount, queryString);
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

            Assert.AreEqual(9, queryResult.TotalCount, queryString);
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
        /// <summary>
        /// Delete a single container history record
        /// </summary>
        [TestMethod]
        public void DeleteContainerHistoryOneRecord()
        {
            string containerNumber = "1000S30";
            int containerSeqNumber = 2;

            var containerTableQuery = new QueryBuilder<ContainerHistory>()
                .Filter(y => y.Property(x => x.ContainerNumber).EqualTo(containerNumber)
                .And().Property(x => x.ContainerSeqNumber).EqualTo(containerSeqNumber));

            string queryString = containerTableQuery.GetQuery();
            Console.WriteLine(string.Format("queryString:{0}", queryString));

            QueryResult<ContainerHistory> queryResult = _client.QueryAsync(containerTableQuery).Result;
            var container = queryResult.Records.Cast<ContainerHistory>().FirstOrDefault();

            if (null != container)
            {
                Console.WriteLine(string.Format("container.Id:{0}", container.Id));
                ChangeResult changeResult = _client.DeleteAsync<String, ContainerHistory>(container.Id).Result;
                if (!changeResult.WasSuccessful)
                {
                    Console.WriteLine(string.Format("Delete Failed"));
                    Assert.Fail("ContainerHistory Delete Async not successful");
                }
                else
                {
                    Console.WriteLine(string.Format("Delete Successful"));
                }
            }
            else
            {
                Console.WriteLine(string.Format("containerNumber:{0} containerSeqNumber {1} not found.", 
                                  containerNumber, containerSeqNumber));
            }
        }
        [TestMethod]
        public void DeleteContainerHistoryMultipleRecords()
        {
            string containerNumber = "T3001";

            var containerTableQuery = new QueryBuilder<ContainerHistory>()
                .Filter(y => y.Property(x => x.ContainerNumber).EqualTo(containerNumber))
                .OrderBy(x => x.ContainerSeqNumber);
            string queryString = containerTableQuery.GetQuery();
            Console.WriteLine(string.Format("queryString:{0}", queryString));

            QueryResult<ContainerHistory> queryResult = _client.QueryAsync(containerTableQuery).Result;
            var containers = queryResult.Records.Cast<ContainerHistory>().ToList();

            if (null != containers && containers.Count() > 0)
            {
                foreach (var container in containers)
                {
                    Console.WriteLine(string.Format("container.Id:{0}", container.Id));
                    ChangeResult changeResult = _client.DeleteAsync<String, ContainerHistory>(container.Id).Result;
                    if (!changeResult.WasSuccessful)
                    {
                        Console.WriteLine(string.Format("Delete Failed"));
                        Assert.Fail("ContainerHistory Delete Async not successful");
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Delete Successful"));
                    }
                }
            }
            else
            {
                Console.WriteLine(string.Format("containerNumber:{0} not found.", containerNumber));
            }
        }

        /// <summary>
        /// Illustrate equivalence of QueryBuilder and raw querystring approach
        /// </summary>
        [TestMethod]
        public void TestQueryEquivalance()
        {

            //    
            // 1st Query the CodeTable for an ordered set of CUSTOMERTYPE using the QueryBuilder.
            // Note QueryAsync(QueryBuilder) allows a strongly typed QueryResult
            // so I can avoid a cast.
            //
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo("CUSTOMERTYPE"))
                .OrderBy(x => x.CodeName)
                .OrderBy(x => x.CodeValue);
            var queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;
            Assert.AreEqual(9, queryResult.TotalCount, queryString);
            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual("CUSTOMERTYPE", codeTableInstance.CodeName, queryString);
            }

            //    
            // 2st Query the CodeTable for an ordered set of CUSTOMERTYPE by a query string.
            // Note QueryAsync(string) forces a weakly typed QueryResult<Object>
            // so we must cast the Records.
            //
            var queryString2 = "CodeTables?$filter=CodeName='CUSTOMERTYPE' &$orderby=CodeName,CodeValue";
            QueryResult queryResult2 = _client.QueryAsync(queryString2).Result;
            Assert.AreEqual(9, queryResult2.TotalCount, queryString);
            Assert.AreEqual(queryResult.TotalCount, queryResult2.TotalCount, "The two approaches are not the same!");
            for (int i = 0; i < queryResult.TotalCount; i++)
            {
                CodeTable codeTable1 = queryResult.Records[i];
                CodeTable codeTable2 = (CodeTable)queryResult2.Records[i];
                Assert.AreEqual(codeTable1.Id, codeTable2.Id);
                Assert.AreEqual(codeTable1.CodeName, codeTable2.CodeName);
                Assert.AreEqual(codeTable1.CodeValue, codeTable2.CodeValue);
            }

        }
        [TestMethod]
        public void IllustrateQueryBuilder()
        {

            // Simple AND of two clauses (nested within the AND)
            var qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                 .And(em2 => em2.Property(x => x.AllowMessaging).EqualTo(Constants.Yes)));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and (AllowMessaging='Y')", qb.GetQuery());
            Console.WriteLine(string.Format("{0}", qb.GetQuery()));

            // Simple AND of two clauses (appended to AND)
            qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and AllowMessaging='Y'", qb.GetQuery());
            Console.WriteLine(string.Format("{0}", qb.GetQuery()));

            // Simple AND of three clauses (nested within the ANDs)
            var terminalid = "FOO";
            qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(emp => emp.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And(em2 => em2.Property(x => x.AllowMessaging).EqualTo(Constants.Yes))
                .And(em3 => em3.Property(x2 => x2.TerminalId).EqualTo(terminalid)));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and (AllowMessaging='Y') and (TerminalId='FOO')", qb.GetQuery());
            Console.WriteLine(string.Format("{0}", qb.GetQuery()));

            // Simple AND of three clauses (appended to ANDs)
            qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(emp => emp.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes)
                .And().Property(x2 => x2.TerminalId).EqualTo(terminalid));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and AllowMessaging='Y' and TerminalId='FOO'", qb.GetQuery());
            Console.WriteLine(string.Format("{0}", qb.GetQuery()));

        }

    }
}