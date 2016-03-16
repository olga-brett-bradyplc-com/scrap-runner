using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// An DriverMaster record.  
    /// </summary>
    public class DriverMaster : IHaveCompositeId, IEquatable<DriverMaster>
    {
        public virtual string EmployeeId { get; set; }
        public virtual DateTime? StartingTime { get; set; }
        public virtual DateTime? QuitTime { get; set; }
        public virtual DateTime? HoursPerDay { get; set; }
        public virtual string DispatcherId { get; set; }
        public virtual string Route { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string Tractor { get; set; }
        public virtual string DLicenseNumber { get; set; }
        public virtual string DLicenseState { get; set; }
        public virtual DateTime? DLicenseExpiry { get; set; }
        public virtual string OwnerOperFlag { get; set; }
        public virtual DateTime? SeniorityDate { get; set; }
        public virtual DateTime? NextPhysDate { get; set; }
        public virtual DateTime? BegVacDate { get; set; }
        public virtual DateTime? EndVacDate { get; set; }
        public virtual string Comments { get; set; }
        public virtual string TrailerQualified { get; set; }
        public virtual string PowerQualified { get; set; }
        public virtual string ContractDriverFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return EmployeeId;
            }
            set
            {

            }
        }

        public virtual bool Equals(DriverMaster other)
        {
            return string.Equals(EmployeeId, other.EmployeeId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverMaster)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
    
