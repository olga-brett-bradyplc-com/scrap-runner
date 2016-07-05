namespace Brady.ScrapRunner.Mobile.Messages
{
    using Domain.Models;
    using Interfaces;
    using MvvmCross.Plugins.Messenger;

    public class TripNotificationMessage : MvxMessage
    {
        public TripNotificationMessage(object sender) : base(sender)
        {
        }

        public TripNotificationContext Context { get; set; }
        public Trip Trip { get; set; }
    }
}
