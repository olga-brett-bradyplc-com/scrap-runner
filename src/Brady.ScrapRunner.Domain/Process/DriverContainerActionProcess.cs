using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverContainerActionProcess (request and response).
    ///
    public class DriverContainerActionProcess : IHaveId<string>, IEquatable<DriverContainerActionProcess>
    {
        ///The driver. Required.
        public virtual string EmployeeId { get; set; }

        ///The power id. Required.
        public virtual string PowerId { get; set; }

        ///The Action Type: D=Done,E=Exception,R=Review,L=Load,S=SetDown(Dropped). Required.
        public virtual string ActionType { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        /// Trip Number.  
        /// Required for Action Type:D=Done,E=Exception,R=Review.  
        /// Blank for Action Type:L=Load,S=SetDown(Dropped)
        public virtual string TripNumber { get; set; }

        /// Trip Segment Number.  
        /// Required for Action Type:D=Done,E=Exception,R=Review.
        /// Blank for Action Type:L=Load,S=SetDown(Dropped)
        public virtual string TripSegNumber { get; set; }

        /// Container Number.  Required.
        public virtual string ContainerNumber { get; set; }

        /// Container Type.  Optional. Will use ContainerMaster Type.
        public virtual string ContainerType { get; set; }

        /// Container Size.  Optional. Will use ContainerMaster Size
        public virtual string ContainerSize { get; set; }

        /// Commodity Code.  Optional.
        public virtual string CommodityCode { get; set; }

        /// Commodity Description.  Optional.
        public virtual string CommodityDesc { get; set; }

        /// Container Location.  Optional.
        public virtual string ContainerLocation { get; set; }

        /// Container Contents: E=Empty or L=Loaded. Required.
        public virtual string ContainerContents { get; set; }

        /// Container Level.  Optional.
        public virtual short? ContainerLevel { get; set; }

        /// Latitude of the action. Optional.
        public virtual int? Latitude { get; set; }

        /// Longitude of the action. Optional.
        public virtual int? Longitude { get; set; }

        ///The Action Code: Exception Code or Review Code or blank.
        public virtual string ActionCode { get; set; }

        ///The Action Desc: Exception Desc or Review Desc or blank.
        public virtual string ActionDesc { get; set; }

        ///Method Of Entry: M=Manual, S=Scanned. 
        ///Required if Container Number was entered by driver for this transaction.
        ///Can be null for RT/RN type segments where container number is not entered.
        public virtual string MethodOfEntry { get; set; }

        public virtual string DriverNotes { get; set; }

        ///
        ///Additional fields for yard/scale processing
        ///

        ///Scale Reference Number. Optional.
        public virtual string ScaleReferenceNumber { get; set; }

        ///Set In Yard Flag: Y= Set Down or N = Left On Truck
        public virtual string SetInYardFlag { get; set; }

        ///Gross (1st) Action Date/Time. 
        public virtual DateTime? Gross1ActionDateTime { get; set; }

        ///Gross (1st) Weight. Optional.
        public virtual int Gross1Weight { get; set; }

        ///Gross (2nd) Action Date/Time. 
        public virtual DateTime? Gross2ActionDateTime { get; set; }

        ///Gross (2nd) Weight. Optional.
        public virtual int Gross2Weight { get; set; }

        ///Tare Action Date/Time. 
        public virtual DateTime? TareActionDateTime { get; set; }

        ///Tare Weight. Optional.
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
        public virtual bool Equals(DriverContainerActionProcess other)
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
            return Equals((DriverContainerActionProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverContainerActionProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", ActionType:" + ActionType);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", ContainerNumber:" + ContainerNumber);
            sb.Append(", TripNumber: " + TripNumber);
            sb.Append(", TripSegNumber:" + TripSegNumber);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
