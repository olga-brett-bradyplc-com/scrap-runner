using System;

namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("Messages")]
    public class MessagesModel
    {
        [MaxLength(10)]
        public string TerminalId { get; set; }

        [PrimaryKey]
        public int? MsgId { get; set; }

        public DateTime CreateDateTime { get; set; }

        [MaxLength(10)]
        public string SenderId { get; set; }

        [MaxLength(10)]
        public string ReceiverId { get; set; }

        [MaxLength(512)]
        public string MsgText { get; set; }

        [MaxLength(1)]
        public string Ack { get; set; }

        public int? MessageThread { get; set; }

        [MaxLength(1)]
        public string Area { get; set; }

        [MaxLength(25)]
        public string SenderName { get; set; }

        [MaxLength(25)]
        public string ReceiverName { get; set; }

        [MaxLength(1)]
        public string Urgent { get; set; }

        [MaxLength(1)]
        public string Processed { get; set; }

        [MaxLength(1)]
        public string MsgSource { get; set; }

        [MaxLength(1)]
        public string DeleteFlag { get; set; }

        // Helper fields that help us determine if the SenderId or RecieverId is the current driver id
        // This is used for display purposes mostly
        // As of now, LocalUser needs to be manually set for each message object in order for this properties to function correctly.
        [Ignore]
        public string LocalUser { get; set; }

        [Ignore]
        public string RemoteUserId => LocalUser == SenderId ? ReceiverId : SenderId;

        [Ignore]
        public string RemoteUserName => LocalUser == SenderId ? ReceiverName : SenderName;
    }
}

