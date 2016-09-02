using Brady.ScrapRunner.Domain.Enums;
using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Models
{
   
    /// <summary>
    /// A TripPoints record.
    /// </summary>
    public class TripPoints : IHaveCompositeId, IEquatable<TripPoints>
    {
        public virtual string TripPointsHostCode1 { get; set; }
        public virtual string TripPointsHostCode2 { get; set; }
        public virtual int? TripPointsStandardMinutes { get; set; }
        public virtual int? TripPointsStandardMiles { get; set; }
        public virtual TripSendToMapsValue TripPointsSendToMaps { get; set; }
        public virtual DateTime? ChgDateTime { get; set; }
        public virtual string ChgEmployeeId { get; set; }

        public virtual string Id
        {
            get
            {
                return string.Format("{0};{1}", TripPointsHostCode1, TripPointsHostCode2);
            }
            set
            {

            }
        }

        public virtual bool Equals(TripPoints other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripPointsHostCode1, other.TripPointsHostCode1) &&
                   string.Equals(TripPointsHostCode2, other.TripPointsHostCode2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TripPoints)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripPointsHostCode1 != null ? TripPointsHostCode1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TripPointsHostCode2 != null ? TripPointsHostCode2.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
