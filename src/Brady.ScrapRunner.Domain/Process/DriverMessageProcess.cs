using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    ///
    /// A DriverMessageProcess (request and response).
    ///
    public class DriverMessageProcess : IHaveId<string>, IEquatable<DriverMessageProcess>
    {
        ///The driver id. Required.
        public virtual string EmployeeId { get; set; }

        /// The ActionDateTime. Required.
        public virtual DateTime? ActionDateTime { get; set; }

        /// Sender Id. Required for received messages.
        public virtual string SenderId { get; set; }

        /// Receiver Id. Required for received messages.
        public virtual string ReceiverId { get; set; }

        ///The Message Id. Required for received messages.
        public virtual int MessageId { get; set; }

        ///The Message Id. Required for received messages.
        public virtual int MessageThread { get; set; }

        ///The Message Text. Required for received messages.
        public virtual string MessageText { get; set; }

        /// Urgent Flag.  Y/N. Default is N.
        public virtual string UrgentFlag { get; set; }

        /// <summary>
        /// The return values
        /// </summary>
        public virtual List<Messages> Messages { get; set; }

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
        public virtual bool Equals(DriverMessageProcess other)
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
            return Equals((DriverMessageProcess)obj);
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
            StringBuilder sb = new StringBuilder("DriverMessageProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", SenderId: " + SenderId);
            sb.Append(", ReceiverId:" + ReceiverId);
            sb.Append(", ActionDateTime:" + ActionDateTime);
            sb.Append(", UrgentFlag: " + UrgentFlag);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
