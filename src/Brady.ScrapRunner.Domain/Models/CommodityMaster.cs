using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A CommodityMaster record.
    /// </summary>
    public class CommodityMaster : IHaveCompositeId, IEquatable<CommodityMaster>
    {
        public virtual string CommodityCode { get; set; }
        public virtual string CommodityDesc { get; set; }
        public virtual string InventoryCode { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual string DestCustHostCode { get; set; }
        public virtual string DestContainerLocation { get; set; }
        public virtual string InactiveFlag { get; set; }
        public virtual string UniversalFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0}", CommodityCode);
            }
            set
            {

            }
        }

        public virtual bool Equals(CommodityMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CommodityCode, other.CommodityCode) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommodityMaster) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CommodityCode != null ? CommodityCode.GetHashCode() : 0);
                return hashCode;
            }
        }

    }

}
