using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Helpers
{
    public class ContainerGroupKey
    {
        public string Id => $"{CustHostCode};{TripNumber}";
        public string Name { get; set; }
        public string TripNumber { get; set; }
        public string CustHostCode { get; set; }
    }
}
