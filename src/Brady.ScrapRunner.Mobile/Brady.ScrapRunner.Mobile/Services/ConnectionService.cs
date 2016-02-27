using System;
using System.Net;
using Brady.ScrapRunner.Mobile.Interfaces;
using BWF.DataServices.PortableClients;
using BWF.DataServices.PortableClients.Interfaces;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class ConnectionService : IConnectionService<DataServiceClient>
    {
        private IDataServiceClient Connection { get; set; }

        // @TODO : Set up configuration files/PCL to handle this
        private string HostName => "https://maunb-jtw10.bradyplc.com:7776";

        public bool CreateConnection(string username, string password, string dataService = null)
        {
            // @TODO : Hardcoding BWF username/password, then we check for a valid EmployeeMaster record at login
            Connection = new DataServiceClient(HostName, "admin", "mem_2014", dataService);
            return true;
        }

        public void DeleteConnection()
        {
            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }

        public DataServiceClient GetConnection()
        {
            // @TODO : Add validations to check to make sure a valid connection has been made, etc.
            return (DataServiceClient) Connection;
        }
    }
}
