using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A ContainerHistory record.
    /// </summary>
    public class ContainerHistory : IHaveCompositeId, IEquatable<ContainerHistory>
    {
        public virtual string ContainerNumber { get; set; }
        public virtual int ContainerSeqNumber { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual string ContainerUnits { get; set; }
        public virtual int? ContainerLength { get; set; }
        public virtual string ContainerCustHostCode { get; set; }
        public virtual string ContainerCustType { get; set; }
        public virtual string ContainerCustTypeDesc { get; set; }
        public virtual string ContainerTerminalId { get; set; }
        public virtual string ContainerTerminalName { get; set; }
        public virtual string ContainerRegionId { get; set; }
        public virtual string ContainerRegionName { get; set; }
        public virtual string ContainerLocation { get; set; }
        public virtual DateTime? ContainerLastActionDateTime { get; set; }
        public virtual int? ContainerDaysAtSite { get; set; }
        public virtual string ContainerTripNumber { get; set; }
        public virtual string ContainerTripSegNumber { get; set; }
        public virtual string ContainerTripSegType { get; set; }
        public virtual string ContainerTripSegTypeDesc{ get; set; }
        public virtual string ContainerContents { get; set; }
        public virtual string ContainerContentsDesc { get; set; }
        public virtual string ContainerStatus { get; set; }
        public virtual string ContainerStatusDesc { get; set; }
        public virtual string ContainerCommodityCode { get; set; }
        public virtual string ContainerCommodityDesc { get; set; }
        public virtual string ContainerComments { get; set; }
        public virtual string ContainerPowerId { get; set; }
        public virtual string ContainerShortTerm { get; set; }
        public virtual string ContainerCustName { get; set; }
        public virtual string ContainerCustAddress1 { get; set; }
        public virtual string ContainerCustAddress2 { get; set; }
        public virtual string ContainerCustCity { get; set; }
        public virtual string ContainerCustState { get; set; }
        public virtual string ContainerCustZip { get; set; }
        public virtual string ContainerCustCountry { get; set; }
        public virtual string ContainerCustCounty { get; set; }
        public virtual string ContainerCustTownship { get; set; }
        public virtual string ContainerCustPhone1 { get; set; }
        public virtual int? ContainerLevel { get; set; }
        public virtual int? ContainerLatitude { get; set; }
        public virtual int? ContainerLongitude { get; set; }
        public virtual string ContainerNotes { get; set; }
        public virtual string ContainerCurrentTerminalId { get; set; }
        public virtual string ContainerCurrentTerminalName { get; set; }
        public virtual int? ContainerWidth { get; set; }
        public virtual int? ContainerHeight { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", ContainerNumber, ContainerSeqNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(ContainerHistory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ContainerNumber, other.ContainerNumber) &&
                   ContainerSeqNumber == other.ContainerSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerHistory)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ContainerNumber != null ? ContainerNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContainerSeqNumber.GetHashCode());
                return hashCode;
            }
        }
    }
}
