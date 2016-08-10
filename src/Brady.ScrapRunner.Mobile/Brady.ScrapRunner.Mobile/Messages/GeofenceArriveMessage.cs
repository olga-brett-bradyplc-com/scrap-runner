namespace Brady.ScrapRunner.Mobile.Messages
{
    using MvvmCross.Plugins.Messenger;

    public class GeofenceArriveMessage : MvxMessage
    {
        public GeofenceArriveMessage(object sender) : base(sender)
        {
        }

        public string Key { get; set; }
    }
}