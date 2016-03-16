using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
   
    /// <summary>
    /// A ControlNumberMaster record.  
    /// </summary>
    public class ControlNumberMaster : IHaveCompositeId, IEquatable<ControlNumberMaster>
    {
        public virtual string ControlType { get; set; }
        public virtual int? ControlNumberNext { get; set; }
 
        public virtual string Id
        {
            get
            {
                return string.Format("{0}", ControlType);
            }
            set
            {

            }
        }

        public virtual bool Equals(ControlNumberMaster other)
        {
            return string.Equals(ControlType, other.ControlType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ControlNumberMaster)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ControlType != null ? ControlType.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
