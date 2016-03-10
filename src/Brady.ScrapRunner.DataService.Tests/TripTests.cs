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
    /// Trip tests using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class TripTests
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
        /// Code to retrieve trips to be sent to a particular driver at login
        /// SQL Query:
        ///     select * from Trip where TripDriverId = '930' 
        ///         and(TripStatus = 'P' or TripStatus = 'M')
        ///         and(TripAssignStatus = 'D' or TripAssignStatus = 'A')
        ///         and(TripSendFlag = 1 or TripSendFlag = 2)
        ///         order by TripSequenceNumber ASC        
        /// After it is sent, TripSendFlag is set to 2 (SentToDriver)
        /// </summary>
        
        [TestMethod]
        public void RetrieveTripsAtLogin()
        {
            string driverid = "930";
            var tripTableQuery = new QueryBuilder<Trip>()
                .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver))
                .OrderBy(x => x.TripSequenceNumber);
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Assert.AreEqual(tripTableInstance.TripDriverId, driverid, queryString);
            }

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripAssignStatus,
                                                 tripTableInstance.TripSendFlag,
                                                 tripTableInstance.TripCustName));
            }
        }

        /// <summary>
        /// Code to retrieve trips to be sent to a particular driver after a driver has logged in
        /// SQL Query:
        ///     select * from Trip where TripDriverId = '930' 
        ///         and(TripStatus = 'P' or TripStatus = 'M')
        ///         and(TripAssignStatus = 'D' or TripAssignStatus = 'A')
        ///         and(TripSendFlag = 1)
        ///         order by TripSequenceNumber ASC    
        ///   After it is sent, TripSendFlag is set to 2 (SentToDriver)
        /// </summary>
        
        [TestMethod]
        public void RetrieveTripsAfterLogin()
        {
            string driverid = "930";
            var tripTableQuery = new QueryBuilder<Trip>()
                .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.Ready))
                .OrderBy(x => x.TripSequenceNumber);
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Assert.AreEqual(tripTableInstance.TripDriverId, driverid, queryString);
            }

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripAssignStatus,
                                                 tripTableInstance.TripSendFlag,
                                                 tripTableInstance.TripCustName));
            }
        }
        /// <summary>
        ///Sending Cancelled Trip Notice to a Driver where a trip that was previously dispatched to him, has 
        ///now been canceled, put on hold, or changed to a future date
        ///   select * from Trip where TripDriverId = '930' 
        ///      and(TripStatus = 'X' or TripStatus = 'H' or TripStatus = 'F')
        ///      and(TripAssignStatus = 'D' or TripAssignStatus = 'A')
        ///      and(TripSendFlag = 4)
        ///      order by TripSequenceNumber ASC        
        ///After it is sent, TripSendFlag is set to 5  (CanceledSent)
        ///</summary>

        [TestMethod]
        public void RetrieveTripsCanceled()
        {
            string driverid = "930";
            var tripTableQuery = new QueryBuilder<Trip>()
                .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Canceled, TripStatusConstants.Hold, TripStatusConstants.Future)
                .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.CanceledReady))
                .OrderBy(x => x.TripSequenceNumber);
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Assert.AreEqual(tripTableInstance.TripDriverId, driverid, queryString);
            }

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripAssignStatus,
                                                 tripTableInstance.TripSendFlag,
                                                 tripTableInstance.TripCustName));
            }
        }

        /// <summary>
        ///Sending Cancelled Trip Notice to a Driver where a trip that has been previously dispatched to a driver,
        ///has been reassigned to a different driver or unassigned.
        ///  select * from Trip where TripDriverIdPrev = '930' 
        ///		AND (TripStatusPrev != 'F'  OR TripStatusPrev IS NULL)
        ///		AND (TripStatus = 'P' OR TripStatus = 'M' OR TripStatusPrev = 'P' or TripStatusPrev = 'M')        
        ///After reassign message is sent, TripDriverIdPrev is set to null
        ///</summary>

        [TestMethod]
        public void RetrieveTripsUnassigned()
        {
            string driverid = "930";
            var tripTableQuery = new QueryBuilder<Trip>()
                .Filter(y => y.Property(x => x.TripDriverIdPrev).EqualTo(driverid)
                .And().Property(x => x.TripStatusPrev).NotEqualTo(TripStatusConstants.Future)
                    .Or(x => x.TripStatusPrev).IsNull()
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Missed, TripStatusConstants.Pending)
                    .Or(x => x.TripStatusPrev).In(TripStatusConstants.Missed, TripStatusConstants.Pending));
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Assert.AreEqual(tripTableInstance.TripDriverIdPrev, driverid, queryString);
            }

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripDriverIdPrev,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripStatusPrev,
                                                 tripTableInstance.TripCustName));
            }
        }

        /// <summary>
        ///Sending Mark Done Trip Notice to a Driver where a trip that has been previously dispatched to a driver,
        ///has been now been marked done by the dispatcher
        ///   select * from Trip where TripDriverId = '930'
        ///       AND TripDriverIdPrev IS NOT NULL
        ///       AND(TripStatus = 'D' OR TripStatus = 'R' OR TripStatus = 'E' OR TripStatus = 'Q')        
        ///After mark done message is sent, TripDriverIdPrev is set to null
        ///</summary>

        [TestMethod]
        public void RetrieveTripsMarkedDone()
        {
            string driverid = "930";
            var tripTableQuery = new QueryBuilder<Trip>()
                .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                .And().Property(x => x.TripDriverIdPrev).IsNotNull()
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Done, TripStatusConstants.Exception, TripStatusConstants.Review, TripStatusConstants.ErrorQueue));
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Assert.AreEqual(tripTableInstance.TripDriverId, driverid, queryString);
            }

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripDriverIdPrev,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripCustName));
            }
        }

        /// <summary>
        ///Sending Resequenced Trips to a Driver where the dispatcher has resequenced the trips for a driver that is logged in.
        ///   select * from Trip where TripDriverId = '930' 
        ///		AND (TripStatus = 'P' OR TripStatus = 'M')
        ///		AND (TripSendReseqFlag= 1  OR TripSendReseqFlag = 2)
        ///		order by TripSequenceNumber ASC        
        ///After resquenced message is sent TripSendReseqFlag is set to 3 (ReseqSent)
        ///</summary>

        [TestMethod]
        public void RetrieveTripsResequenced()
        {
            string driverid = "930";
            var tripTableQuery = new QueryBuilder<Trip>()
                .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                .And().Property(x => x.TripSendReseqFlag).In(TripSendReseqFlagValue.AutoReseq, TripSendReseqFlagValue.ManualReseq))
                .OrderBy(x => x.TripSequenceNumber);
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Assert.AreEqual(tripTableInstance.TripDriverId, driverid, queryString);
            }

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripDriverIdPrev,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripSendReseqFlag,
                                                 tripTableInstance.TripCustName));
            }
        }
    }
}