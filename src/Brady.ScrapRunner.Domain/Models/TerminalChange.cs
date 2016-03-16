using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A TerminalChange record.
    /// </summary>
    public class TerminalChange : IHaveCompositeId, IEquatable<TerminalChange>
    {
        public virtual string TerminalId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string CustType { get; set; }
        public virtual string CustHostCode { get; set; }
        public virtual string CustCode4_4 { get; set; }
        public virtual string CustName { get; set; }
        public virtual string CustAddress1 { get; set; }
        public virtual string CustAddress2 { get; set; }
        public virtual string CustCity { get; set; }
        public virtual string CustState { get; set; }
        public virtual string CustZip { get; set; }
        public virtual string CustCountry { get; set; }
        public virtual string CustPhone1 { get; set; }
        public virtual string CustContact1 { get; set; }
        public virtual DateTime? CustOpenTime { get; set; }
        public virtual DateTime? CustCloseTime { get; set; }
        public virtual int? CustLatitude { get; set; }
        public virtual int? CustLongitude { get; set; }
        public virtual int? CustRadius { get; set; }
        public virtual DateTime? ChgDateTime { get; set; }
        public virtual string ChgActionFlag { get; set; }
        public virtual string CustDriverInstructions { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0}", TerminalId);
            }
            set
            {

            }
        }

        public virtual bool Equals(TerminalChange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TerminalId, other.TerminalId) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TerminalChange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TerminalId != null ? TerminalId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
