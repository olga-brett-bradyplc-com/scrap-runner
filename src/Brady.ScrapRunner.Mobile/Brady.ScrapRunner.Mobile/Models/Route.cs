using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Models
{
    public class Route
    {
        public string TripNumber { get; set; }

        // Properties for Route Summary
        public string TripType { get; set; }
        public string Notes { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CloseTime { get; set; }
        public string OpenTime { get; set; }
        public string CompanyName { get; set; }

        // Convenience methods
        public string CityStateZipFormatted => $"{City}, {State} {Zipcode}";
        public string OpenFormatted => $"OPEN: {OpenTime}";
        public string CloseFormatted => $"CLOSE: {CloseTime}";
        
    }
}
