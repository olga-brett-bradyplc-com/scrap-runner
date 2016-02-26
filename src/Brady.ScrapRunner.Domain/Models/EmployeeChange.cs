using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// An EmployeeChange record.  
    /// </summary>
    public class EmployeeChange : IHaveCompositeId, IEquatable<EmployeeChange>
    {
        public virtual string EmployeeId { get; set; }
        public virtual string LoginId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string ActionFlag { get; set; }
        public virtual string Password { get; set; }
        public virtual DateTime ChangeDateTime { get; set; }
  
        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2};{3};{4}",
                    ActionFlag, EmployeeId, LoginId, Password, RegionId);
            }
            set
            {

            }
        }

        public virtual bool Equals(EmployeeChange other)
        {
            return string.Equals(ActionFlag, other.ActionFlag) &&
                   string.Equals(EmployeeId, other.EmployeeId) &&
                   string.Equals(LoginId, other.LoginId) &&
                   string.Equals(Password, other.Password) &&
                   string.Equals(RegionId, other.RegionId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmployeeChange)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ActionFlag != null ? ActionFlag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LoginId != null ? LoginId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RegionId != null ? RegionId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
