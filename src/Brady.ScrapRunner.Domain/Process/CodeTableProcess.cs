using System;
using System.Collections.Generic;
using System.Text;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    /// <summary>
    /// A CodeTableProcess (request and response).  A pseudo-record.
    /// </summary>
    public class CodeTableProcess : IHaveId<string>, IEquatable<CodeTableProcess>
    {
        /// <summary>
        /// Mandatory input paramter
        /// </summary>
        public virtual string EmployeeId { get; set; }

        public virtual string CodeName { get; set; }
        public virtual string CodeValue { get; set; }
        public virtual string CodeDisp1 { get; set; }
        public virtual string CodeDisp2 { get; set; }
        public virtual string CodeDisp3 { get; set; }
        public virtual string CodeDisp4 { get; set; }
        public virtual string CodeDisp5 { get; set; }
        public virtual string CodeDisp6 { get; set; }

        /// <summary>
        /// The return values
        /// </summary>
        public virtual List<CodeTable> CodeTables { get; set; }

        public virtual string Id
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

        public virtual bool Equals(CodeTableProcess other)
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
            return Equals((CodeTableProcess)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = EmployeeId?.GetHashCode() ?? 0;
            return hashCode;
        }

        /// <summary>
        /// Relevant input (and output) values, useful for logging
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder("CodeTableProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", CodeName:" + CodeName);
            sb.Append(", CodeValue:" + CodeValue);
            sb.Append(", CodeDisp1: " + CodeDisp1);
            sb.Append(", CodeDisp2: " + CodeDisp2);
            sb.Append(", CodeDisp3: " + CodeDisp3);
            sb.Append(", CodeDisp4: " + CodeDisp4);
            sb.Append(", CodeDisp5: " + CodeDisp5);
            sb.Append(", CodeDisp6: " + CodeDisp6);
            if (null != CodeTables)
            {
                sb.Append(", CodeTables: [Count: ");
                sb.Append(CodeTables.Count);
                sb.Append("]");
            }
            sb.Append("}");
            return sb.ToString();
        }

    }
}
