using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    //
    // A DriverLoginProcess (request and response).  A pseudo-record.
    //
    // Q1) Are these named special to indicate In/Out or Req/Resp nature of pseudo-record
    // Q2) Is Mdtid needed for phones?  Or null Or "Phone"?
    // Q3) Is PndVer needed for phones?  Or null Or "Phone"?
    // Q4) What is overrideflag and how is it used or updated during login?
    public class DriverLoginProcess : IHaveId<String>, IEquatable<DriverLoginProcess>
    {

        public virtual string EmployeeId { get; set; }
        public virtual string Password { get; set; }
        public virtual string PowerId { get; set; }
        public virtual int Odometer { get; set; }
        // Here for now, possibly break out as separate call
        public virtual string CodeListVersion { get; set; }
        // Here for now, probably break out as separate call
        public virtual DateTime? LastContainerMasterUpdate { get; set; }
        // Here for now, possibly break out as separate call
        public virtual DateTime? LastTerminalMasterUpdate { get; set; }
        // Eg: 1033 = English, USA
        //     2058 = Spanish, Mexico
        // Is this a device preference or could it be a user preference (membership?)
        public virtual string LocaleCode { get; set; }
        // How is this used?
        public virtual string OverrideFlag { get; set; }

        // Initially null Or "Phone"?
        public virtual string Mdtid { get; set; }
        // Initially null
        public virtual string TermId { get; set; }
        // Initially null
        public virtual string RegionId { get; set; }
        // Initially null
        public virtual string AreaId { get; set; }
        // Initially null Or "Phone"?
        public virtual string PndVer { get; set; }
        // Initially null
        public virtual string TripNumber { get; set; }
        // Initially null
        public virtual string TripSegNumber { get; set; }
        // Initially null
        public virtual string DriverStatus { get; set; }
        // LoginDateTime form hand held.  Initially null?  Why shoud we trust the client?
        public virtual DateTime? LoginDateTime { get; set; }

        //Either in this Message or a separate call
        //public virtual List<ContainerMasterLite> ContainerMasters { get; set; }

        //Either in this Message or a separate call
        //public virtual List<TermainalMasterLite> TerminalMasters { get; set; }

        //Either in this Message or a separate call
        //public virtual List<CommodityMasterLite> UniversalCommodities { get; set; }

        //For now in this Message
        public virtual List<Preference> Preferences { get; set; }

        //For now in this Message
        public virtual List<CodeTable> CodeTables { get; set; }

        // 19) Send Trips (common processing)
        // a.Send the Commodities and Locations for each unique customer host code.
        // b.Send Trip Reference Numbers for each trip.
        // c.Send Trip Information for each trip
        // d.Send Driver Instructions
        // e.	Send TripSegment information for each trip
        // f.Send TripSegmentContainer information for each trip segment
        // g.	Send a Trip End record
        // h.	Update the Trip Table
        // i.Add entry to Event Log – Trip Sent.

        // 20)	Send Container Inventory(Containers on Power Unit)

        // 21)	Send Dispatcher List for Messaging

        public virtual String Id
        {
            get
            {
                return EmployeeId;
            }
            set
            {
                // no-op
            }
        }

        public virtual bool Equals(DriverLoginProcess other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EmployeeId == other.EmployeeId;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverLoginProcess) obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}
