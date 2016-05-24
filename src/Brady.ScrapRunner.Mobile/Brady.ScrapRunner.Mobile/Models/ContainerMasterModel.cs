using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace Brady.ScrapRunner.Mobile.Models
{
    [Table("ContainerMaster")]
    public class ContainerMasterModel
    {
        [PrimaryKey, MaxLength(15)]
        public string ContainerNumber { get; set; }

        [MaxLength(5)]
        public string ContainerType { get; set; }

        [MaxLength(5)]
        public string ContainerSize { get; set; }

        [MaxLength(35)]
        public string ContainerLocation { get; set; }

        [MaxLength(15)]
        public string ContainerCustHostCode { get; set; }

        [MaxLength(1)]
        public string ContainerStatus { get; set; }

        [MaxLength(10)]
        public string ContainerCommodityCode { get; set; }

        [MaxLength(35)]
        public string ContainerCommodityDesc { get; set; }

        [MaxLength(16)]
        public string ContainerPowerId { get; set; }

        [Ignore]
        // Convienence methods
        public string ContainerFullDescription
        {
            get { return ContainerNumber + " " + ContainerType + "-" + ContainerSize; }
        }
    }
}
