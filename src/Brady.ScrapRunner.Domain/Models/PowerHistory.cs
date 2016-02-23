using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A PowerHistory record. 
    /// </summary>
    public class PowerHistory : IHaveCompositeId, IEquatable<PowerHistory>
    {
        public virtual string PowerId { get; set; }
        public virtual int PowerSeqNumber { get; set; }
        public virtual string PowerType { get; set; }
        public virtual string PowerDesc { get; set; }
        public virtual string PowerSize { get; set; }
        public virtual int? PowerLength { get; set; }
        public virtual int? PowerTareWeight { get; set; }
        public virtual string PowerCustType { get; set; }
        public virtual string PowerCustTypeDesc { get; set; }
        public virtual string PowerTerminalId { get; set; }
        public virtual string PowerTerminalName { get; set; }
        public virtual string PowerRegionId { get; set; }
        public virtual string PowerRegionName { get; set; }
        public virtual string PowerLocation { get; set; }
        public virtual string PowerStatus { get; set; }
        public virtual DateTime? PowerDateOutOfService { get; set; }
        public virtual DateTime? PowerDateInService { get; set; }
        public virtual string PowerDriverId { get; set; }
        public virtual string PowerDriverName { get; set; }
        public virtual int? PowerOdometer { get; set; }
        public virtual string PowerComments { get; set; }
        public virtual string MdtId { get; set; }
        public virtual string PrimaryPowerType { get; set; }
        public virtual string PowerCustHostCode { get; set; }
        public virtual string PowerCustName { get; set; }
        public virtual string PowerCustAddress1 { get; set; }
        public virtual string PowerCustAddress2 { get; set; }
        public virtual string PowerCustCity { get; set; }
        public virtual string PowerCustState { get; set; }
        public virtual string PowerCustZip { get; set; }
        public virtual string PowerCustCountry { get; set; }
        public virtual string PowerCustCounty { get; set; }
        public virtual string PowerCustTownship { get; set; }
        public virtual string PowerCustPhone1 { get; set; }
        public virtual DateTime? PowerLastActionDateTime { get; set; }
        public virtual string PowerStatusDesc { get; set; }
        public virtual string PowerCurrentTripNumber { get; set; }
        public virtual string PowerCurrentTripSegNumber { get; set; }
        public virtual string PowerCurrentTripSegType { get; set; }
        public virtual string PowerCurrentTripSegTypeDesc { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", PowerId, PowerSeqNumber);
            }
            set
            {
            }
        }

        public virtual bool Equals(PowerHistory other)
        {
            return string.Equals(PowerId, other.PowerId) &&
                    PowerSeqNumber == other.PowerSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PowerHistory)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PowerId != null ? PowerId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PowerSeqNumber.GetHashCode());
                return hashCode;
            }
        }

    }
}
