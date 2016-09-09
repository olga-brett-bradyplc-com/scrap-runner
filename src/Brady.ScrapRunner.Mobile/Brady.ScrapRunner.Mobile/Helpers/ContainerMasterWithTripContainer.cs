using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Helpers
{
    public class ContainerMasterWithTripContainer
    {
        public string Key { get; set; }
        public ContainerMasterModel ContainerMaster { get; set; }
        public TripSegmentContainerModel TripSegmentContainer { get; set; }
    }
}
