using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A TerminalMaster record.  FIXME:  Remove the composite Id workaround for NHibernate and/or metadata 
    /// </summary>
    public class TerminalMaster : IHaveCompositeId, IEquatable<TerminalMaster>
    {

        public virtual string TerminalId { get; set; }
        public virtual string Region { get; set; }
        public virtual string TerminalName { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }
        public virtual string Country { get; set; }
        public virtual string Phone { get; set; }
        public virtual int? EtakLatitude { get; set; }
        public virtual int? EtakLongitude { get; set; }
        public virtual int? Latitude { get; set; }
        public virtual int? Longitude { get; set; }
        public virtual string DispatchZone { get; set; }
        public virtual string Geocoded { get; set; }
        public virtual int? RegionIndex { get; set; }
        public virtual int? TerminalIdNumber { get; set; }
        public virtual string MasterTerminal { get; set; }
        public virtual DateTime? ChgDateTime { get; set; }
        public virtual string ChgEmployeeId { get; set; }
        public virtual string TerminalType { get; set; }
        public virtual int? TimeZoneFactor { get; set; }
        public virtual string DaylightSavings { get; set; }
        public virtual string TerminalIdHost { get; set; }
        public virtual string FileNameHost { get; set; }

        public virtual string Id
        {
            get
            {
                return TerminalId;
            }
            set
            {

            }
        }

        public virtual bool Equals(TerminalMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TerminalId, other.TerminalId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TerminalMaster) obj);
        }

        public override int GetHashCode()
        {
            return (TerminalId != null ? TerminalId.GetHashCode() : 0);
        }
    }

}
