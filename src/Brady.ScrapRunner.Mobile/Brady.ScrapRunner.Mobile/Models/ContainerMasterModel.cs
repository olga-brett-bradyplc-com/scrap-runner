using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Models
{
    // @TODO : Needs to be configure for use with repository/sqlite
    public class ContainerMasterModel
    {
        public string ContainerNumber { get; set; }
        public string ContainerType { get; set; }
        public string ContainerSize { get; set; }
        public string ContainerLocation { get; set; }
        public string ContainerCustHostCode { get; set; }
        public string ContainerStatus { get; set; }
        public string ContainerCommodityCode { get; set; }
        public string ContainerCommodityDesc { get; set; }

        // Convienence methods
        public string ContainerFullDescription
        {
            get { return ContainerNumber + " " + ContainerType + "-" + ContainerSize; }
        }
    }
}
