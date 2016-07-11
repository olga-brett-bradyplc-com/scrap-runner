namespace Brady.ScrapRunner.Mobile.Messages
{
    using MvvmCross.Plugins.Messenger;

    public class ForceLogoffMessage : MvxMessage
    {
        public ForceLogoffMessage(object sender) : base(sender)
        {
        }
    }
}
