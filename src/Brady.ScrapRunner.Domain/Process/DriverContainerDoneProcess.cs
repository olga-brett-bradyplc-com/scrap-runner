using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverContainerDoneProcess (request and response).
    ///
    public class DriverContainerDoneProcess : IHaveId<string>, IEquatable<DriverContainerDoneProcess>
    {
        ///The driver. Required.
        public virtual string EmployeeId { get; set; }

        /// Trip Number.  Required.
        public virtual string TripNumber { get; set; }

        /// Trip Segment Number.  Required.
        public virtual string TripSegNumber { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        ///The power id. Required.
        public virtual string PowerId { get; set; }

        /// Container Number.  Required.
        public virtual string ContainerNumber { get; set; }

        /// Container Type.  Required.
        public virtual string ContainerType { get; set; }

        /// Container Size.  Optional.
        public virtual string ContainerSize { get; set; }

        /// Commodity Code.  Optional.
        public virtual string CommodityCode { get; set; }

        /// Commodity Description.  Optional.
        public virtual string CommodityDesc { get; set; }

        /// Container Location.  Optional.
        public virtual string ContainerLocation { get; set; }

        /// Container Contents: E=Empty or L=Loaded.
        public virtual string ContainerContents { get; set; }

        /// Container Level.  Optional.
        public virtual short? ContainerLevel { get; set; }

        /// Latitude of the action.
        public virtual int? Latitude { get; set; }

        /// Longitude of the action.
        public virtual int? Longitude { get; set; }

        ///The Action Type: D=Done,E=Exception,R=Review. Required.
        public virtual string ActionType { get; set; }

        ///The Action Code: Exception Code or Review Code or blank.
        public virtual string ActionCode { get; set; }

        ///The Action Desc: Exception Desc or Review Desc or blank.
        public virtual string ActionDesc { get; set; }

        ///Method Of Entry: M=Manual, S=Scanned
        public virtual string MethodOfEntry { get; set; }

        ///
        ///Additional fields for yard/scale processing
        ///
        
        ///Scale Reference Number
        public virtual string ScaleReferenceNumber { get; set; }

        ///Set In Yard Flag: Y= Set Down or N = Left On Truck
        public virtual string SetInYardFlag { get; set; }

        ///Gross (1st) Action Date/Time. 
        public virtual DateTime? Gross1ActionDateTime { get; set; }

        ///Gross (1st) Weight
        public virtual int Gross1Weight { get; set; }

        ///Gross (2nd) Action Date/Time. 
        public virtual DateTime? Gross2ActionDateTime { get; set; }

        ///Gross (2nd) Weight
        public virtual int Gross2Weight { get; set; }

        ///Tare Action Date/Time. 
        public virtual DateTime? TareActionDateTime { get; set; }

        ///Tare Weight
        public virtual int TareWeight { get; set; }

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
        public virtual bool Equals(DriverContainerDoneProcess other)
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
            return Equals((DriverContainerDoneProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverContainerDoneProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", TripNumber: " + TripNumber);
            sb.Append(", TripSegNumber:" + TripSegNumber);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", ContainerNumber:" + ContainerNumber);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
