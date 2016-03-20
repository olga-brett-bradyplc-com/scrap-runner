using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A ContainerQuantity record.
    /// </summary>
    public class ContainerQuantity : IHaveCompositeId, IEquatable<ContainerQuantity>
    {
        public virtual string CustHostCode { get; set; }
        public virtual int CustSeqNo { get; set; }
        public virtual string CustTerminalId { get; set; }
        public virtual string ContainerType { get; set; }
        public virtual string ContainerSize { get; set; }
        public virtual DateTime? LastActionDateTime { get; set; }
        public virtual string LastTripNumber { get; set; }
        public virtual string LastTripSegNumber { get; set; }
        public virtual string LastTripSegType { get; set; }
        public virtual int? LastQuantity { get; set; }
        public virtual int? CurrentQuantity { get; set; }
        public virtual DateTime? ChangedDateTime { get; set; }
        public virtual string ChangedUserId { get; set; }
        public virtual string ChangedUserName { get; set; }
        public virtual string RemoveFromList { get; set; }

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

        public virtual bool Equals(ContainerQuantity other)
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
            return Equals((ContainerQuantity)obj);
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
