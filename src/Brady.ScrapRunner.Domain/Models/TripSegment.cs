using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Brady.ScrapRunner.Domain.Models
{
    public class TripSegment : IHaveCompositeId, IEquatable<TripSegment>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual string TripSegStatus { get; set; }
        public virtual string TripSegStatusDesc { get; set; }
        public virtual string TripSegType { get; set; }
        public virtual string TripSegTypeDesc { get; set; }
        public virtual string TripSegPowerId { get; set; }
        public virtual string TripSegDriverId { get; set; }
        public virtual string TripSegDriverName { get; set; }
        public virtual DateTime? TripSegStartDateTime { get; set; }
        public virtual DateTime? TripSegEndDateTime { get; set; }
        public virtual int? TripSegStandardDriveMinutes { get; set; }
        public virtual int? TripSegStandardStopMinutes { get; set; }
        public virtual int? TripSegActualDriveMinutes { get; set; }
        public virtual int? TripSegActualStopMinutes { get; set; }
        public virtual int? TripSegOdometerStart { get; set; }
        public virtual int? TripSegOdometerEnd { get; set; }
        public virtual string TripSegComments { get; set; }
        public virtual string TripSegOrigCustType { get; set; }
        public virtual string TripSegOrigCustTypeDesc { get; set; }
        public virtual string TripSegOrigCustHostCode { get; set; }
        public virtual string TripSegOrigCustCode4_4 { get; set; }
        public virtual string TripSegOrigCustName { get; set; }
        public virtual string TripSegOrigCustAddress1 { get; set; }
        public virtual string TripSegOrigCustAddress2 { get; set; }
        public virtual string TripSegOrigCustCity { get; set; }
        public virtual string TripSegOrigCustState { get; set; }
        public virtual string TripSegOrigCustZip { get; set; }
        public virtual string TripSegOrigCustCountry { get; set; }
        public virtual string TripSegOrigCustPhone1 { get; set; }
        public virtual int? TripSegOrigCustTimeFactor { get; set; }
        public virtual string TripSegDestCustType { get; set; }
        public virtual string TripSegDestCustTypeDesc { get; set; }
        public virtual string TripSegDestCustHostCode { get; set; }
        public virtual string TripSegDestCustCode4_4 { get; set; }
        public virtual string TripSegDestCustName { get; set; }
        public virtual string TripSegDestCustAddress1 { get; set; }
        public virtual string TripSegDestCustAddress2 { get; set; }
        public virtual string TripSegDestCustCity { get; set; }
        public virtual string TripSegDestCustState { get; set; }
        public virtual string TripSegDestCustZip { get; set; }
        public virtual string TripSegDestCustCountry { get; set; }
        public virtual string TripSegDestCustPhone1 { get; set; }
        public virtual int? TripSegDestCustTimeFactor { get; set; }
        public virtual string TripSegPrimaryContainerNumber { get; set; }
        public virtual string TripSegPrimaryContainerType { get; set; }
        public virtual string TripSegPrimaryContainerSize { get; set; }
        public virtual string TripSegPrimaryContainerCommodityCode { get; set; }
        public virtual string TripSegPrimaryContainerCommodityDesc { get; set; }
        public virtual string TripSegPrimaryContainerLocation { get; set; }
        public virtual DateTime? TripSegActualDriveStartDateTime { get; set; }
        public virtual DateTime? TripSegActualDriveEndDateTime { get; set; }
        public virtual DateTime? TripSegActualStopStartDateTime { get; set; }
        public virtual DateTime? TripSegActualStopEndDateTime { get; set; }
        public virtual int? TripSegStartLatitude { get; set; }
        public virtual int? TripSegStartLongitude { get; set; }
        public virtual int? TripSegEndLatitude { get; set; }
        public virtual int? TripSegEndLongitude { get; set; }
        public virtual int? TripSegStandardMiles { get; set; }
        public virtual string TripSegErrorDesc { get; set; }
        public virtual int? TripSegContainerQty { get; set; }
        public virtual string TripSegDriverGenerated { get; set; }
        public virtual string TripSegDriverModified { get; set; }
        public virtual string TripSegPowerAssetNumber { get; set; }
        public virtual string TripSegExtendedFlag { get; set; }
        public virtual int? TripSegSendReceiptFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", TripNumber, TripSegNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripSegment other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripNumber, other.TripNumber) && string.Equals(TripSegNumber, other.TripSegNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripNumber != null ? TripNumber.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (TripSegNumber != null ? TripSegNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
