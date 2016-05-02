namespace Brady.ScrapRunner.Mobile.Interfaces
{

    /// <summary>
    /// A "service" wrapper around connections (i.e. BWF portable client.  This should allow Xamarin/Android/MvvmCross
    /// to conveniently treat the clinet as a singleton with application wide visibility.
    /// @TODO: Lifecycle and possible restarts? 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConnectionService<T> where T : class
    {
        bool CreateConnection(string hosturl, string username, string password, string dataService = null);

        void DeleteConnection();

        T GetConnection();
    }
}
