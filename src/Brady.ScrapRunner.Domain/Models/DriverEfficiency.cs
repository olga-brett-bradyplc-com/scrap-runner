using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
      /// <summary>
    /// A DriverEfficiency record.
    /// </summary>
    public class DriverEfficiency : IHaveCompositeId, IEquatable<DriverEfficiency>
    {
        public virtual string TripDriverId { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual string TripDriverName { get; set; }
        public virtual string TripType { get; set; }
        public virtual string TripTypeDesc { get; set; }
        public virtual string TripCustHostCode { get; set; }
        public virtual string TripCustName { get; set; }
        public virtual string TripCustAddress1 { get; set; }
        public virtual string TripCustAddress2 { get; set; }
        public virtual string TripCustCity { get; set; }
        public virtual string TripCustState { get; set; }
        public virtual string TripCustZip { get; set; }
        public virtual string TripCustCountry { get; set; }
        public virtual string TripTerminalId { get; set; }
        public virtual string TripTerminalName { get; set; }
        public virtual string TripRegionId { get; set; }
        public virtual string TripRegionName { get; set; }
        public virtual string TripReferenceNumber { get; set; }
        public virtual DateTime? TripCompletedDateTime { get; set; }
        public virtual long? TripActualDriveMinutes { get; set; }
        public virtual long? TripStandardDriveMinutes { get; set; }
        public virtual long? TripActualStopMinutes { get; set; }
        public virtual long? TripStandardStopMinutes { get; set; }
        public virtual long? TripActualYardMinutes { get; set; }
        public virtual long? TripStandardYardMinutes { get; set; }
        public virtual long? TripActualTotalMinutes { get; set; }
        public virtual long? TripStandardTotalMinutes { get; set; }
        public virtual long? TripDelayMinutes { get; set; }
        public virtual string TripPowerId { get; set; }
        public virtual long? TripOdometerStart { get; set; }
        public virtual long? TripOdometerEnd { get; set; }
        public virtual long? TripYardDelayMinutes { get; set; }
        public virtual long? TripCustDelayMinutes { get; set; }
        public virtual long? TripLunchBreakDelayMinutes { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", TripDriverId, TripNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(DriverEfficiency other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripDriverId, other.TripDriverId) &&
                    string.Equals(TripNumber, other.TripNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentContainer)obj);
        }
        public override int GetHashCode()
        {
            var hashCode = TripDriverId.GetHashCode();
            hashCode = (hashCode * 397) ^ TripNumber.GetHashCode();
            return hashCode;
        }
    }
}
