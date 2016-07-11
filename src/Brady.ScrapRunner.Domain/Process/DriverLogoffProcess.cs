using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverLogOffProcess (request and response).
    ///
    public class DriverLogoffProcess : IHaveId<string>, IEquatable<DriverLogoffProcess>
    {
        ///The driver id from the phone. Required.
        public virtual string EmployeeId { get; set; }

        ///The power id from the phone. Required.
        public virtual string PowerId { get; set; }

        ///The odometer from the phone. Required.
        public virtual int? Odometer { get; set; }

        /// The LogoffDateTime from the phone.  Required.
        public virtual DateTime ActionDateTime { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string TripNumber { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string TripSegNumber { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string DriverStatus { get; set; }


         public virtual String Id
        {
            get
            {
                return EmployeeId;
            }
            set
            {
                // no-op
            }
        }

        public virtual bool Equals(DriverLogoffProcess other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EmployeeId == other.EmployeeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverLogoffProcess)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Relevant input values, useful for logging
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("DriverLogoffProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", PowerId:" + PowerId);
            sb.Append(", Odometer:" + Odometer);
            sb.Append(", DateTime:" + ActionDateTime);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
