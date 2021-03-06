﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A DriverHistory record.
    /// </summary>
    public class DriverHistory : IHaveCompositeId, IEquatable<DriverHistory>
    {
        public virtual string EmployeeId { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual int DriverSeqNumber { get; set; }
        public virtual string TripSegNumber { get; set; }
        public virtual string TripSegType { get; set; }
        public virtual string TripSegTypeDesc { get; set; }
        public virtual string TripAssignStatus { get; set; }
        public virtual string TripAssignStatusDesc { get; set; }
        public virtual string TripStatus { get; set; }
        public virtual string TripStatusDesc { get; set; }
        public virtual string TripSegStatus { get; set; }
        public virtual string TripSegStatusDesc { get; set; }
        public virtual string DriverStatus { get; set; }
        public virtual string DriverStatusDesc { get; set; }
        public virtual string DriverName { get; set; }
        public virtual string TerminalId { get; set; }
        public virtual string TerminalName { get; set; }
        public virtual string RegionId { get; set; }
        public virtual string RegionName { get; set; }
        public virtual string PowerId { get; set; }
        public virtual string DriverArea { get; set; }
        public virtual string MDTId { get; set; }
        public virtual DateTime? LoginDateTime { get; set; }
        public virtual DateTime? ActionDateTime { get; set; }
        public virtual int? DriverCumMinutes { get; set; }
        public virtual int? Odometer { get; set; }
        public virtual string DestCustType { get; set; }
        public virtual string DestCustTypeDesc { get; set; }
        public virtual string DestCustHostCode { get; set; }
        public virtual string DestCustName { get; set; }
        public virtual string DestCustAddress1 { get; set; }
        public virtual string DestCustAddress2 { get; set; }
        public virtual string DestCustCity { get; set; }
        public virtual string DestCustState { get; set; }
        public virtual string DestCustZip { get; set; }
        public virtual string DestCustCountry { get; set; }
        public virtual string GPSAutoGeneratedFlag { get; set; }
        public virtual string GPSXmitFlag { get; set; }
        public virtual string MdtVersion { get; set; }
        public virtual string ServicesFlag { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1};{2}", DriverSeqNumber, EmployeeId, TripNumber);
            }
            set
            {

            }
        }

        public virtual bool Equals(DriverHistory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DriverSeqNumber == other.DriverSeqNumber
                && string.Equals(EmployeeId, other.EmployeeId)
                && string.Equals(TripNumber, other.TripNumber) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverHistory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DriverSeqNumber;
                hashCode = (hashCode * 397) ^ (EmployeeId != null ? EmployeeId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TripNumber != null ? TripNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
