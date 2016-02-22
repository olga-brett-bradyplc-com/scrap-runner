using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
 
    /// <summary>
    /// An EmployeeArea record.
    /// </summary>
    public class EmployeeArea : IHaveCompositeId, IEquatable<EmployeeArea>
    {
        public virtual string EmployeeId { get; set; }
        public virtual string AreaId { get; set; }
        public virtual int? ButtonNumber { get; set; }
        public virtual string DefaultTerminalId { get; set; }
        public virtual int? KeyCode { get; set; }
        public virtual string KeyDescription { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", AreaId, EmployeeId);
            }
            set
            {

            }
        }

        public virtual bool Equals(EmployeeArea other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(AreaId, other.AreaId)
                && string.Equals(EmployeeId, other.EmployeeId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmployeeArea)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AreaId != null ? AreaId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}
