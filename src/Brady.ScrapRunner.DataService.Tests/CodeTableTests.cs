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
using System.Collections.Generic;
using System.Linq;

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
        /// Code to retrieve all code tables to be sent to the driver
        /// CONTAINERSIZE
        /// CONTAINERTYPE
        /// DELAYCODES 
        /// EXCEPTIONCODES
        /// REASONCODES
        /// Although CONTAINERSIZE contains both type and size, some types that do not have sizes will not be present in the
        /// CONTAINERSIZE table, so both CONTAINERTYPE and CONTAINERSIZE tables need to be sent
        /// RegionId, if present, is stored in CodeDisp5. If null the code is included for all regions.
        /// </summary>
        [TestMethod]
        public void RetrieveCodeTablesForDriver()
        {
            string regionId = "SDF";
            var codeTableQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerType)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ExceptionCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ReasonCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerLevel))
                    .OrderBy(x => x.CodeName)
                    .OrderBy(x => x.CodeValue);

                   // .And().Parenthesis(z => z.Property(x => x.CodeDisp5).EqualTo(regionId)
                   //         .Or(x => x.CodeDisp5).IsNull())

            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;
            Console.WriteLine(string.Format("{0}", queryString));

            List<CodeTable> codetables = new List<CodeTable>();
            codetables = queryResult.Records.Cast<CodeTable>().ToList();
            //Filter the results
            //If a region id is in the CodeDisp5 field, make sure it matches the region id for the user
            //Also for reason codes, do not send the reason code SR#
            var filteredcodetables =
                from entry in codetables
                where (entry.CodeDisp5 == regionId || entry.CodeDisp5 == null)
                && (entry.CodeValue != Constants.ScaleRefNotAvailable)
                select entry;

            foreach (CodeTable codeTableInstance in filteredcodetables)
            {
                Console.WriteLine(string.Format("{0}\t\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                                    codeTableInstance.CodeName,
                                    codeTableInstance.CodeValue, 
                                    codeTableInstance.CodeDisp1, 
                                    codeTableInstance.CodeDisp2,
                                    codeTableInstance.CodeDisp3,
                                    codeTableInstance.CodeDisp4,
                                    codeTableInstance.CodeDisp5,
                                    codeTableInstance.CodeDisp6));
            }
        } 
        /// <summary>
        /// Code to retrieve container types and sizes from the CodeTable
        /// Both type and size info is in the CONTAINERSIZE CodeTable
        /// </summary>
        [TestMethod]
        public void RetrieveContainerTypeSize()
        {
            string regionId = "SDF";
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                .And(x => x.CodeDisp5).EqualTo(regionId)
                .Or(x => x.CodeDisp5).IsNull())
                .OrderBy(x => x.CodeValue);
            string queryString = codeTableQuery.GetQuery();
            QueryResult<CodeTable> queryResult = _client.QueryAsync(codeTableQuery).Result;

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Assert.AreEqual(CodeTableNameConstants.ContainerSize, codeTableInstance.CodeName, queryString);
            }

            foreach (CodeTable codeTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t\t{1}\t{2}", codeTableInstance.CodeValue, codeTableInstance.CodeDisp1, codeTableInstance.CodeDisp2));
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
        /// Code to retrieve all review reason codes from the CodeTable
        /// Except for NOTAVLSCALREFNO which is defined as SR# (Scale Reference Number)
        /// </summary>
        [TestMethod]
        public void RetrieveReasonCodes()
        {
            var codeTableQuery = new QueryBuilder<CodeTable>()
                .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ReasonCodes)
                .And().Property(x => x.CodeValue).NotEqualTo(Constants.ScaleRefNotAvailable))
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
        /// <summary>
        /// Get a list of all commodities with the universal flag set to Y.
        /// These will be sent to the driver at login..
        /// </summary>
        [TestMethod]
        public void RetrieveMasterCommoditiesForDriver()
        {
            var commodityTableQuery = new QueryBuilder<CommodityMaster>()
                   .Filter(y => y.Property(x => x.InactiveFlag).NotEqualTo(Constants.Yes)
                    .Or(x => x.InactiveFlag).IsNull()
                    .And(x => x.UniversalFlag).EqualTo(Constants.Yes))
                    .OrderBy(x => x.CommodityDesc);
            string queryString = commodityTableQuery.GetQuery();
            QueryResult<CommodityMaster> queryResult = _client.QueryAsync(commodityTableQuery).Result;
            Console.WriteLine(string.Format("{0}", queryString));
             foreach (CommodityMaster commodityTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", 
                    commodityTableInstance.CommodityCode, 
                    commodityTableInstance.CommodityDesc,
                    commodityTableInstance.InactiveFlag,
                    commodityTableInstance.UniversalFlag));
            }
        }
    }
}
