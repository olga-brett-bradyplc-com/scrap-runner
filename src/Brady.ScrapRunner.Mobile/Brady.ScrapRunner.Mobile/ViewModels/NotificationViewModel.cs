namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class NotificationViewModel : BaseViewModel
    {
        private int _notificationModelId;

        public void Init(int id)
        {
            _notificationModelId = id;
        }

        public override void Start()
        {
            base.Start();
        }
    }
}
