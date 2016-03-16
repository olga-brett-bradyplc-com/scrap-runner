using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A DriverDelay record.
    /// </summary>
    public class DriverDelay : IHaveCompositeId, IEquatable<DriverDelay>
    {
        public virtual string DriverId { get; set; }
        public virtual int DelaySeqNumber { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual string DriverName { get; set; }
        public virtual string DelayCode { get; set; }
        public virtual string DelayReason { get; set; }
        public virtual DateTime? DelayStartDateTime { get; set; }
        public virtual DateTime? DelayEndDateTime { get; set; }
        public virtual int? DelayLatitude { get; set; }
        public virtual int? DelayLongitude { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string TerminalName { get; set; }
        public virtual string RegionName { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", DelaySeqNumber, DriverId, TripNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(DriverDelay other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DelaySeqNumber == other.DelaySeqNumber 
                && string.Equals(DriverId, other.DriverId)
                && string.Equals(TripNumber, other.TripNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverDelay) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DelaySeqNumber.GetHashCode();
                hashCode = (hashCode*397) ^ (DriverId != null ? DriverId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (TripNumber != null ? TripNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
