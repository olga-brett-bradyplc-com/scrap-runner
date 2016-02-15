using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
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
                return string.Format("{0};{1}", TerminalId, Parameter);
            }
            set
            {

            }
        }

        public virtual bool Equals(Preference other)
        {
            return string.Equals(TerminalId, other.TerminalId)
                   && string.Equals(Parameter, other.Parameter) ;
            //&& string.Equals(ParameterValue, other.ParameterValue) 
            //&& string.Equals(Description, other.Description) 
            //&& string.Equals(Id, other.Id);
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
                var hashCode = (TerminalId != null ? TerminalId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Parameter?.GetHashCode() ?? 0);
                //hashCode = (hashCode * 397) ^ (ParameterValue != null ? ParameterValue.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                //hashCode = (hashCode * 397) ^ (Id?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}