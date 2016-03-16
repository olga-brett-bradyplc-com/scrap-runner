using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// A PowerLimits record.
    /// </summary>

    public class PowerLimits : IHaveCompositeId, IEquatable<PowerLimits>
    {
        public virtual string PowerId { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual int PowerSeqNumber { get; set; }
        public virtual string ContainerMinSize { get; set; }
        public virtual string ContainerMaxSize { get; set; }


        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", ContainerType, PowerId, PowerSeqNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(PowerLimits other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ContainerType, other.ContainerType) &&
                   string.Equals(PowerId, other.PowerId) &&
                   PowerSeqNumber == other.PowerSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PowerLimits)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ContainerType != null ? ContainerType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PowerId != null ? PowerId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PowerSeqNumber.GetHashCode();
                return hashCode;
            }
        }
    }
}
