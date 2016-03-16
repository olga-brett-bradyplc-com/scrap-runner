using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
 
    /// <summary>
    /// A TripTypeAllowCustType record.
    /// </summary>
    public class TripTypeAllowCustType : IHaveCompositeId, IEquatable<TripTypeAllowCustType>
    {
        public virtual string TripTypeCode { get; set; }
        public virtual int TripTypeSeqNumber { get; set; }
        public virtual string TripTypeCustType { get; set; }
 
        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", TripTypeCode, TripTypeCustType, TripTypeSeqNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripTypeAllowCustType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripTypeCode, other.TripTypeCode) &&
                   string.Equals(TripTypeCustType, other.TripTypeCustType) && 
                   TripTypeSeqNumber == other.TripTypeSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripTypeAllowCustType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripTypeCode != null ? TripTypeCode.GetHashCode() : 0);
                hashCode = (TripTypeCustType != null ? TripTypeCustType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TripTypeSeqNumber.GetHashCode();
                return hashCode;
            }
        }
    }
}
