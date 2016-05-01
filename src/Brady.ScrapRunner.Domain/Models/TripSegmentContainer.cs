using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A TripSegmentContainer record.
    /// </summary>
    public class TripSegmentContainer : IHaveCompositeId, IEquatable<TripSegmentContainer>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual short TripSegContainerSeqNumber { get; set; }
        public virtual string TripSegContainerNumber { get; set; }
        public virtual string TripSegContainerType { get; set; }
        public virtual string TripSegContainerSize { get; set; }
        public virtual string TripSegContainerCommodityCode { get; set; }
        public virtual string TripSegContainerCommodityDesc { get; set; }
        public virtual string TripSegContainerLocation { get; set; }
        public virtual string TripSegContainerShortTerm { get; set; }
        public virtual int? TripSegContainerWeightGross { get; set; }
        public virtual int? TripSegContainerWeightGross2nd { get; set; }
        public virtual int? TripSegContainerWeightTare { get; set; }
        public virtual string TripSegContainerReviewFlag { get; set; }
        public virtual string TripSegContainerReviewReason { get; set; }
        public virtual DateTime? TripSegContainerActionDateTime { get; set; }
        public virtual string TripSegContainerEntryMethod { get; set; }
        public virtual DateTime? WeightGrossDateTime { get; set; }
        public virtual DateTime? WeightGross2ndDateTime { get; set; }
        public virtual DateTime? WeightTareDateTime { get; set; }
        public virtual short? TripSegContainerLevel { get; set; }
        public virtual int? TripSegContainerLatitude { get; set; }
        public virtual int? TripSegContainerLongitude { get; set; }
        public virtual string TripSegContainerLoaded { get; set; }
        public virtual string TripSegContainerOnTruck { get; set; }
        public virtual string TripScaleReferenceNumber { get; set; }
        public virtual string TripSegContainerSubReason { get; set; }
        public virtual string TripSegContainerComment { get; set; }
        public virtual string TripSegContainerComplete { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", TripNumber, TripSegContainerSeqNumber, TripSegNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripSegmentContainer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripNumber, other.TripNumber) &&
                   TripSegContainerSeqNumber == other.TripSegContainerSeqNumber &&
                    string.Equals(TripSegNumber, other.TripSegNumber); 
      }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripSegmentContainer) obj);
        }
        public override int GetHashCode()
        {
            var hashCode = TripNumber.GetHashCode();
            hashCode = (hashCode * 397) ^ TripSegContainerSeqNumber.GetHashCode();
            hashCode = (hashCode * 397) ^ TripSegNumber.GetHashCode();
            return hashCode;
        }
    }
}
