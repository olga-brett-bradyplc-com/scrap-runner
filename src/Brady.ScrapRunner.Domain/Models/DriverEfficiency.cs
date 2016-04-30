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
        public virtual int? TripActualDriveMinutes { get; set; }
        public virtual int? TripStandardDriveMinutes { get; set; }
        public virtual int? TripActualStopMinutes { get; set; }
        public virtual int? TripStandardStopMinutes { get; set; }
        public virtual int? TripActualYardMinutes { get; set; }
        public virtual int? TripStandardYardMinutes { get; set; }
        public virtual int? TripActualTotalMinutes { get; set; }
        public virtual int? TripStandardTotalMinutes { get; set; }
        public virtual int? TripDelayMinutes { get; set; }
        public virtual string TripPowerId { get; set; }
        public virtual int? TripOdometerStart { get; set; }
        public virtual int? TripOdometerEnd { get; set; }
        public virtual int? TripYardDelayMinutes { get; set; }
        public virtual int? TripCustDelayMinutes { get; set; }
        public virtual int? TripLunchBreakDelayMinutes { get; set; }

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
            return Equals((DriverEfficiency)obj);
        }
        public override int GetHashCode()
        {
            var hashCode = TripDriverId.GetHashCode();
            hashCode = (hashCode * 397) ^ TripNumber.GetHashCode();
            return hashCode;
        }
    }
}
