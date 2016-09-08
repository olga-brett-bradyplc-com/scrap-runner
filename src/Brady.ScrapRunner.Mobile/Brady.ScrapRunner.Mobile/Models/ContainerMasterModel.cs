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

        [MaxLength(10)]
        public string ContainerCurrentTripNumber { get; set; }

        [MaxLength(2)]
        public string ContainerCurrentTripSegNumber { get; set; }

        [MaxLength(2)]
        public string ContainerCurrentTripSegType { get; set; }

        [MaxLength(15)]
        public string ContainerBarCodeNo { get; set; }

        public int ContainerLevel { get; set; }

        // These fields are un-mapped to help else track unused contianers left on power id
        [MaxLength(1)]
        public string ContainerComplete { get; set; }

        [MaxLength(1)]
        public string ContainerReviewFlag { get; set; }

        // This is a special field that allows us to unload a container at a later time
        [MaxLength(1)]
        public string ContainerToBeUnloaded { get; set; }

        [Ignore]
        // Convienence methods
        public string ContainerFullDescription
        {
            get { return ContainerNumber + " " + ContainerType + "-" + ContainerSize; }
        }
    }
}
