using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A TripTypeMasterDetails record.
    /// </summary>
    public class TripTypeMasterDetails : IHaveCompositeId, IEquatable<TripTypeMasterDetails>
    {
        public virtual string TripTypeCode { get; set; }
        public virtual int TripTypeSeqNumber { get; set; }
        public virtual string AccessorialCode { get; set; }
        public virtual int? NumberOfContainers { get; set; }
        public virtual string ActivationFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", AccessorialCode, TripTypeCode, TripTypeSeqNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripTypeMasterDetails other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(AccessorialCode, other.AccessorialCode) &&
                   string.Equals(TripTypeCode, other.TripTypeCode) &&
                   TripTypeSeqNumber == other.TripTypeSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripTypeMasterDetails)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AccessorialCode != null ? AccessorialCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TripTypeCode != null ? TripTypeCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TripTypeSeqNumber.GetHashCode();
                return hashCode;
            }
        }
    }
}
