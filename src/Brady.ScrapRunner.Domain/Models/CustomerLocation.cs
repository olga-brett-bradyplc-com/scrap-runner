using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A CustomerLocation record.
    /// </summary>
    public class CustomerLocation : IHaveCompositeId, IEquatable<CustomerLocation>
    {
        public virtual string CustHostCode { get; set; }
        public virtual string CustLocation { get; set; }
        public virtual int? CustStandardMinutes { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", CustHostCode, CustLocation);
            }
            set
            {

            }
        }

        public virtual bool Equals(CustomerLocation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CustHostCode, other.CustHostCode) &&
                   string.Equals(CustLocation, other.CustLocation) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomerLocation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CustHostCode != null ? CustHostCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CustLocation != null ? CustLocation.GetHashCode() : 0);
                return hashCode;
            }
        }

    }

}
