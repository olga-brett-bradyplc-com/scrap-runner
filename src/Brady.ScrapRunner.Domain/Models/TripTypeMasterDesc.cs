using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A TripTypeMasterDesc record.
    /// </summary>

    public class TripTypeMasterDesc : IHaveCompositeId, IEquatable<TripTypeMasterDesc>
    {
        public virtual string TripTypeCode { get; set; }
        public virtual string TripTypeDesc { get; set; }
        public virtual string TripTypePurchase { get; set; }
        public virtual string TripTypeSale { get; set; }
        public virtual string TripTypeScaleMsg { get; set; }
        public virtual string TripTypeCompTripMsg { get; set; }

        public virtual string Id
        {
            get
            {
                return TripTypeCode;
            }
            set
            {

            }
        }

        public virtual bool Equals(TripTypeMasterDesc other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripTypeCode, other.TripTypeCode);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripTypeMasterDesc)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripTypeCode != null ? TripTypeCode.GetHashCode() : 0);
                //hashCode = (hashCode*397) ^ (TripTypeBasicSegNumber != null ? TripTypeBasicSegNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
