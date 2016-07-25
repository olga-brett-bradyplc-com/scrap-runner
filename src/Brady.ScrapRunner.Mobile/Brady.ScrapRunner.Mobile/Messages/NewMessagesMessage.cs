namespace Brady.ScrapRunner.Mobile.Messages
{
    using Models;
    using MvvmCross.Plugins.Messenger;

    public class NewMessagesMessage : MvxMessage
    {
        public NewMessagesMessage(object sender) : base(sender)
        {
        }

        public MessagesModel Message { get; set; }
    }
}
