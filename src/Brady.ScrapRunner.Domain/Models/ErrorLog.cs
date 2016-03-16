using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A ErrorLog record.
    /// </summary>
    public class ErrorLog : IHaveId<int>, IEquatable<ErrorLog>
    {
        public virtual int ErrorId { get; set; }
        public virtual DateTime ErrorDateTime { get; set; }
        public virtual int ErrorSeqNo { get; set; }
        public virtual string ErrorType { get; set; }
        public virtual string ErrorDescription { get; set; }
        public virtual string ErrorTerminalId { get; set; }
        public virtual string ErrorRegionId { get; set; }
        public virtual string ErrorFlag { get; set; }
        public virtual string ErrorContainerNumber { get; set; }
        public virtual string ErrorTripNumber { get; set; }

        public virtual int Id
        {
            get
            {
                return ErrorId;
            }
            set
            {

            }
        }

        public virtual bool Equals(ErrorLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ErrorId == other.ErrorId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ErrorLog) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ErrorId.GetHashCode() ;
                return hashCode;
            }
        }
    }

}
