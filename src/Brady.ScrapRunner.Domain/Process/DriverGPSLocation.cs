using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverGPSLocationProcess (request and response).
    ///
    public class DriverGPSLocationProcess : IHaveId<string>, IEquatable<DriverGPSLocationProcess>
    {
        ///The driver id. Required.
        public virtual string EmployeeId { get; set; }

        ///  GPS ID.  Required.
        public virtual short GPSID { get; set; }

        ///  Date Time of the GPS Capture.  Required.
        public virtual DateTime ActionDateTime { get; set; }

        ///  Latitude.  Required.
        public virtual int Latitude { get; set; }

        ///  Longitude.  Required.
        public virtual int Longitude { get; set; }

        ///  Speed.  Optional.
        public virtual short? Speed { get; set; }

        ///  Heading.  Optional.
        public virtual short? Heading { get; set; }

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
        public virtual bool Equals(DriverGPSLocationProcess other)
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
            return Equals((DriverGPSLocationProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverGPSLocationProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", GPSID:" + GPSID);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", Latitude:" + Latitude);
            sb.Append(", Longitude:" + Longitude);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
