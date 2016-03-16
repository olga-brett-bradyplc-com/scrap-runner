using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A ContainerChange record.
    /// </summary>
    public class ContainerChange : IHaveCompositeId, IEquatable<ContainerChange>
    {
        public virtual string ContainerNumber { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual DateTime? ActionDate { get; set; }
        public virtual string ActionFlag { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string ContainerBarCodeNo { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0}", ContainerNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(ContainerChange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ContainerNumber, other.ContainerNumber) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerChange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ContainerNumber != null ? ContainerNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
