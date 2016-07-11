namespace Brady.ScrapRunner.Mobile.Messages
{
    using Domain.Models;
    using MvvmCross.Plugins.Messenger;

    public class NewMessagesMessage : MvxMessage
    {
        public NewMessagesMessage(object sender) : base(sender)
        {
        }

        public Messages Message { get; set; }
    }
}
