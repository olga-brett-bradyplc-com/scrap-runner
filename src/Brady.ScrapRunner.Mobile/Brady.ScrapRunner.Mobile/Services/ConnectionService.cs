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
        
        public bool CreateConnection(string hosturl, string username, string password, string dataService = null)
        {
            Connection = new DataServiceClient(hosturl, username, password, dataService);
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
