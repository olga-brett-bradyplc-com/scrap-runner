namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface INotificationService
    {
        INotification Create(object id, object icon, string title, string text);
        void Notify(INotification notification);
        void Cancel(object notificationId);
        void CancelAll();
    }

    public interface INotification
    {
        object Id { get; set; }
        object Icon { get; set; }
        string Title { get; set; }
        string Text { get; set; }
        object Context { get; set; }
    }
}