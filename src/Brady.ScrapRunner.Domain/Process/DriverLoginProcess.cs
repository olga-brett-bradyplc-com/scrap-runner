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
        ///The driver id from the phone.
        public virtual string EmployeeId { get; set; }

        ///The password from the phone.
        public virtual string Password { get; set; }

        ///The power id from the phone.
        public virtual string PowerId { get; set; }

        ///The odometer from the phone.
        public virtual int Odometer { get; set; }

        ///Currently the locale code from the phone. Driver can change settings.
        /// TODO: Q: Is this really a device preference or could/should it be a user preference perhaps via membership?
        /// Eg: 1033 = English, USA
        ///     2058 = Spanish, Mexico
        public virtual int? LocaleCode { get; set; }

        /// <summary>Set this to Y if driver has double checked Odometer and is resubmitting</summary>
        public virtual string OverrideFlag { get; set; }

        /// Currently this is the driver id. Probably will not be needed in the future.
        public virtual string Mdtid { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string TermId { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string RegionId { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string AreaId { get; set; }

        /// <summary>PndVer is a client version number for the handheld.  Typically used for logging issues.</summary>
        /// TODO: We'll need something similar for phones
        public virtual string PndVer { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string TripNumber { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string TripSegNumber { get; set; }

        /// Initially null.  Will be filled in and returned to driver.
        public virtual string DriverStatus { get; set; }
        
        /// The LoginDateTime from the phone.  (Q: Why shoud we trust the client or whe do we backfill on server side?)
        public virtual DateTime? LoginDateTime { get; set; }

        /// The ContainerMasterDateTime from the phone. Will now be sent to ContainerChangeProcess. 
        ///public virtual DateTime? ContainerMasterDateTime { get; set; }

        /// The TerminalMasterDateTime from the phone.  Will now be sent to TerminalChangeProcess. 
        /// public virtual DateTime? TerminalMasterDateTime { get; set; }

        ///Universal or Master Commodities.  Not implemented see CommodityMasterProcess.
        //public virtual List<CommodityMasterLite> UniversalCommodities { get; set; }

        //Preferences.  Not implemented see PreferencesProcess.
        //public virtual List<Preference> Preferences { get; set; }

        //CodeTable. Not implemented see CodeTableProcess.
        //public virtual List<CodeTable> CodeTables { get; set; }

        // Trip Info. Not implemented see TripInfoProcess.
        // a. Send Trip Information for each trip
        // b. Send Trip Reference Numbers for each trip.
        // c. Send TripSegment information for each trip
        // e. Send TripSegmentContainer information for each trip segment
        // f. Send a Trip End record. May not need this.
        // g. Directions, Commodities and Locations for each unique customer host code.

        /// Container Inventory (Containers on Power Unit)
        public virtual List<ContainerMaster> ContainersOnPowerId { get; set; }
        /// Send Dispatcher List for Messaging
        public virtual List<EmployeeMaster> UsersForMessaging { get; set; }

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
