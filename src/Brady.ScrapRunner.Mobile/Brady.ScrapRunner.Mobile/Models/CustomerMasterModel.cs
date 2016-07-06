using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace Brady.ScrapRunner.Mobile.Models
{
    [Table("CustomerMaster")]
    public class CustomerMasterModel
    {
        [PrimaryKey, MaxLength(15)]
        public string CustHostCode { get; set; }

        [MaxLength(1)]
        public string CustType { get; set; }

        [MaxLength(1)]
        public string CustSignatureRequired { get; set; }

        [MaxLength(30)]
        public string CustName { get; set; }

        [MaxLength(38)]
        public string CustAddress1 { get; set; }

        [MaxLength(38)]
        public string CustAddress2 { get; set; }

        [MaxLength(30)]
        public string CustCity { get; set; }

        [MaxLength(2)]
        public string CustState { get; set; }

        [MaxLength(10)]
        public string CustZip { get; set; }

        public int? CustLatitude { get; set; }

        public int? CustLongitude { get; set; }
    }
}
