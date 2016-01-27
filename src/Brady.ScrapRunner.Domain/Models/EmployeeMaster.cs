using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// An EmployeeMaster record.  FIXME:  Remove the composite Id workaround for NHibernate and/or metadata 
    /// </summary>
    public class EmployeeMaster : IHaveCompositeId, IEquatable<EmployeeMaster>
    {
        public virtual string EmployeeId { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Address { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }
        public virtual string Phone1 { get; set; }
        public virtual string Phone2 { get; set; }
        public virtual string Password { get; set; }
        public virtual string FileMaintAccess { get; set; }
        public virtual string SecurityLevel { get; set; }
        public virtual string SupervisorId { get; set; }
        public virtual string NickName { get; set; }
        public virtual string EmployeeType { get; set; }
        public virtual string CompanyCode { get; set; }
        public virtual string EmployeeStatus { get; set; }
        public virtual DateTime? LoginDateTime { get; set; }
        public virtual DateTime? AccessDateTime { get; set; }
        public virtual string WorkArea { get; set; }
        public virtual string BillerInitials { get; set; }
        public virtual short? NumTimesLogin { get; set; }
        public virtual short? MaxLogins { get; set; }
        public virtual string RouterId { get; set; }
        public virtual string AreaId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string DefTerminalId { get; set; }
        public virtual string PrevEmployeeId { get; set; }
        public virtual string AllowMessaging { get; set; }
        public virtual string DefLabelPrinter { get; set; }
        public virtual string Country { get; set; }
        public virtual string AllowMapsAccess { get; set; }
        public virtual string DefReadyDateTomorrow { get; set; }
        public virtual string AudibleAlertNewMsg { get; set; }
        public virtual string MapId { get; set; }
        public virtual string ActionFlag { get; set; }
        public virtual string DefReadyDateMonday { get; set; }
        public virtual string DefStartAcctSearchBegName { get; set; }
        public virtual string AllowChangeContNumber { get; set; }
        public virtual string AllowModDoneTrips { get; set; }
        public virtual string AllowCancelDoneTrips { get; set; }
        public virtual string opt { get; set; }
        public virtual int? SessionID { get; set; }
        public virtual string Router { get; set; }
        public virtual string DisplayReceiptNumber { get; set; }
        public virtual string DisplayScaleReferenceNumber { get; set; }
        public virtual string LoginID { get; set; }
        public virtual string LoginIDPrev { get; set; }
        public virtual DateTime? InactiveDate { get; set; }

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

        public virtual bool Equals(EmployeeMaster other)
        {
            return string.Equals(EmployeeId, other.EmployeeId) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmployeeMaster)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ (Id?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

    }
}
