﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;
using Brady.ScrapRunner.Domain.Enums;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A DriverStatus record.  FIXME:  Remove the composite Id workaround for NHibernate and/or metadata 
    /// </summary>
    public class DriverStatus : IHaveId<string>, IEquatable<DriverStatus>
    {

        public virtual string EmployeeId { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual string TripSegType { get; set; }
        public virtual string TripAssignStatus { get; set; }
        public virtual string TripStatus { get; set; }
        public virtual string TripSegStatus { get; set; }
        /// <summary>
        /// Known as DriverStatus in the scraprunner database.
        /// </summary>
        public virtual string Status { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string PowerId { get; set; }
        public virtual string DriverArea { get; set; }
        public virtual string MDTId { get; set; }
        public virtual DateTime? LoginDateTime { get; set; }
        public virtual DateTime? ActionDateTime { get; set; }
        public virtual int? DriverCumMinutes { get; set; }
        public virtual int? Odometer { get; set; }
        public virtual string RFIDFlag { get; set; }
        public virtual string RouteTo { get; set; }
        public virtual DateTime? LoginProcessedDateTime { get; set; }
        public virtual string GPSAutoGeneratedFlag { get; set; }
        public virtual DateTime? ContainerMasterDateTime { get; set; }
        public virtual string DelayCode { get; set; }
        public virtual string PrevDriverStatus { get; set; }
        public virtual string MdtVersion { get; set; }
        public virtual string GPSXmitFlag { get; set; }
        public virtual DriverForceLogoffValue SendHHLogoffFlag { get; set; }
        public virtual DateTime? TerminalMasterDateTime { get; set; }
        public virtual int? DriverLCID { get; set; }
        public virtual string ServicesFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return EmployeeId;
            }
            set
            {

            }
        }

        public virtual bool Equals(DriverStatus other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(EmployeeId, other.EmployeeId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverStatus) obj);
        }

        public override int GetHashCode()
        {
            return (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
        }
    }

}
