using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A RegionMaster record.
    /// </summary>
    public class RegionMaster : IHaveCompositeId, IEquatable<RegionMaster>
    {
        public virtual string RegionId { get; set; }
        public virtual string RegionName { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }
        public virtual string Country { get; set; }
        public virtual string Phone { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0}", RegionId);
            }
            set
            {

            }
        }

        public virtual bool Equals(RegionMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(RegionId, other.RegionId) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RegionMaster) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RegionId != null ? RegionId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
