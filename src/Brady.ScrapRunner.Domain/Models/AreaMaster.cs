using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// An AreaMaster record.
    /// </summary>
    public class AreaMaster : IHaveCompositeId, IEquatable<AreaMaster>
    {
        public virtual string AreaId { get; set; }
        public virtual string TerminalId { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", AreaId, TerminalId);
            }
            set
            {

            }
        }
        public virtual bool Equals(AreaMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(AreaId, other.AreaId)
                && string.Equals(TerminalId,other.TerminalId);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AreaMaster)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AreaId != null ? AreaId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
