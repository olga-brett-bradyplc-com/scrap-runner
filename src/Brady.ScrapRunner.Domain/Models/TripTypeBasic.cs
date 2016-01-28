using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Brady.ScrapRunner.Domain.Models
{
    public class TripTypeBasic : IHaveCompositeId, IEquatable<TripTypeBasic>
    {
        public virtual string TripTypeCode { get; set; }
        public virtual string TripTypeDesc { get; set; }
        public virtual string TripTypeHostCode { get; set; }
        public virtual string TripTypeHostCodeScale { get; set; }
        public virtual int? TripTypeStandardMinutes { get; set; }

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

        public virtual bool Equals(TripTypeBasic other)
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
            return Equals((TripTypeBasic) obj);
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
