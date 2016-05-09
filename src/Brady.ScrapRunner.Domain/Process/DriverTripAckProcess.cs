using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverTripsAckProcess (request and response).
    ///
    public class DriverTripAckProcess : IHaveId<string>, IEquatable<DriverTripAckProcess>
    {
        ///The driver id.  Required.
        public virtual string EmployeeId { get; set; }

        /// Trip Number.  Required.
        public virtual string TripNumber { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

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
        public virtual bool Equals(DriverTripAckProcess other)
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
            return Equals((DriverTripAckProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverEnrouteProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", TripNumber: " + TripNumber);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
