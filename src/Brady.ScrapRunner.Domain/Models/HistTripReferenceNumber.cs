using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A HistTripReferenceNumber record.
    /// </summary>
    public class HistTripReferenceNumber : IHaveCompositeId, IEquatable<HistTripReferenceNumber>
    {
        public virtual int HistSeqNo { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual int TripSeqNumber { get; set; }
        public virtual string TripRefNumberDesc { get; set; }
        public virtual string TripRefNumber { get; set; }
        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", HistSeqNo,TripNumber, TripSeqNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(HistTripReferenceNumber other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HistSeqNo == other.HistSeqNo &&
                   string.Equals(TripNumber, other.TripNumber) &&
                   TripSeqNumber == other.TripSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HistTripReferenceNumber)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HistSeqNo.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripNumber != null ? TripNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TripSeqNumber.GetHashCode();
                return hashCode;
            }
        }
    }
}
