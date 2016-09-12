using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{

    /// <summary>
    /// An EmployeePreferences record.
    /// </summary>
    public class EmployeePreferences : IHaveCompositeId, IEquatable<EmployeePreferences>
    {
        public virtual string RegionId { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string EmployeeId { get; set; }
        public virtual string Parameter { get; set; }
        public virtual string ParameterValue { get; set; }
        public virtual string Description { get; set; }
        public virtual string PreferenceType { get; set; }
        public virtual int? PreferenceSeqNo { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2};{3}", 
                    RegionId, TerminalId, EmployeeId, Parameter);
            }
            set
            {

            }
        }

        public virtual bool Equals(EmployeePreferences other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(RegionId, other.RegionId)
                && string.Equals(TerminalId, other.TerminalId)
                && string.Equals(EmployeeId, other.EmployeeId)
                && string.Equals(Parameter, other.Parameter);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmployeePreferences)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RegionId != null ? RegionId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TerminalId != null ? TerminalId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Parameter != null ? Parameter.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}
