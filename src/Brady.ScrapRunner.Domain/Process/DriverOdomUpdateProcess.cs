using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverOdomUpdateProcess (request and response).
    ///
    public class DriverOdomUpdateProcess : IHaveId<string>, IEquatable<DriverOdomUpdateProcess>
    {
        ///The driver id. Required.
        public virtual string EmployeeId { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        ///The power id. Required.
        public virtual string PowerId { get; set; }

        ///The odometer. Required.
        public virtual int Odometer { get; set; }

        /// Currently this is the driver id. Probably will not be needed in the future.
        public virtual string Mdtid { get; set; }

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
        public virtual bool Equals(DriverOdomUpdateProcess other)
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
            return Equals((DriverOdomUpdateProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverOdomUpdateProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", PowerId:" + PowerId);
            sb.Append(", Odometer:" + Odometer);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
