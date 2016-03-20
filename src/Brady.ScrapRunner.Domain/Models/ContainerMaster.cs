using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A ContainerMaster record.
    /// </summary>
    public class ContainerMaster : IHaveCompositeId, IEquatable<ContainerMaster>
    {
        public virtual string ContainerNumber { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual string ContainerUnits { get; set; }
        public virtual int? ContainerLength { get; set; }
        public virtual int? ContainerWeightTare { get; set; }
        public virtual string ContainerShortTerm { get; set; }
        public virtual string ContainerCustHostCode { get; set; }
        public virtual string ContainerCustType { get; set; }
        public virtual string ContainerTerminalId { get; set; }
        public virtual string ContainerRegionId { get; set; }
        public virtual string ContainerLocation { get; set; }
        public virtual DateTime? ContainerLastActionDateTime { get; set; }
        public virtual DateTime? ContainerPendingMoveDateTime { get; set; }
        public virtual string ContainerCurrentTripNumber { get; set; }
        public virtual string ContainerCurrentTripSegNumber { get; set; }
        public virtual string ContainerCurrentTripSegType { get; set; }
        public virtual string ContainerContents { get; set; }
        public virtual string ContainerStatus { get; set; }
        public virtual string ContainerCommodityCode { get; set; }
        public virtual string ContainerCommodityDesc { get; set; }
        public virtual string ContainerComments { get; set; }
        public virtual string ContainerPowerId { get; set; }
        public virtual string ContainerBarCodeFlag { get; set; }
        public virtual DateTime? ContainerAddDateTime { get; set; }
        public virtual string ContainerAddUserId { get; set; }
        public virtual string ContainerRestrictToHostCode { get; set; }
        public virtual string ContainerPrevCustHostCode { get; set; }
        public virtual string ContainerPrevTripNumber { get; set; }
        public virtual int? ContainerLevel { get; set; }
        public virtual int? ContainerLatitude { get; set; }
        public virtual int? ContainerLongitude { get; set; }
        public virtual string ContainerNotes { get; set; }
        public virtual string ContainerCurrentTerminalId { get; set; }
        public virtual string ContainerManufacturer { get; set; }
        public virtual float? ContainerCost { get; set; }
        public virtual string ContainerSerialNumber { get; set; }
        public virtual string ContainerOrigin { get; set; }
        public virtual DateTime? ContainerPurchaseDate { get; set; }
        public virtual string LocationWarningFlag { get; set; }
        public virtual int? ContainerWidth { get; set; }
        public virtual int? ContainerHeight { get; set; }
        public virtual string ContainerBarCodeNo { get; set; }
        public virtual string ContainerInboundTerminalId { get; set; }
        public virtual string ContainerQtyInIDFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0}", ContainerNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(ContainerMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ContainerNumber, other.ContainerNumber) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerMaster) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ContainerNumber != null ? ContainerNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
