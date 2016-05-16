using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverNewContainerProcess (request and response).
    ///
    public class DriverNewContainerProcess : IHaveId<string>, IEquatable<DriverNewContainerProcess>
    {
        ///The driver id. Required.
        public virtual string EmployeeId { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime ActionDateTime { get; set; }

        /// Container Number. Required.
        public virtual string ContainerNumber { get; set; }

        /// ContainerType. Required.
        public virtual string ContainerType { get; set; }

        ///The ContainerSize. Required.
        public virtual string ContainerSize { get; set; }

        ///The ContainerBarcode. Required.
        public virtual string ContainerBarcode { get; set; }

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
        public virtual bool Equals(DriverNewContainerProcess other)
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
            return Equals((DriverNewContainerProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverNewContainerProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", ContainerNumber: " + ContainerNumber);
            sb.Append(", ContainerType:" + ContainerType);
            sb.Append(", ContainerSize: " + ContainerSize);
            sb.Append(", ContainerBarcode: " + ContainerBarcode);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
