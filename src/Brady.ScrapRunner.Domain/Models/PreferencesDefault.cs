using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A PreferencesDefault record.
    /// </summary>

    public class PreferencesDefault : IHaveId<string>, IEquatable<PreferencesDefault>
    {
        public virtual string Parameter { get; set; }
        public virtual string ParameterValue { get; set; }
        public virtual string Description { get; set; }
        public virtual string PreferenceType { get; set; }
        public virtual int? PreferenceSeqNo { get; set; }

        public virtual string Id
        {
            get
            {
                return Parameter;
            }
            set
            {

            }
        }


        public virtual bool Equals(PreferencesDefault other)
        {
            return string.Equals(Parameter, other.Parameter);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PreferencesDefault)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Parameter != null ? Parameter.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}