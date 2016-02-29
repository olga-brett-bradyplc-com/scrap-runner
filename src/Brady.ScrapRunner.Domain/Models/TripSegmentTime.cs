using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A TripSegmentTime record.
    /// </summary>

    public class TripSegmentTime : IHaveCompositeId, IEquatable<TripSegmentTime>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual int SeqNumber { get; set; }
        public virtual string TimeType { get; set; }
        public virtual string TimeDesc { get; set; }
        public virtual int SegmentTime { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", SeqNumber, TripNumber, TripSegNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripSegmentTime other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SeqNumber == other.SeqNumber
                && string.Equals(TripNumber, other.TripNumber)
                && string.Equals(TripSegNumber, other.TripSegNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentTime)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SeqNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripNumber != null ? TripNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TripSegNumber != null ? TripSegNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
