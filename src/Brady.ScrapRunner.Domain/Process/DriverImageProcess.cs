using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverImageProcess (request and response).
    ///
    public class DriverImageProcess : IHaveId<string>, IEquatable<DriverImageProcess>
    {
        ///The driver id. Required.
        public virtual string EmployeeId { get; set; }

        /// Trip Number.  Required.
        public virtual string TripNumber { get; set; }

        /// Trip Segment Number.  Required.
        public virtual string TripSegNumber { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        ///The Printed Name. Required for S=Signature ImageTypes.
        public virtual string PrintedName { get; set; }

        ///The ImageType. P=Picture, S=Signature. Required.
        public virtual string ImageType { get; set; }

        ///The Image. Required.
        public virtual byte[] Image { get; set; }

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
        public virtual bool Equals(DriverImageProcess other)
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
            return Equals((DriverImageProcess)obj);
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
            sb.Append(", TripSegNumber:" + TripSegNumber);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", PrintedName:" + PrintedName);
            sb.Append(", ImageType:" + ImageType);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
