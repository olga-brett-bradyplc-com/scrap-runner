using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A TripSegmentContainerTime record.
    /// </summary>

    public class TripSegmentContainerTime : IHaveCompositeId, IEquatable<TripSegmentContainerTime>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual int TripSegContainerSeqNumber { get; set; }
        public virtual int SeqNumber { get; set; }
        public virtual string TimeType { get; set; }
        public virtual string TimeDesc { get; set; }
        public virtual int ContainerTime { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2};{3}", SeqNumber, TripNumber, TripSegContainerSeqNumber, TripSegNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripSegmentContainerTime other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SeqNumber == other.SeqNumber
                && string.Equals(TripNumber, other.TripNumber)
                && TripSegContainerSeqNumber == other.TripSegContainerSeqNumber
                && string.Equals(TripSegNumber, other.TripSegNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentContainerTime)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SeqNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripNumber != null ? TripNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SeqNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripSegNumber != null ? TripSegNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
