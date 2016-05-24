using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A FuelEntryProcess (request and response).
    ///
    public class DriverFuelEntryProcess : IHaveId<string>, IEquatable<DriverFuelEntryProcess>
    {
        ///The driver id.  Required.
        public virtual string EmployeeId { get; set; }

        /// Trip Number.  Optional.
        public virtual string TripNumber { get; set; }

        /// Trip Segment Number.  Optional.
        public virtual string TripSegNumber { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        ///The power id. Required.
        public virtual string PowerId { get; set; }

        ///The odometer. Required.
        public virtual int Odometer { get; set; }

        ///The state. Required.
        public virtual string State { get; set; }

        ///The country.  Required.
        public virtual string Country { get; set; }

        ///The amount of fuel. Required.
        public virtual float FuelAmount { get; set; }

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
        public virtual bool Equals(DriverFuelEntryProcess other)
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
            return Equals((DriverFuelEntryProcess)obj);
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
            StringBuilder sb = new StringBuilder("FuelEntryProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", TripNumber: " + TripNumber);
            sb.Append(", TripSegNumber:" + TripSegNumber);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", PowerId:" + PowerId);
            sb.Append(", Odometer:" + Odometer);
            sb.Append(", State:" + State);
            sb.Append(", Amount:" + FuelAmount);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
