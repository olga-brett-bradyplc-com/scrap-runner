namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using BWF.DataServices.PortableClients;
    using BWF.DataServices.PortableClients.Interfaces;
    using Interfaces;
    using MvvmCross.Platform;

    public class ConnectionService : IConnectionService
    {
        private DataServiceClient _dataServiceClient;
        private QueuedDataServiceClient _queuedDataServiceClient;

        public void CreateConnection(string hosturl, string username, string password, string dataService = null)
        {
            _dataServiceClient = new DataServiceClient(hosturl, username, password, dataService);
            _queuedDataServiceClient = new QueuedDataServiceClient(
                Mvx.Resolve<INetworkAvailabilityService>(),
                Mvx.Resolve<IQueueService>(),
                _dataServiceClient);
        }

        public void DeleteConnection()
        {
            _queuedDataServiceClient.Dispose();
            _dataServiceClient.Dispose();
        }

        public IDataServiceClient GetConnection(ConnectionType connectionType = ConnectionType.Online)
        {
            switch (connectionType)
            {
                case ConnectionType.Online:
                    return _dataServiceClient;
                case ConnectionType.Offline:
                    return _queuedDataServiceClient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}