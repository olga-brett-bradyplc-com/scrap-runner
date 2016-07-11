namespace Brady.ScrapRunner.Mobile.Interfaces
{
    /// <summary>
    /// For scheduling background tasks including: polling for route changes, messages, and queue management.
    /// </summary>
    public interface IBackgroundScheduler
    {
        void Schedule(int timeoutMillis);
        void Unschedule();
    }
}