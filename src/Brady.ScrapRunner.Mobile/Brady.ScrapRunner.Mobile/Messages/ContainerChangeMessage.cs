namespace Brady.ScrapRunner.Mobile.Messages
{
    using Domain.Models;
    using MvvmCross.Plugins.Messenger;

    public class ContainerChangeMessage : MvxMessage
    {
        public ContainerChangeMessage(object sender) : base(sender)
        {
        }

        public ContainerChange Change { get; set; }
    }
}
