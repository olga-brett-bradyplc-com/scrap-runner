namespace Brady.ScrapRunner.Mobile.Messages
{
    using Models;
    using MvvmCross.Plugins.Messenger;

    public class LocationModelMessage : MvxMessage
    {
        public LocationModelMessage(object sender) : base(sender)
        {
        }

        public LocationModel Location { get; set; }
    }
}
