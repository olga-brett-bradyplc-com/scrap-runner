using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    public class PowerFuel : IHaveCompositeId, IEquatable<PowerFuel>
    {
        public virtual string PowerId { get; set; }
        public virtual int PowerFuelSeqNumber { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual string TripTerminalId { get; set; }
        public virtual string TripRegionId { get; set; }
        public virtual string TripDriverId { get; set; }
        public virtual string TripDriverName { get; set; }
        public virtual DateTime? PowerDateOfFuel { get; set; }
        public virtual string PowerState { get; set; }
        public virtual string PowerCountry { get; set; }
        public virtual int? PowerOdometer { get; set; }
        public virtual float? PowerGallons { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", PowerFuelSeqNumber, PowerId, TripNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(PowerFuel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(PowerFuelSeqNumber, other.PowerFuelSeqNumber)
                && PowerId == other.PowerId
                && string.Equals(TripNumber, other.TripNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentMileage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PowerId != null ? PowerId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PowerFuelSeqNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripNumber != null ? TripNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
