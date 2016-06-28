using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;
using Brady.ScrapRunner.Domain.Enums;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A GPSLocation record.
    /// </summary>
    public class GPSLocation : IHaveId<int>, IEquatable<GPSLocation>
    {
        // The new key populated by autoincrement column
        public virtual int GPSSeqId { get; set; }
        public virtual string EmployeeId { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual short GPSID { get; set; }
        public virtual DateTimeOffset GPSDateTime { get; set; }
        public virtual int GPSLatitude { get; set; }
        public virtual int GPSLongitude { get; set; }
        public virtual short? GPSSpeed { get; set; }
        public virtual short? GPSHeading { get; set; }
        public virtual GPSSendFlagValue GPSSendFlag { get; set; }

        public virtual int Id
        {
            get
            {
                return GPSSeqId;
            }
            set
            {

            }
        }

        public virtual bool Equals(GPSLocation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GPSSeqId == other.GPSSeqId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GPSLocation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = GPSSeqId.GetHashCode();
                return hashCode;
            }
        }
    }

}
