using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverLoginProcess (request and response).
    ///
    public class DriverLoginProcess : IHaveId<string>, IEquatable<DriverLoginProcess>
    {
        public virtual string EmployeeId { get; set; }

        public virtual string Password { get; set; }

        public virtual string PowerId { get; set; }

        public virtual int Odometer { get; set; }

        /// TODO: Q: Is this really a device preference or could/should it be a user preference perhaps via membership?
        /// Eg: 1033 = English, USA
        ///     2058 = Spanish, Mexico
        public virtual int? LocaleCode { get; set; }

        /// <summary>Set this to Y if driver has double checked Odometer and is resubmitting</summary>
        public virtual string OverrideFlag { get; set; }

        /// TODO: Q: Is Mdtid needed for phones?  Initially null Or "Phone"?
        public virtual string Mdtid { get; set; }

        /// Initially null
        public virtual string TermId { get; set; }

        /// Initially null
        public virtual string RegionId { get; set; }
        
        /// Initially null
        public virtual string AreaId { get; set; }

        /// <summary>PndVer is a client version number for the handheld.  Typically used for logging issues.</summary>
        /// TODO: We'll need something similar for phones
        public virtual string PndVer { get; set; }

        /// Initially null
        public virtual string TripNumber { get; set; }

        /// Initially null
        public virtual string TripSegNumber { get; set; }

        /// Initially null
        public virtual string DriverStatus { get; set; }
        
        /// The LoginDateTime from the phone.  (Q: Why shoud we trust the client or whe do we backfill on server side?)
        public virtual DateTime? LoginDateTime { get; set; }

        /// The ContainerMasterDateTime from the phone.  
        public virtual DateTime? ContainerMasterDateTime { get; set; }

        /// The ContainerMasterDateTime from the phone.  
        public virtual DateTime? TerminalMasterDateTime { get; set; }

        //Login Step 14) Send container master updates.  Not implemented see ContainerMasterProcess.
        //public virtual DateTime? LastContainerMasterUpdate { get; set; }
        //public virtual List<ContainerMasterLite> ContainerMasters { get; set; }

        //Login Step 15) Send terminal master updates.  Not implemented see TerminalMasterProcess.
        //public virtual DateTime? LastTerminalMasterUpdate { get; set; }
        //public virtual List<TermainalMasterLite> TerminalMasters { get; set; }

        //Login Step 16) Send Universal Commodities.  Not implemented see UniversalCommoditiesProcess.
        //public virtual List<CommodityMasterLite> UniversalCommodities { get; set; }

        //Login Step 17): Send Preferences.  Not implemented see PreferencesProcess.
        //public virtual List<Preference> Preferences { get; set; }

        //Login step?
        //public virtual List<CodeTable> CodeTables { get; set; }

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

        /// <summary>
        /// Relevant input values, useful for logging
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("DriverLoginProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", Password: " + Password);
            sb.Append(", PowerId:" + PowerId);
            sb.Append(", Odometer:" + Odometer);
            sb.Append(", LocaleCode:" + LocaleCode);
            sb.Append(", OverrideFlag:" + OverrideFlag);
            sb.Append(", Mdtid: " + Mdtid);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
