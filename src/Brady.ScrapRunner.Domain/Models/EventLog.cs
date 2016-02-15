using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A EventLog record.
    /// </summary>
    public class EventLog : IHaveId<int>, IEquatable<EventLog>
    {
        // The new key populated by autoincrement column
        public virtual int EventId { get; set;}
        public virtual DateTime EventDateTime { get; set; }
        public virtual int EventSeqNo { get; set; }
        public virtual string EventTerminalId { get; set; }
        public virtual string EventRegionId { get; set; }
        public virtual string EventEmployeeId { get; set; }
        public virtual string EventEmployeeName { get; set; }
        public virtual string EventTripNumber { get; set; }
        public virtual string EventProgram { get; set; }
        public virtual string EventScreen { get; set; }
        public virtual string EventAction { get; set; }
        public virtual string EventComment { get; set; }

        public virtual int Id
        {
            get
            {
                return EventId;
            }
            set
            {

            }
        }

        public virtual bool Equals(EventLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EventId == other.EventId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EventLog) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EventId.GetHashCode() ;
                return hashCode;
            }
        }
    }

}
