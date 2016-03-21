using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A full CodeTableHdr record.
    /// </summary>
    public class CodeTableHdr : IHaveId<string>, IEquatable<CodeTableHdr>
    {
        public virtual string CodeName { get; set; }
        public virtual string CodeDesc { get; set; }
        public virtual string CodeType { get; set; }
        public virtual string AppliesTo { get; set; }

        public virtual string Id
        {
            get
            {
                return CodeName;
            }
            set
            {
                // No-op 
            }
        }

        public virtual bool Equals(CodeTableHdr other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CodeName, other.CodeName) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CodeTableHdr) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CodeName != null ? CodeName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
