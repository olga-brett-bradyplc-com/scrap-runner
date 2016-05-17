namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using BWF.DataServices.PortableClients.Interfaces;

    /// <summary>
    /// A "service" wrapper around connections (i.e. BWF portable client.  This should allow Xamarin/Android/MvvmCross
    /// to conveniently treat the client as a singleton with application wide visibility.
    /// @TODO: Lifecycle and possible restarts? 
    /// </summary>
    public interface IConnectionService
    {
        void CreateConnection(string hosturl, string username, string password, string dataService = null);

        void DeleteConnection();

        IDataServiceClient GetConnection(ConnectionType connectionType = ConnectionType.Online);
    }

    /// <summary>
    /// Enumerates the types of available connections.
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// The connection will make live web service calls. Data is lost if the web service request fails.
        /// </summary>
        Online,
        /// <summary>
        /// The connection will first attempt live web service calls. 
        /// If the live web service call fails then the request will be queued and eventually sent in the background.
        /// </summary>
        Offline
    }
}