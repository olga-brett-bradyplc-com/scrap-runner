using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A CommodityMasterDest record.
    /// </summary>
     
    public class CommodityMasterDest : IHaveCompositeId, IEquatable<CommodityMasterDest>
    {
        public virtual string CommodityCode { get; set; }
        public virtual string DestTerminalId { get; set; }
        public virtual string DestCustHostCode { get; set; }
        public virtual string DestContainerLocation { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", CommodityCode, DestTerminalId);
            }
            set
            {

            }
        }

        public virtual bool Equals(CommodityMasterDest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CommodityCode, other.CommodityCode) &&
                   string.Equals(DestTerminalId, other.DestTerminalId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommodityMasterDest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CommodityCode != null ? CommodityCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DestTerminalId != null ? DestTerminalId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
