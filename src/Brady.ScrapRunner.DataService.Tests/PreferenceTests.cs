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
using System.Collections.Generic;

namespace Brady.ScrapRunner.DataService.Tests
{
    /// <summary>
    /// Preference tests using the BWF DataServices PortableClient.
    /// </summary>
    [TestClass]
    public class PreferenceTests
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
        ///  Get a simple list of all preferences for a terminalId.
        /// <returns>An empty list if termianlId is null</returns>
        public static List<Preference> GetPreferences(string terminalId)
        {
            List<Preference> preferences = new List<Preference>();
            if (null != terminalId)
            {
                // "Preferences?$filter= TerminalId='{0}'", terminalId
                var currentQuery = new QueryBuilder<Preference>()
                     .Filter(t => t.Property(p => p.TerminalId).EqualTo(terminalId));
                string queryString = currentQuery.GetQuery();
                QueryResult<Preference> queryResult = _client.QueryAsync(currentQuery).Result;
                foreach (Preference preferenceTableInstance in queryResult.Records)
                {
                    preferences.Add(preferenceTableInstance);
                }
            }
            return preferences;
        }
        /// <summary>
        /// Get a list of preferences
        /// </summary>
        [TestMethod]
        public void RetrievePreferences()
        {
            string terminalid = "LI";
            List<Preference> preferences = GetPreferences(terminalid);
            foreach (Preference preferenceInstance in preferences)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}",
                    preferenceInstance.TerminalId,
                    preferenceInstance.Parameter,
                    preferenceInstance.ParameterValue,
                    preferenceInstance.Description));
            }
        }
    }
}
