using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A CustomerCommodity record.
    /// </summary>
    public class CustomerCommodity : IHaveCompositeId, IEquatable<CustomerCommodity>
    {
        public virtual string CustHostCode { get; set; }
        public virtual string CustCommodityCode { get; set; }
        public virtual string MasterCommodityCode { get; set; }
        public virtual string CustCommodityDesc { get; set; }
        public virtual string CustContainerType { get; set; }
        public virtual string CustContainerSize { get; set; }
        public virtual string CustContainerLocation { get; set; }
        public virtual string DestCustHostCode { get; set; }
        public virtual string DestContainerLocation { get; set; }
        public virtual DateTime? DestExpirationDate { get; set; }
        public virtual int? CustStandardMinutes { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", CustCommodityCode, CustHostCode) ;
            }
            set
            {

            }
        }

        public virtual bool Equals(CustomerCommodity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CustCommodityCode, other.CustCommodityCode) &&
                   string.Equals(CustHostCode, other.CustHostCode) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomerCommodity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CustCommodityCode != null ? CustCommodityCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CustHostCode != null ? CustHostCode.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
