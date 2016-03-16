using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A SecurityMaster record.
    /// </summary>
    public class SecurityMaster : IHaveCompositeId, IEquatable<SecurityMaster>
    {
        public virtual string SecurityLevel { get; set; }
        public virtual string SecurityType { get; set; }
        public virtual string SecurityProgram { get; set; }
        public virtual string SecurityFunction { get; set; }
        public virtual string SecurityDescription1 { get; set; }
        public virtual string SecurityDescription2 { get; set; }
        public virtual string SecurityDescription3 { get; set; }
        public virtual string SecurityDescription4 { get; set; }
        public virtual int? SecurityAccess { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2};{3}", SecurityFunction, SecurityLevel, SecurityProgram, SecurityType);
            }
            set
            {

            }
        }

        public virtual bool Equals(SecurityMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(SecurityFunction, other.SecurityFunction) &&
                   string.Equals(SecurityLevel, other.SecurityLevel) &&
                   string.Equals(SecurityProgram, other.SecurityProgram) &&
                   string.Equals(SecurityType, other.SecurityType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SecurityMaster)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (SecurityFunction != null ? SecurityFunction.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SecurityLevel != null ? SecurityLevel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SecurityProgram != null ? SecurityProgram.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SecurityType != null ? SecurityType.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
