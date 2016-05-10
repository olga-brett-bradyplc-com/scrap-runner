namespace Brady.ScrapRunner.Mobile.Services
{
    using BWF.DataServices.PortableClients;
    using BWF.DataServices.PortableClients.Interfaces;
    using Interfaces;

    public class ConnectionService : IConnectionService<QueuedDataServiceClient>
    {
        private IDataServiceClient Connection { get; set; }
        
        public bool CreateConnection(string hosturl, string username, string password, string dataService = null)
        {
            Connection = new QueuedDataServiceClient(hosturl, username, password, dataService);
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

        public QueuedDataServiceClient GetConnection()
        {
            return (QueuedDataServiceClient) Connection;
        }
    }
}
