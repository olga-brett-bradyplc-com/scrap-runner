namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IQueueScheduler
    {
        void Schedule(int timeoutMillis);
        void Unschedule();
    }
}