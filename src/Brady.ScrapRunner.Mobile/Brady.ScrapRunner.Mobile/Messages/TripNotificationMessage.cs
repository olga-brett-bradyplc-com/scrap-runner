namespace Brady.ScrapRunner.Mobile.Messages
{
    using Interfaces;
    using Models;
    using MvvmCross.Plugins.Messenger;

    public class TripNotificationMessage : MvxMessage
    {
        public TripNotificationMessage(object sender) : base(sender)
        {
        }

        public TripNotificationContext Context { get; set; }
        public TripModel Trip { get; set; }
    }
}
