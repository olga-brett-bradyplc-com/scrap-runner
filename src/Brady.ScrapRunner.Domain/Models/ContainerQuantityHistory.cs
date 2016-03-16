using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A ContainerQuantityHistory record.
    /// </summary>
    public class ContainerQuantityHistory : IHaveCompositeId, IEquatable<ContainerQuantityHistory>
    {
        public virtual string CustHostCode { get; set; }
        public virtual int CustSeqNo { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerTypeDesc { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual DateTime? LastActionDateTime { get; set; }
        public virtual string LastTripNumber { get; set; }
        public virtual string LastTripSegNumber { get; set; }
        public virtual string LastTripSegType { get; set; }
        public virtual string LastTripSegTypeDesc { get; set; }
        public virtual int? LastQuantity { get; set; }
        public virtual int? CurrentQuantity { get; set; }
        public virtual DateTime? ChangedDateTime { get; set; }
        public virtual string ChangedUserId { get; set; }
        public virtual string ChangedUserName { get; set; }
        public virtual string CustTerminalId { get; set; }
        public virtual string CustTerminalName { get; set; }
        public virtual string CustRegionId { get; set; }
        public virtual string CustRegionName { get; set; }
        public virtual string CustType { get; set; }
        public virtual string CustTypeDesc { get; set; }
        public virtual string CustName { get; set; }
        public virtual string CustAddress1 { get; set; }
        public virtual string CustAddress2 { get; set; }
        public virtual string CustCity { get; set; }
        public virtual string CustState { get; set; }
        public virtual string CustZip { get; set; }
        public virtual string CustCountry { get; set; }
        public virtual string CustCounty { get; set; }
        public virtual string CustTownship { get; set; }
        public virtual string CustPhone1 { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", CustHostCode, CustSeqNo);
            }
            set
            {
            }
        }

        public virtual bool Equals(ContainerQuantityHistory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CustHostCode, other.CustHostCode) &&
                   CustSeqNo == other.CustSeqNo;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerQuantityHistory)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CustHostCode != null ? CustHostCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ CustSeqNo.GetHashCode();
                return hashCode;
            }
        }
    }
}
