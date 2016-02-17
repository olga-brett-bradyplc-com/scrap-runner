using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    public class CustomerDirections : IHaveCompositeId,IEquatable<CustomerDirections>
    {
        public virtual string CustHostCode { get; set; }
        public virtual int DirectionsSeqNo { get; set; }
        public virtual string DirectionsDesc { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", CustHostCode, DirectionsSeqNo);
            }
            set
            {
            }
        }

        public virtual bool Equals(CustomerDirections other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CustHostCode, other.CustHostCode) &&
                   DirectionsSeqNo == other.DirectionsSeqNo;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomerDirections)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CustHostCode != null ? CustHostCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DirectionsSeqNo.GetHashCode();
                return hashCode;
            }
        }
    }
}
