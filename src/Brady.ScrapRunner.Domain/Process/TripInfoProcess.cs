using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    /// <summary>
    /// A TripInfoProcess (request and response).  A pseudo-record.
    /// </summary>
    public class TripInfoProcess : IHaveId<String>, IEquatable<TripInfoProcess>
    {
        /// <summary>
        /// Mandatory input paramter
        /// </summary>
        public virtual string EmployeeId { get; set; }
        public virtual string TripNumber { get; set; }

        /// <summary>
        /// The return value
        /// </summary>
        public virtual List<Trip> Trips { get; set; }

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
        public virtual bool Equals(TripInfoProcess other)
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
            return Equals((TripInfoProcess)obj);
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
            StringBuilder sb = new StringBuilder("TripInfoProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
