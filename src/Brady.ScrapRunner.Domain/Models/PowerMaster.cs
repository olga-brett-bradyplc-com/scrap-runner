using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A PowerMaster record.  FIXME:  Remove the Id workaround for NHibernate 
    /// </summary>
    public class PowerMaster : IHaveCompositeId, IEquatable<PowerMaster>
    {
        public virtual string PowerId { get; set; }
        public virtual string PowerType { get; set; }
        public virtual string PowerDesc { get; set; }
        public virtual string PowerSize { get; set; }
        public virtual int? PowerLength { get; set; }
        public virtual int? PowerTareWeight { get; set; }
        public virtual string PowerCustHostCode { get; set; }
        public virtual string PowerCustType { get; set; }
        public virtual string PowerTerminalId { get; set; }
        public virtual string PowerRegionId { get; set; }
        public virtual string PowerLocation { get; set; }
        public virtual string PowerStatus { get; set; }
        public virtual DateTime? PowerDateOutOfService { get; set; }
        public virtual DateTime? PowerDateInService { get; set; }
        public virtual string PowerDriverId { get; set; }
        public virtual int? PowerOdometer { get; set; }
        public virtual string PowerComments { get; set; }
        public virtual string MdtId { get; set; }
        public virtual string PrimaryContainerType { get; set; }
        public virtual string OrigTerminalId { get; set; }
        public virtual DateTime? PowerLastActionDateTime { get; set; }
        public virtual string PowerCurrentTripNumber { get; set; }
        public virtual string PowerCurrentTripSegNumber { get; set; }
        public virtual string PowerCurrentTripSegType { get; set; }
        public virtual string PowerAssetNumber { get; set; }
        public virtual string PowerIdHost { get; set; }

        public virtual string Id
        {
            get
            {
                return PowerId;
            }
            set
            {
            }
        }

        public virtual bool Equals(PowerMaster other)
        {
            return string.Equals(PowerId, other.PowerId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PowerMaster)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PowerId != null ? PowerId.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ (Id?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

    }
}
