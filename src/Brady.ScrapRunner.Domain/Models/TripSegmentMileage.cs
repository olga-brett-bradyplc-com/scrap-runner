using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A TripSegmentMileage record.
    /// </summary>
    public class TripSegmentMileage : IHaveCompositeId, IEquatable<TripSegmentMileage>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual int TripSegMileageSeqNumber { get; set; }
        public virtual string TripSegMileageState { get; set; }
        public virtual string TripSegMileageCountry { get; set; }
        public virtual int? TripSegMileageOdometerStart { get; set; }
        public virtual int? TripSegMileageOdometerEnd { get; set; }
        public virtual string TripSegLoadedFlag { get; set; }
        public virtual string TripSegMileagePowerId { get; set; }
        public virtual string TripSegMileageDriverId { get; set; }
        public virtual string TripSegMileageDriverName { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", TripNumber, TripSegMileageSeqNumber, TripSegNumber );
            }
            set
            {

            }
        }

        public virtual bool Equals(TripSegmentMileage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripNumber, other.TripNumber) 
                && TripSegMileageSeqNumber == other.TripSegMileageSeqNumber
                && string.Equals(TripSegNumber, other.TripSegNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentMileage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripNumber != null ? TripNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TripSegMileageSeqNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (TripSegNumber != null ? TripSegNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
