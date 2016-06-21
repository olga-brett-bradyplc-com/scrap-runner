namespace Brady.ScrapRunner.Mobile.Models
{
    using System;

    public class LocationModel
    {
        public double? Accuracy { get; set; }
        public double? Heading { get; set; }
        public double? HeadingAccuracy { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}