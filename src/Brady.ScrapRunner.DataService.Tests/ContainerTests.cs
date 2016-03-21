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
        /// At login time or whenever a container is changed, we should send a list of containers from the 
        /// ContainerChange table
        /// 
        /// if DefContMasterValidate = "Y" or DefContMasterScannedVal = "Y"
        /// then send the list of containers
        /// otherwise do not send anything
        /// 
        /// Code to retrieve containers updated since a particular date from the ContainerChange table
        /// RegionId and TerminalId are optional arguments
        /// I have made all three arguments optional, but in actuality, there will always be a date
        /// and optionally region and terminal
        /// If all containers need to be sent, the query should be on the ContainerMaster, 
        /// not the ContainerChange table
        /// 
        /// This method using QueryBuilder does not include the conditionals
        /// </summary>
        [TestMethod]
        public void RetrieveContainerChangeUpdatesQB()
        {
            DateTime dt  = new DateTime(2015, 12, 01);
            var containerTableQuery = new QueryBuilder<ContainerChange>()
                .Filter(y => y.Property(x => x.ActionDate).GreaterThan(dt))
                .OrderBy(x => x.ContainerNumber);
            string queryString = containerTableQuery.GetQuery();
            QueryResult<ContainerChange> queryResult = _client.QueryAsync(containerTableQuery).Result;

            foreach (ContainerChange containerTableInstance in queryResult.Records)
            {
                Assert.IsTrue(containerTableInstance.ActionDate > dt);
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
        /// At login time or whenever a container is changed, we should send a list of containers from the 
        /// ContainerChange table
        /// 
        /// if DefContMasterValidate = "Y" or DefContMasterScannedVal = "Y"
        /// then send the list of containers
        /// otherwise do not send anything
        /// 
        /// Code to retrieve containers updated since a particular date from the ContainerChange table
        /// RegionId and TerminalId are optional arguments
        /// I have made all three arguments optional, but in actuality, there will always be a date
        /// and optionally region and terminal
        /// If all containers need to be sent, the query should be on the ContainerMaster, 
        /// not the ContainerChange table
        /// 
        /// This method includes the conditionals
        /// </summary>
        [TestMethod]
        public void RetrieveContainerChangeUpdates()
        {
            bool haveFilter = false;

            //DateTime? dt = null;
            string regionid = null;
            string terminalid = null;

            DateTime? dt = new DateTime(2015, 11, 01);
            //string regionid = "SDF";
            //string terminalid = "F1";
            //string terminalid = "LI";

            //Specify the base
            string baseString = string.Format("ContainerChanges?");

            //Build the filter string
            string filterString = "$filter=";
            if (dt != null)
            {
                filterString += string.Format("ActionDate>datetime({0})", dt);
                haveFilter = true;
            }
            if (regionid != null)
            {
                if (haveFilter) filterString += " and ";
                else haveFilter = true;
                filterString += string.Format("RegionId='{0}'", regionid);
            }
            if (terminalid != null)
            {
               if (haveFilter) filterString += " and ";
                else haveFilter = true;
                filterString += string.Format("TerminalId='{0}'", terminalid);
            }
            //Build the order by string
            string orderString = "&$orderby = ContainerNumber";

            //Build the query
            string queryString = baseString;
            if (haveFilter) queryString += filterString;
            queryString +=  orderString;

            QueryResult queryResult = _client.QueryAsync(queryString).Result;

            Console.WriteLine(string.Format("{0}", queryString));
            foreach (ContainerChange containerTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                                                 containerTableInstance.RegionId,
                                                 containerTableInstance.TerminalId,
                                                 containerTableInstance.ContainerNumber,
                                                 containerTableInstance.ContainerType,
                                                 containerTableInstance.ContainerSize,
                                                 containerTableInstance.ActionDate,
                                                 containerTableInstance.ActionFlag,
                                                 containerTableInstance.ContainerBarCodeNo));
            }
        }
        /// <summary>
        /// If a driver's container master needs to be reloaded, we should send a complete
        /// list of containers from the ContainerMaster table.
        /// 
        /// if DefContMasterValidate = "Y" or DefContMasterScannedVal = "Y"
        /// then send the list of containers
        /// otherwise do not send anything
        /// 
        /// Code to retrieve all containers from the ContainerMaster
        /// RegionId and TerminalId are optional arguments
        /// 
        /// This method includes the conditionals
        /// </summary>
        [TestMethod]
        public void RetrieveContainerMasterAll()
        {
            bool haveFilter = false;

            string regionid = null;
            //string terminalid = null;

            //string regionid = "SDF";
            //string terminalid = "F1";
           string terminalid = "LI";

            //To use this query at login, provide the following:  
            //if preference DEFAllowAnyContainer =Y
            //do not provide region or terminal
            //otherwise provide just the region

            //Specify the base
            string baseString = string.Format("ContainerMasters?");

            //Build the filter string
            string filterString = "$filter=";
            if (regionid != null)
            {
                if (haveFilter) filterString += " and ";
                else haveFilter = true;
                filterString += string.Format("ContainerRegionId='{0}'", regionid);
            }
            if (terminalid != null)
            {
                if (haveFilter) filterString += " and ";
                else haveFilter = true;
                filterString += string.Format("ContainerTerminalId='{0}'", terminalid);
            }
            //Build the order by string
            string orderString = "&$orderby = ContainerNumber";

            //Build the query
            string queryString = baseString;
            if (haveFilter) queryString += filterString;
            queryString += orderString;

            QueryResult queryResult = _client.QueryAsync(queryString).Result;

            foreach (ContainerMaster containerTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                                 containerTableInstance.ContainerRegionId,
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
