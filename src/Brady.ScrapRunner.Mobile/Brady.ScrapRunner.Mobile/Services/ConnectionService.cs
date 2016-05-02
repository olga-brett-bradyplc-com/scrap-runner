namespace Brady.ScrapRunner.Mobile.Services
{
    using BWF.DataServices.PortableClients;
    using BWF.DataServices.PortableClients.Interfaces;
    using Interfaces;

    public class ConnectionService : IConnectionService<OfflineCapableDataServiceClient>
    {
        private IDataServiceClient Connection { get; set; }
        
        public bool CreateConnection(string hosturl, string username, string password, string dataService = null)
        {
            Connection = new OfflineCapableDataServiceClient(hosturl, username, password, dataService);
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

        public OfflineCapableDataServiceClient GetConnection()
        {
            return (OfflineCapableDataServiceClient) Connection;
        }
    }
}
