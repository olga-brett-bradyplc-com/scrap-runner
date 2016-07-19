namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;

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

        private DateTimeOffset _notificationDateTime;
        public DateTimeOffset NotificationDateTime
        {
            get { return _notificationDateTime; }
            set { SetProperty(ref _notificationDateTime, value); }
        }

        private MvxCommand<NotificationModel> _selectNotificationCommand;
        public IMvxCommand ScanContainerCommand => _selectNotificationCommand ?? (_selectNotificationCommand = new MvxCommand<NotificationModel>(SelectNotification));

        private void SelectNotification(NotificationModel notification)
        {
            NotificationSummary = notification.Summary;
            NotificationDateTime = notification.NotificationDateTimeOffset;
        }
    }
}