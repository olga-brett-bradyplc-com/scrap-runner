using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A Messages record.
    /// </summary>
    public class Messages : IHaveId<int>, IEquatable<Messages>
    {
        public virtual int MsgId { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual DateTime CreateDateTime { get; set; }
        public virtual string SenderId { get; set; }
        public virtual string ReceiverId { get; set; }
        public virtual string MsgText { get; set; }
        public virtual string Ack { get; set; }
        public virtual int MsgThread { get; set; }
        public virtual string Area { get; set; }
        public virtual string SenderName { get; set; }
        public virtual string ReceiverName { get; set; }
        public virtual string Urgent { get; set; }
        public virtual string Processed { get; set; }
        public virtual string MsgSource { get; set; }
        public virtual string DeleteFlag { get; set; }

        public virtual int Id
        {
            get
            {
                return MsgId;
            }
            set
            {
                
            }
        }

        public virtual bool Equals(Messages other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MsgId == other.MsgId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Messages)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MsgId.GetHashCode();
                return hashCode;
            }
        }
    }
}
