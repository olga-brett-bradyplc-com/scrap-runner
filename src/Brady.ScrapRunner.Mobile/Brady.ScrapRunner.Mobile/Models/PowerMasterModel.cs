using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace Brady.ScrapRunner.Mobile.Models
{
    [Table("PowerMaster")]
    public class PowerMasterModel
    {
        [PrimaryKey, MaxLength(16)]
        public string PowerId { get; set; }
        [MaxLength(10)]
        public string PowerType { get; set; }
        [MaxLength(35)]
        public string PowerDesc { get; set; }
        [MaxLength(5)]
        public string PowerSize { get; set; }
        public int? PowerLength { get; set; }
        public int? PowerTareWeight { get; set; }
        [MaxLength(15)]
        public string PowerCustHostCode { get; set; }
        [MaxLength(1)]
        public string PowerCustType { get; set; }
        [MaxLength(10)]
        public string PowerTerminalId { get; set; }
        [MaxLength(10)]
        public string PowerRegionId { get; set; }
        [MaxLength(35)]
        public string PowerLocation { get; set; }
        [MaxLength(1)]
        public string PowerStatus { get; set; }
        public DateTime? PowerDateOutOfService { get; set; }
        public DateTime? PowerDateInService { get; set; }
        [MaxLength(10)]
        public string PowerDriverId { get; set; }
        public int? PowerOdometer { get; set; }
        [MaxLength(60)]
        public string PowerComments { get; set; }
        [MaxLength(12)]
        public string MdtId { get; set; }
        [MaxLength(5)]
        public string PrimaryContainerType { get; set; }
        [MaxLength(10)]
        public string OrigTerminalId { get; set; }
        public DateTime? PowerLastActionDateTime { get; set; }
        [MaxLength(10)]
        public string PowerCurrentTripNumber { get; set; }
        [MaxLength(2)]
        public string PowerCurrentTripSegNumber { get; set; }
        [MaxLength(4)]
        public string PowerCurrentTripSegType { get; set; }
        [MaxLength(50)]
        public string PowerAssetNumber { get; set; }
        [MaxLength(16)]
        public string PowerIdHost { get; set; }
    }
}
