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
using System.Text;
using System.Collections.Generic;
using System.Linq;

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
        ///   For a New Trip, Mobile Device displays "New Trip (%s):\n%s",TripNumber, TripSegDestCustName
        ///   For a Modified Trip, Mobile Device displays "Trip Modified (%s):\n%s",TripNumber, TripSegDestCustName
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
        /// Code to retrieve trips, tripsegments, tripsegmentcontainers to be sent to a particular driver after a driver has logged in
        /// SQL Query:
        ///     select * from Trip where TripDriverId = '930' 
        ///         and(TripStatus = 'P' or TripStatus = 'M')
        ///         and(TripAssignStatus = 'D' or TripAssignStatus = 'A')
        ///         and(TripSendFlag = 1)
        ///         order by TripSequenceNumber ASC 
        ///     for each TripNumber    
        ///     select * from TripSegment where TripNumber = {TripNumber}
        ///         and TripSegStatus = 'P' or TripStatus = 'M')
        ///     for each TripSegment    
        ///     select * from TripSegmentContainer where TripNumber = {TripNumber}
        ///         and TripSegNumber = {TripSegNumber}
        ///   After it is sent, TripSendFlag is set to 2 (SentToDriver)
        ///   For a New Trip, Mobile Device displays "New Trip (%s):\n%s",TripNumber, TripSegDestCustName
        ///   For a Modified Trip, Mobile Device displays "Trip Modified (%s):\n%s",TripNumber, TripSegDestCustName
        /// </summary>

        [TestMethod]
        public void RetrieveTripInfo()
        {
            string driverid = "930";
            bool bLogin = true;
            List<string> customersInTrips = new List<string>();
            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Get the trips
            var tripTableQuery = new QueryBuilder<Trip>();
           if (bLogin)
            {
                tripTableQuery.Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver))
                    .OrderBy(x => x.TripSequenceNumber);
            }
            else
            {
                tripTableQuery.Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverid)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.Ready))
                    .OrderBy(x => x.TripSequenceNumber);
            }
            string queryString = tripTableQuery.GetQuery();
            QueryResult<Trip> queryResult = _client.QueryAsync(tripTableQuery).Result;

            foreach (Trip tripTableInstance in queryResult.Records)
            {
                Console.WriteLine("Trip");
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                 tripTableInstance.TripTerminalId,
                                                 tripTableInstance.TripDriverId,
                                                 tripTableInstance.TripNumber,
                                                 tripTableInstance.TripStatus,
                                                 tripTableInstance.TripAssignStatus,
                                                 tripTableInstance.TripSendFlag,
                                                 tripTableInstance.TripCustName));

                ////////////////////////////////////////////////////////////////////////////////////////////////
                //For each trip, get the reference numbers
                var tripreferenceTableQuery = new QueryBuilder<TripReferenceNumber>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripTableInstance.TripNumber))
                    .OrderBy(x => x.TripRefNumber);

                string query5String = tripreferenceTableQuery.GetQuery();
                QueryResult<TripReferenceNumber> query5Result = _client.QueryAsync(tripreferenceTableQuery).Result;
                Console.WriteLine("Trip Reference Numbers");
                foreach (TripReferenceNumber tripreferenceTableInstance in query5Result.Records)
                {
                    Console.WriteLine(string.Format("{0}\t{1}\t{2}",
                                                     tripreferenceTableInstance.TripNumber,
                                                     tripreferenceTableInstance.TripRefNumber,
                                                     tripreferenceTableInstance.TripRefNumberDesc));
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////
                //For each trip, get the segments
                var tripsegmentTableQuery = new QueryBuilder<TripSegment>()
                .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripTableInstance.TripNumber)
                .And().Property(x => x.TripSegStatus).In(TripSegStatusConstants.Pending, TripSegStatusConstants.Missed))
                .OrderBy(x => x.TripSegNumber);

                string query2String = tripsegmentTableQuery.GetQuery();
                QueryResult<TripSegment> query2Result = _client.QueryAsync(tripsegmentTableQuery).Result;
                foreach (TripSegment tripsegmentTableInstance in query2Result.Records)
                {
                    Console.WriteLine("Trip Segment");
                    Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                        tripsegmentTableInstance.TripNumber,
                                                        tripsegmentTableInstance.TripSegNumber,
                                                        tripsegmentTableInstance.TripSegStatus,
                                                        tripsegmentTableInstance.TripSegOrigCustHostCode,
                                                        tripsegmentTableInstance.TripSegOrigCustName,
                                                        tripsegmentTableInstance.TripSegDestCustHostCode,
                                                        tripsegmentTableInstance.TripSegDestCustName));
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //For each trip segment, get the containers
                    var tripsegcontainerTableQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripsegmentTableInstance.TripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripsegmentTableInstance.TripSegNumber))
                    .OrderBy(x => x.TripSegContainerSeqNumber);
                    string query3String = tripsegcontainerTableQuery.GetQuery();
                    QueryResult<TripSegmentContainer> query3Result = _client.QueryAsync(tripsegcontainerTableQuery).Result;
                    Console.WriteLine("Trip Segment Containers");
                    foreach (TripSegmentContainer tripsegcontainerTableInstance in query3Result.Records)
                    {
                        Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                                            tripsegcontainerTableInstance.TripNumber,
                                                            tripsegcontainerTableInstance.TripSegNumber,
                                                            tripsegcontainerTableInstance.TripSegContainerSeqNumber,
                                                            tripsegcontainerTableInstance.TripSegContainerNumber,
                                                            tripsegcontainerTableInstance.TripSegContainerType,
                                                            tripsegcontainerTableInstance.TripSegContainerSize));
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    //For each segment get the dest cust host code. 
                    //Add it to the customersInTrips list, if not already in the list
                    if (customersInTrips.Where(x => x.Contains(tripsegmentTableInstance.TripSegDestCustHostCode)).FirstOrDefault() == null)
                        customersInTrips.Add(tripsegmentTableInstance.TripSegDestCustHostCode);
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Loop through the list of customer host codes
            //Get the directions to each customer host code 
            Console.WriteLine("Customer Directions");
            foreach (string custHostCode in customersInTrips)
            {
                var directionsTableQuery = new QueryBuilder<CustomerDirections>()
                .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                .OrderBy(x => x.CustHostCode)
                .OrderBy(x => x.DirectionsSeqNo);
                string query4String = directionsTableQuery.GetQuery();
                QueryResult<CustomerDirections> query4Result = _client.QueryAsync(directionsTableQuery).Result;

                StringBuilder sbDirections = new StringBuilder();
                foreach (CustomerDirections directionsTableInstance in query4Result.Records)
                {
                    sbDirections.Append(directionsTableInstance.DirectionsDesc.Trim());
                    sbDirections.Append(" ");
                }
                string directions = sbDirections.ToString().Trim();
                Console.WriteLine(string.Format("{0}\t{1}", custHostCode, directions));
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Loop through the list of customer host codes
            //if DefCommodSelection = Y, set the commodities for each customer
            Console.WriteLine("Customer Commodities");
            foreach (string custHostCode in customersInTrips)
            {
                var customerCommoditiesTableQuery = new QueryBuilder<CustomerCommodity>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.CustCommodityDesc);
                string query6String = customerCommoditiesTableQuery.GetQuery();
                QueryResult<CustomerCommodity> query6Result = _client.QueryAsync(customerCommoditiesTableQuery).Result;
                foreach (CustomerCommodity customerCommoditiesTableInstance in query6Result.Records)
                {
                    Console.WriteLine(string.Format("{0}\t{1}\t{2}",
                                     customerCommoditiesTableInstance.CustHostCode,
                                     customerCommoditiesTableInstance.CustCommodityCode,
                                     customerCommoditiesTableInstance.CustCommodityDesc));
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Loop through the list of customer host codes
            //Get the locations for each customer
            Console.WriteLine("Customer Locations");
            foreach (string custHostCode in customersInTrips)
            {
                var customerLocationsTableQuery = new QueryBuilder<CustomerLocation>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.CustLocation);
                string query7String = customerLocationsTableQuery.GetQuery();
                QueryResult<CustomerLocation> query7Result = _client.QueryAsync(customerLocationsTableQuery).Result;
                foreach (CustomerLocation customerLocationsTableInstance in query7Result.Records)
                {
                    Console.WriteLine(string.Format("{0}\t{1}",
                                      customerLocationsTableInstance.CustHostCode,
                                      customerLocationsTableInstance.CustLocation));
                }
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
        ///For Cancels, Mobile Device displays "Trip canceled by DISPATCH:\n%s",TripSegDestCustName
        ///For Holds, Mobile Device displays "Trip placed on hold by DISPATCH:\n%s",TripSegDestCustName
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
        ///Mobile Device displays "Trip canceled by DISPATCH:\n%s",TripSegDestCustName
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
        ///Mobile Device displays "Trip marked done by DISPATCH:\n%s",TripSegDestCustName
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
        ///For mannually resequenced trips, Mobile Device displays ""TRIPS RESEQUENCED"
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
        /// <summary>
        /// Sending a force logoff message to driver that is logged in
        /// 
        /// select * from DriverStatus where DriverId = {driverId}  
        ///    AND SendHHLogoffFlag = 1
        ///    AND(DriverStatus<> 'K' and DriverStatus <> 'R') 
        /// 
        /// After a force logoff message is sent, SendHHLogoffFlag is set to 2
        /// When Mobile Device sends back an ack, SendHHLogoffFlag is set to 3
        /// </summary>
        [TestMethod]
        public void SendForceLogoffMessageToDriver()
        {
            string driverid = "930";

            var driverTableQuery = new QueryBuilder<DriverStatus>()
                .Filter(y => y.Property(x => x.EmployeeId).EqualTo(driverid)
                .And().Property(x => x.SendHHLogoffFlag).EqualTo(DriverForceLogoffValue.Ready)
                .And().Property(x => x.Status).NotIn(DriverStatusSRConstants.Disconnected, DriverStatusSRConstants.Ready));

            string queryString = driverTableQuery.GetQuery();
            QueryResult<DriverStatus> queryResult = _client.QueryAsync(driverTableQuery).Result;

            foreach (DriverStatus driverTableInstance in queryResult.Records)
            {
                Assert.AreEqual(driverTableInstance.SendHHLogoffFlag, DriverForceLogoffValue.Ready, queryString);
            }

            foreach (DriverStatus driverTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}",
                                                 driverTableInstance.EmployeeId,
                                                 driverTableInstance.SendHHLogoffFlag,
                                                 driverTableInstance.Status));
            }
        }
    }
}