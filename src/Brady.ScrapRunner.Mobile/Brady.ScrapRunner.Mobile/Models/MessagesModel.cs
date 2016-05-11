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

    }
}

