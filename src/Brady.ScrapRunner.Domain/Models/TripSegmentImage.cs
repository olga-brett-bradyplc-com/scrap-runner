using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A TripSegmentImage record.
    /// </summary>

    public class TripSegmentImage : IHaveCompositeId, IEquatable<TripSegmentImage>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual int TripSegImageSeqId { get; set; }
        public virtual DateTime? TripSegImageActionDateTime { get; set; }
        public virtual string TripSegImageLocation { get; set; }
        public virtual string TripSegImagePrintedName { get; set; }
        public virtual string TripSegImageType { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", TripNumber, TripSegImageSeqId, TripSegNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripSegmentImage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripNumber, other.TripNumber)
                && TripSegImageSeqId == other.TripSegImageSeqId
                && string.Equals(TripSegNumber, other.TripSegNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentImage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripNumber != null ? TripNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TripSegImageSeqId.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripSegNumber != null ? TripSegNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
