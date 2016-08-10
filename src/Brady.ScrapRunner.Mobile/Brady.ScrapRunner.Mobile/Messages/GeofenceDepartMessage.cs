namespace Brady.ScrapRunner.Mobile.Messages
{
    using MvvmCross.Plugins.Messenger;

    public class GeofenceDepartMessage : MvxMessage
    {
        public GeofenceDepartMessage(object sender) : base(sender)
        {
        }

        public string Key { get; set; }
    }
}