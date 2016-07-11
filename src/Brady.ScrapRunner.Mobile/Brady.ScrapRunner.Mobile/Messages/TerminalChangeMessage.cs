namespace Brady.ScrapRunner.Mobile.Messages
{
    using Domain.Models;
    using MvvmCross.Plugins.Messenger;

    public class TerminalChangeMessage : MvxMessage
    {
        public TerminalChangeMessage(object sender) : base(sender)
        {
        }

        public TerminalChange Change { get; set; }
    }
}
