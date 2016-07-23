using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A TripTypeBasicDetails record.
    /// </summary>
    public class TripTypeBasicDetails : IHaveId<int>, IEquatable<TripTypeBasicDetails>
    {
        public virtual string TripTypeCode { get; set; }
        public virtual int SeqNo { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual int? FirstCTRTime { get; set; }
        public virtual int? SecondCTRTime { get; set; }
  
        public virtual int Id
        {
            get
            {
                return SeqNo;
            }
            set
            {

            }
        }

        public virtual bool Equals(TripTypeBasicDetails other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ContainerType, other.ContainerType) &&
                   SeqNo == other.SeqNo &&
                   string.Equals(TripTypeCode, other.TripTypeCode);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripTypeBasicDetails)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = SeqNo.GetHashCode();
            return hashCode;
        }
    }
}
