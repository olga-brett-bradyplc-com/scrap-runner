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
        private string HostName => "https://maunb-jtw10.bradyplc.com:7776";

        public bool CreateConnection(string username, string password, string dataService = null)
        {
            // @TODO : Hardcoding this for now against the Brady Membership tables, while authing
            // @TODO : against a valid EmployeeMaster record
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
            return (DataServiceClient) Connection;
        }
    }
}
