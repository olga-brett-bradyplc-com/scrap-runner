﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Process
{
    /// <summary>
    /// A CommodityMasterProcess (request and response).  A pseudo-record.
    /// </summary>
    public class CommodityMasterProcess : IHaveId<string>, IEquatable<CommodityMasterProcess>
    {
        /// <summary>
        /// Mandatory input paramter
        /// </summary>
        public virtual string EmployeeId { get; set; }
        public virtual string CommodityCode { get; set; }
        public virtual string CommodityDesc { get; set; }

        /// <summary>
        /// The return value
        /// </summary>
        public virtual List<CommodityMaster> CommodityMasters { get; set; }

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

        public virtual bool Equals(CommodityMasterProcess other)
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
            return Equals((CommodityMasterProcess)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EmployeeId?.GetHashCode() ?? 0;
                return hashCode;
            }
        }

        /// <summary>
        /// Relevant input (and output) values, useful for logging
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("CommodityMasterProcess{");
            sb.Append("EmployeeId:" + EmployeeId);
            sb.Append(", CommodityCode:" + CommodityCode);
            sb.Append(", CommodityDesc:" + CommodityDesc);
            if (null != CommodityMasters)
            {
                sb.Append(", CommodityMasters: [Count: ");
                sb.Append(CommodityMasters.Count);
                sb.Append("]");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
