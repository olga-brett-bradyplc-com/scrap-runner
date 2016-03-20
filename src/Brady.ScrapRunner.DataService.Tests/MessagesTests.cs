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
    /// Messages tests using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class MessagesTests
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
        ///Code to retrieve messages to be sent to a particular driver who is logged in.
        /// SQL Query:
        /// select * from messages where ReceiverId = '930' 
        ///     AND Processed = 'N' 
        ///     AND DeleteFlag = 'N'
        /// </summary>
        [TestMethod]
        public void RetrieveMessagesForDriver()
        {
            string driverid = "930";
            var messagesTableQuery = new QueryBuilder<Messages>()
                .Filter(y => y.Property(x => x.ReceiverId).EqualTo(driverid)
                .And().Property(x => x.Processed).EqualTo(Constants.No)
                .And().Property(x => x.DeleteFlag).EqualTo(Constants.No))
                .OrderBy(x => x.MsgId);
            string queryString = messagesTableQuery.GetQuery();
            QueryResult<Messages> queryResult = _client.QueryAsync(messagesTableQuery).Result;

            foreach (Messages messageTableInstance in queryResult.Records)
            {
                Assert.AreEqual(messageTableInstance.ReceiverId.Trim(), driverid, queryString);
            }

            foreach (Messages messageTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 messageTableInstance.MsgId,
                                                 messageTableInstance.TerminalId,
                                                 messageTableInstance.SenderId,
                                                 messageTableInstance.ReceiverId,
                                                 messageTableInstance.Processed,
                                                 messageTableInstance.DeleteFlag,
                                                 messageTableInstance.MsgText));
            }
        }
    }
}
