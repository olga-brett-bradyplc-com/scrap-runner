using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverDelayProcess (request and response).
    ///
    public class DriverDelayProcess : IHaveId<string>, IEquatable<DriverDelayProcess>
    {
        ///The driver id.  Required.
        public virtual string EmployeeId { get; set; }

        ///The power id. Required.
        public virtual string PowerId { get; set; }

        ///The Action Type: D=Delay,B=BackOnDuty. Required.
        public virtual string ActionType { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        /// Trip Number.  Optional.
        public virtual string TripNumber { get; set; }

        /// Trip Segment Number.  Optional.
        public virtual string TripSegNumber { get; set; }

        ///The delay code. Required.
        public virtual string DelayCode { get; set; }

        ///The latitude. Optional.
        public virtual int? Latitude { get; set; }

        ///The longitude. Optional.
        public virtual int? Longitude { get; set; }

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
        public virtual bool Equals(DriverDelayProcess other)
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
            return Equals((DriverDelayProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverDelayProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", TripNumber: " + TripNumber);
            sb.Append(", TripSegNumber:" + TripSegNumber);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", PowerId:" + PowerId);
            sb.Append(", DelayCode:" + DelayCode);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
