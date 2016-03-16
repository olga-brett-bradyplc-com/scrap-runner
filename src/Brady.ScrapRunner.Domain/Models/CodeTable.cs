using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A CodeTable record.
    /// </summary>
    public class CodeTable : IHaveCompositeId, IEquatable<CodeTable>
    {
        public virtual string CodeName { get; set; }
        public virtual string CodeValue { get; set; }
        public virtual int? CodeSeq { get; set; }
        public virtual string CodeDisp1 { get; set; }
        public virtual string CodeDisp2 { get; set; }
        public virtual string CodeDisp3 { get; set; }
        public virtual string CodeDisp4 { get; set; }
        public virtual string CodeDisp5 { get; set; }
        public virtual string CodeDisp6 { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", CodeName, CodeValue);
            }
            set
            {

            }
        }

        public virtual bool Equals(CodeTable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CodeName, other.CodeName) 
                && string.Equals(CodeValue, other.CodeValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CodeTable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode =  (CodeName != null ? CodeName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CodeValue != null ? CodeValue.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
