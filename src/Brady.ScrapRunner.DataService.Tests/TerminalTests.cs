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
using System.Collections.Generic;

namespace Brady.ScrapRunner.DataService.Tests
{
    /// <summary>
    /// Terminal tests using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class TerminalTests
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
        /// Code to test RetrieveTerminalChangesByRegion,RetrieveTerminalChangesAll
        /// At login time or whenever a terminal is changed, we should send a list of terminals from the 
        /// TerminalChange table
        /// </summary>
        [TestMethod]
        public void RetrieveTerminalChangeUpdates()
        {
            string areaid = "OS";
            string regionid = null;
            string DEFSendOnlyYardsForArea = "Y";
            DateTime dt = new DateTime(2015, 10, 01);
            QueryResult<TerminalChange> queryResult;

            if (DEFSendOnlyYardsForArea == Constants.Yes && areaid != null)
                queryResult = RetrieveTerminalChangesForArea(dt, areaid);
            else if (regionid != null)
                queryResult = RetrieveTerminalChangesForRegion(dt, regionid);
            else
                queryResult = RetrieveTerminalChangesAll(dt);

            foreach (TerminalChange terminalTableInstance in queryResult.Records)
            {
                Assert.IsTrue(terminalTableInstance.ChgDateTime > dt);
            }

            foreach (TerminalChange terminalTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                                                 terminalTableInstance.RegionId,
                                                 terminalTableInstance.TerminalId,
                                                 terminalTableInstance.ChgActionFlag,
                                                 terminalTableInstance.ChgDateTime,
                                                 terminalTableInstance.CustHostCode,
                                                 terminalTableInstance.CustName,
                                                 terminalTableInstance.CustCity,
                                                 terminalTableInstance.CustState));
            }
        }
        /// <summary>
        /// Retrieves terminal change records for a given area since a given date
        /// </summary>
        public QueryResult<TerminalChange> RetrieveTerminalChangesForArea(DateTime dt, string areaid)
        {
            List<string> terminals = BuildTerminalList(areaid);
            var terminalTableQuery = new QueryBuilder<TerminalChange>()
                .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dt)
                .And().Property(x => x.TerminalId).In(terminals.ToArray()))
                .OrderBy(x => x.TerminalId);
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalChange> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            return queryResult;
        }
        
        /// <summary>
        /// Retrieves terminal change records for a given region since a given date
        /// </summary>
        public QueryResult<TerminalChange> RetrieveTerminalChangesForRegion(DateTime dt, string regionid)
        {
            var terminalTableQuery = new QueryBuilder<TerminalChange>()
                .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dt)
                .And().Property(x => x.RegionId).EqualTo(regionid))
                .OrderBy(x => x.TerminalId);
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalChange> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Retrieves terminal change records for a given region since a given date
        /// </summary>
        public QueryResult<TerminalChange> RetrieveTerminalChangesAll(DateTime dt)
        {
            var terminalTableQuery = new QueryBuilder<TerminalChange>()
                .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dt))
                .OrderBy(x => x.TerminalId);
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalChange> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Builds a list of terminals for a given area 
        /// </summary>
        public List<string> BuildTerminalList(string areaid)
        {
            List<string> terminals = new List<string>();
            var areaTableQuery = new QueryBuilder<AreaMaster>()
                .Filter(y => y.Property(x => x.AreaId).EqualTo(areaid));
            string queryString = areaTableQuery.GetQuery();
            QueryResult<AreaMaster> queryResult = _client.QueryAsync(areaTableQuery).Result;
            foreach (AreaMaster areaTableInstance in queryResult.Records)
            {
                terminals.Add(areaTableInstance.TerminalId);
            }
            return terminals;
        } 
        /// <summary>
        /// Code to test RetrieveTerminalMasterByRegion,RetrieveTerminalMasterAll
        /// If a driver's terminal master needs to be reloaded, we should send a list of terminals 
        /// from the TerminalMaster table.
        /// RegionId is an optional argument.
        /// </summary>
        [TestMethod]
        public void RetrieveTerminalMaster()
        {
            string areaid = "NE";
            string regionid = null;
            string DEFSendOnlyYardsForArea = "Y";
            QueryResult<TerminalMaster> queryResult;

            if (DEFSendOnlyYardsForArea == Constants.Yes && areaid != null)
                queryResult = RetrieveTerminalMasterForArea(areaid);
            else if (regionid != null)
                queryResult = RetrieveTerminalMasterForRegion(regionid);
            else
                queryResult = RetrieveTerminalMasterAll();

            foreach (TerminalMaster terminalTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                                 terminalTableInstance.Region,
                                                 terminalTableInstance.TerminalId,
                                                 terminalTableInstance.TerminalName,
                                                 terminalTableInstance.Address1,
                                                 terminalTableInstance.City,
                                                 terminalTableInstance.State));
            }
        }
        /// <summary>
        /// Retrieves terminal master records for a given region 
        /// </summary>
        public QueryResult<TerminalMaster> RetrieveTerminalMasterForArea(string areaid)
        {
            List<string> terminals = BuildTerminalList(areaid);
            var terminalTableQuery = new QueryBuilder<TerminalMaster>()
                .Filter(y => y.Property(x => x.TerminalId).In(terminals.ToArray()))
                .OrderBy(x => x.TerminalName);
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalMaster> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Retrieves terminal master records for a given region 
        /// </summary>
        public QueryResult<TerminalMaster> RetrieveTerminalMasterForRegion(string regionid)
        {
            var terminalTableQuery = new QueryBuilder<TerminalMaster>()
                .Filter(y => y.Property(x => x.Region).EqualTo(regionid))
                .OrderBy(x => x.TerminalName);
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalMaster> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            return queryResult;
        }
        /// <summary>
        /// Retrieves terminal master records for a given region 
        /// </summary>
        public QueryResult<TerminalMaster> RetrieveTerminalMasterAll()
        {
            var terminalTableQuery = new QueryBuilder<TerminalMaster>()
                .OrderBy(x => x.TerminalName);
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalMaster> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            return queryResult;
        }
 
        
        /// <summary>
        /// At login time or whenever a terminal is changed, we should send a list of terminals from the 
        /// TerminalChange table
        /// 
        /// if DefAllowAddRT = Y or DefAllowChangeRT = Y 
        /// then send the list of terminals
        /// otherwise do not send anything
        ///
        /// If Preference DefSendOnlyYardsForArea = Y
        /// then send only the yards for the driver's area
        /// otherwise send only the yards for the driver's region
        /// 
        /// RegionId and AreaId are optional arguments
        /// </summary>
        [TestMethod]
        public void RetrieveTerminalChangeUpdatesRS()
        {
            bool haveFilter = false;

            DateTime? dt = null;
            string regionid = null;
            //string areaid = null;
            //string DefSendOnlyYardsForArea = null;

            //DateTime? dt = new DateTime(2015, 11, 01);
            //string regionid = "SDF";
            //string areaid = "ALL";
            //string areaid = "LI";
            string areaid = "NE";
            string DefSendOnlyYardsForArea = "Y";
            //string DefSendOnlyYardsForArea = "N";

            //To use this query at login, provide the following:
            //chgdatetime from driver's login or driverstatus table
            //driver's preference: DefSendOnlyYardsForArea
            //if DefSendOnlyYardsForArea = "Y", provide driver's area
            //otherwise, provide driver's region
            string terminalString = null;

            //Specify the terminal field name in the TerminalMaster table
            string terminalField = "TerminalId";
            //Specify the base
            string baseString = string.Format("TerminalChanges?");

            //Build the filter string
            string filterString = "$filter=";
            if (dt != null)
            {
                filterString += string.Format("ChgDateTime>datetime({0})", dt);
                haveFilter = true;
            }
            //Build the filter string
            if (DefSendOnlyYardsForArea == Constants.Yes && areaid != null)
            {
                terminalString = BuildFilterArea(terminalField, areaid, null);
                if (terminalString != null)
                {
                    if (haveFilter) filterString += " and ";
                    else haveFilter = true;
                    filterString += string.Format(terminalString);
                }
            }

            if (regionid != null)
            {
                if (haveFilter) filterString += " and ";
                else haveFilter = true;
                filterString += string.Format("RegionId='{0}'", regionid);
            }

            //Build the order by string
            string orderString = "&$orderby = TerminalId";

            //Build the query
            string queryString = baseString;
            if (haveFilter) queryString += filterString;
            queryString += orderString;

            QueryResult queryResult = _client.QueryAsync(queryString).Result;

            Console.WriteLine(string.Format("{0}", queryString));
            foreach (TerminalChange terminalTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                                                 terminalTableInstance.RegionId,
                                                 terminalTableInstance.TerminalId,
                                                 terminalTableInstance.ChgActionFlag,
                                                 terminalTableInstance.ChgDateTime,
                                                 terminalTableInstance.CustHostCode,
                                                 terminalTableInstance.CustName,
                                                 terminalTableInstance.CustCity,
                                                 terminalTableInstance.CustState));
            }
        }
        /// <summary>
        /// If a driver's terminal master needs to be reloaded, we should send a complete
        /// list of terminals from the TerminalMaster table.
        /// 
        /// if DefAllowAddRT = Y or DefAllowChangeRT = Y 
        /// then send the list of terminals
        /// otherwise do not send anything
        /// 
        /// If Preference DefSendOnlyYardsForArea = Y
        /// then send only the yards for the driver's area
        /// otherwise send only the yards for the driver's region
        /// 
        /// RegionId and AreaId are optional arguments
        /// </summary>
        [TestMethod]
        public void RetrieveTerminalsForDriverRS()
        {
            bool haveFilter = false;

            //string regionid = null;
            //string areaid = null;
            //string DefSendOnlyYardsForArea = null;

            string regionid = "SDF";
            //string areaid = "ALL";
            //string areaid = "LI";
            string areaid = "NE";
            string DefSendOnlyYardsForArea = "Y";
            //string DefSendOnlyYardsForArea = "N";

            //To use this query at login, provide the following:
            //driver's preference: DefSendOnlyYardsForArea
            //if DefSendOnlyYardsForArea = "Y", provide driver's area
            //otherwise, provide driver's region
            string terminalString = null;

            //Specify the terminal field name in the TerminalMaster table
            string terminalField = "TerminalId";
            //Specify the base
            string baseString = string.Format("TerminalMasters?");

            //Build the filter string
            string filterString = "$filter=";

            if (DefSendOnlyYardsForArea == Constants.Yes && areaid != null)
            {
                terminalString = BuildFilterArea(terminalField, areaid, null);
                if (terminalString != null)
                {
                    haveFilter = true;
                    filterString += string.Format(terminalString);
                }
            }
            if (regionid != null)
            {
                if (haveFilter) filterString += " and ";
                else haveFilter = true;
                filterString += string.Format("Region='{0}'", regionid);
            }
            //Build the order by string
            string orderString = "&$orderby = TerminalId";

            //Build the query
            string queryString = baseString;
            if (haveFilter) queryString += filterString;
            queryString += orderString;

            QueryResult queryResult = _client.QueryAsync(queryString).Result;


            Console.WriteLine(string.Format("{0}", queryString));
            foreach (TerminalMaster terminalTableInstance in queryResult.Records)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                                 terminalTableInstance.Region,
                                                 terminalTableInstance.TerminalId,
                                                 terminalTableInstance.TerminalName,
                                                 terminalTableInstance.Address1,
                                                 terminalTableInstance.City,
                                                 terminalTableInstance.State));
            }
        }
        /// <summary>
        /// Code to retrieve terminal filter from the TerminalMaster table for a user's area
        /// Arguments will be areaid and optionally terminalid
        /// </summary>
        [TestMethod]
        public void RetrieveTerminalsForArea()
        {
            //string areaid = "ALL";
            //string areaid = "NE";
            string areaid = "LI";
            string terminalid = null;
            //string terminalid = "F1";
            string terminalField = "TerminalId";
            string areaFilter = null;

            if (terminalid != null)
            {
                areaFilter += string.Format("{0}='{1}'", terminalField, terminalid);
            }
            else
            {
                var areaTableQuery = new QueryBuilder<AreaMaster>()
                    .Filter(y => y.Property(x => x.AreaId).EqualTo(areaid));
                string queryString = areaTableQuery.GetQuery();
                QueryResult<AreaMaster> queryResult = _client.QueryAsync(areaTableQuery).Result;
                foreach (AreaMaster areaTableInstance in queryResult.Records)
                {
                    if (areaFilter != null) areaFilter += " or ";
                    areaFilter += string.Format("{0}='{1}'", terminalField, areaTableInstance.TerminalId);
                }
                foreach (AreaMaster areaTableInstance in queryResult.Records)
                {
                    Assert.AreEqual(areaTableInstance.AreaId, areaid, queryString);
                }
            }
            if (areaFilter == null)
            {
                areaFilter = string.Format("{0}=''", terminalField);
            }
            areaFilter = "(" + areaFilter + ")";

            Console.WriteLine(string.Format("{0}", areaFilter));
        }
        /// <summary>
        /// Code to retrieve terminal filter from the TerminalMaster table for a user's region
        /// Arguments will be regionid 
        /// </summary>
        [TestMethod]
        public void RetrieveTerminalsForRegion()
        {
            string regionid = "SDF";
            //string regionid = "EMR";
            string terminalField = "TerminalId";
            string regionFilter = null;
 
            var terminalTableQuery = new QueryBuilder<TerminalMaster>()
                .Filter(y => y.Property(x => x.Region).EqualTo(regionid));
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalMaster> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            foreach (TerminalMaster terminalTableInstance in queryResult.Records)
            {
                if (regionFilter != null) regionFilter += " or ";
                regionFilter += string.Format("{0}='{1}'", terminalField, terminalTableInstance.TerminalId);
            }
            if (regionFilter == null)
            {
                regionFilter = string.Format("{0}=''", terminalField);
            }
            regionFilter = "(" + regionFilter + ")";

            Console.WriteLine(string.Format("{0}", regionFilter));
        }
 

        /// <summary>
        /// Need to move this method to a common area
        /// Method to build a filter of all terminals for a user's area
        /// terminalField is required and consists of the database field name, i.e. "TerminalId"
        /// areaid is required
        /// terminalid is optional 
        /// returns a string to be used as a filter or part of a filter
        /// Ex:  (TerminalId = 'LI' or TerminalId = 'F1")
        /// </summary>
        public string BuildFilterArea(string terminalField, string areaid, string terminalid)
        {
            string areaFilter = null;

            if (terminalid != null)
            {
                areaFilter += string.Format("{0}='{1}'", terminalField, terminalid);
            }
            else
            {
                var areaTableQuery = new QueryBuilder<AreaMaster>()
                    .Filter(y => y.Property(x => x.AreaId).EqualTo(areaid));
                string queryString = areaTableQuery.GetQuery();
                QueryResult<AreaMaster> queryResult = _client.QueryAsync(areaTableQuery).Result;
                foreach (AreaMaster areaTableInstance in queryResult.Records)
                {
                    if (areaFilter != null) areaFilter += " or ";
                    areaFilter += string.Format("{0}='{1}'", terminalField, areaTableInstance.TerminalId);
                }
            }
            if (areaFilter == null)
            {
                areaFilter = string.Format("{0}=''", terminalField);
            }
            areaFilter = "(" + areaFilter + ")";

            return areaFilter;
        }
        /// <summary>
        /// Need to move this method to a common area
        /// Method to build a filter of all terminals for a user's region
        /// terminalField is required and consists of the database field name, i.e. "TerminalId"
        /// regionid is required
        /// returns a string to be used as a filter or part of a filter
        /// Ex:  (TerminalId = 'LI' or TerminalId = 'F1")
        /// </summary>
        public string BuildFilterRegion(string terminalField, string regionid)
        {
            string regionFilter = null;
            var terminalTableQuery = new QueryBuilder<TerminalMaster>()
                .Filter(y => y.Property(x => x.Region).EqualTo(regionid));
            string queryString = terminalTableQuery.GetQuery();
            QueryResult<TerminalMaster> queryResult = _client.QueryAsync(terminalTableQuery).Result;
            foreach (TerminalMaster terminalTableInstance in queryResult.Records)
            {
                if (regionFilter != null) regionFilter += " or ";
                regionFilter += string.Format("{0}='{1}'", terminalField, terminalTableInstance.TerminalId);
            }
            if (regionFilter == null)
            {
                regionFilter = string.Format("{0}=''", terminalField);
            }
            regionFilter = "(" + regionFilter + ")";

            return regionFilter;
        }
    }
}
