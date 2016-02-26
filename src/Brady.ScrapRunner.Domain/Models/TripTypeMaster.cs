using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A TripTypeMaster record.
    /// </summary>
    public class TripTypeMaster : IHaveCompositeId, IEquatable<TripTypeMaster>
    {
        public virtual string TripTypeCode { get; set; }
        public virtual int TripTypeSeqNumber { get; set; }
        public virtual string TripTypeCodeBasic { get; set; }
        public virtual string CopyContainerId { get; set; }
        public virtual string CopyContainerType { get; set; }
        public virtual string CopyContainerSize { get; set; }
        public virtual string CopyCustomerLocation { get; set; }
        public virtual string CopyCommodityType { get; set; }
        public virtual string CopyCommoditySaleCustomer { get; set; }
        public virtual string UseCommodityTime { get; set; }
        public virtual string UseLocationTime { get; set; }
        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", TripTypeCode, TripTypeSeqNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripTypeMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripTypeCode, other.TripTypeCode) &&
                   TripTypeSeqNumber == other.TripTypeSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripTypeMaster)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripTypeCode != null ? TripTypeCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TripTypeSeqNumber.GetHashCode();
                return hashCode;
            }
        }
    }
}
