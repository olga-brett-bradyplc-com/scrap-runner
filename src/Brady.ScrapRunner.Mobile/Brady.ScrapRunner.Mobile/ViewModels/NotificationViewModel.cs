namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class NotificationViewModel : BaseViewModel
    {
        private readonly IRepository<NotificationModel> _repository;

        public NotificationViewModel(IRepository<NotificationModel> repository)
        {
            _repository = repository;
        }

        public override async void Start()
        {
            base.Start();
            Title = AppResources.Notifications;
            var notifications = await _repository.AsQueryable().ToListAsync();
            Notifications = new ObservableCollection<NotificationModel>(notifications);
        }

        private ObservableCollection<NotificationModel> _notifications;
        public ObservableCollection<NotificationModel> Notifications
        {
            get { return _notifications; }
            set { SetProperty(ref _notifications, value); }
        }

        private string _notificationSummary;
        public string NotificationSummary
        {
            get { return _notificationSummary; }
            set { SetProperty(ref _notificationSummary, value); }
        }

        private MvxCommand<NotificationModel> _selectNotificationCommand;
        public IMvxCommand SelectNotificationCommand => _selectNotificationCommand ?? 
            (_selectNotificationCommand = new MvxCommand<NotificationModel>(n => NotificationSummary = n.Summary));
    }
}