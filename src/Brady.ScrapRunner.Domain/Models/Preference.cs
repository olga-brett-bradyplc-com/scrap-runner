using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A Preference record.
    /// </summary>

    public class Preference : IHaveCompositeId, IEquatable<Preference>
    {
        public virtual string TerminalId { get; set; }
        public virtual string Parameter { get; set; }
        public virtual string ParameterValue { get; set; }
        public virtual string Description { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", Parameter, TerminalId);
            }
            set
            {

            }
        }

        public virtual bool Equals(Preference other)
        {
            return string.Equals(Parameter, other.Parameter)
                   && string.Equals(TerminalId, other.TerminalId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Preference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Parameter != null ? Parameter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TerminalId != null ? TerminalId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}