using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Models
{
    public class TransactionDetail
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string Location { get; set; }
        public string MaterialType { get; set; }
    }
}
