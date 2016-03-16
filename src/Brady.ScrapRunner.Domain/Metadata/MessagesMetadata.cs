using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class MessagesMetadata : TypeMetadataProvider<Messages>
    {
        public MessagesMetadata()
        {

            AutoUpdatesByDefault();

            IntegerProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            IntegerProperty(x => x.MsgId)
                .IsId()
                .IsNotEditableInGrid()
                .DisplayName("Msg Id");

            StringProperty(x => x.TerminalId);
            TimeProperty(x => x.CreateDateTime);
            StringProperty(x => x.SenderId);
            StringProperty(x => x.ReceiverId);
            StringProperty(x => x.MsgText);
            StringProperty(x => x.Ack);
            IntegerProperty(x => x.MsgThread);
            StringProperty(x => x.Area);
            StringProperty(x => x.SenderName);
            StringProperty(x => x.ReceiverName);
            StringProperty(x => x.Urgent);
            StringProperty(x => x.Processed);
            StringProperty(x => x.MsgSource);
            StringProperty(x => x.DeleteFlag);

            ViewDefaults()
                .Property(x => x.TerminalId)
                .Property(x => x.MsgId)
                .Property(x => x.CreateDateTime)
                .Property(x => x.SenderId)
                .Property(x => x.ReceiverId)
                .Property(x => x.MsgText)
                .Property(x => x.Ack)
                .Property(x => x.MsgThread)
                .Property(x => x.Area)
                .Property(x => x.SenderName)
                .Property(x => x.ReceiverName)
                .Property(x => x.Urgent)
                .Property(x => x.Processed)
                .Property(x => x.MsgSource)
                .Property(x => x.DeleteFlag)
                .OrderBy(x => x.MsgId);
        }
    }
}
