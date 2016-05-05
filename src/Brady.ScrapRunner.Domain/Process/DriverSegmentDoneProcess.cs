using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverSegmentDoneProcess (request and response).
    ///
    public class DriverSegmentDoneProcess : IHaveId<string>, IEquatable<DriverSegmentDoneProcess>
    {
        ///The driver.  Required.
        public virtual string EmployeeId { get; set; }

        /// Trip Number.  Required.
        public virtual string TripNumber { get; set; }

        /// Trip Segment Number.  Required.
        public virtual string TripSegNumber { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        ///The power id. Required.
        public virtual string PowerId { get; set; }

        /// Latitude of the segment done.
        public virtual int? Latitude { get; set; }

        /// Longitude of the segment done.
        public virtual int? Longitude { get; set; }

        /// Flag indicating the driver added this segment (RT)
        public virtual string DriverGenerated { get; set; }

        /// Flag indicating the driver changed this segment (RT)
        public virtual string DriverModified { get; set; }

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
        public virtual bool Equals(DriverSegmentDoneProcess other)
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
            return Equals((DriverSegmentDoneProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverSegmentDoneProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", TripNumber: " + TripNumber);
            sb.Append(", TripSegNumber:" + TripSegNumber);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", PowerId:" + PowerId);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
