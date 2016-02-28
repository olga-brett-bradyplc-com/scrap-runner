﻿namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("TripSegmentContainer")]
    public class TripSegmentContainerModel
    {
        [PrimaryKey]
        public string CompositeKey => TripNumber + ';' + TripSegNumber + ';' + TripSegContainerNumber;

        [MaxLength(10)]
        public string TripNumber { get; set; }

        [MaxLength(2)]
        public string TripSegNumber { get; set; }

        public int? TripSegContainerSeqNumber { get; set; }

        [MaxLength(16)]
        public string TripSegContainerNumber { get; set; }

        [MaxLength(5)]
        public string TripSegContainerType { get; set; }

        [MaxLength(5)]
        public string TripSegContainerSize { get; set; }

        [MaxLength(10)]
        public string TripSegContainerCommodityCode { get; set; }

        [MaxLength(35)]
        public string TripSegContainerCommodityDesc { get; set; }

        [MaxLength(35)]
        public string TripSegContainerLocation { get; set; }

        [MaxLength(60)]
        public string TripSegComments { get; set; }

        [MaxLength(300)]
        public string TripSegContainerComment { get; set; }

        public DateTime? WeightGrossDateTime { get; set; }

        public long? TripSegContainerWeightGross { get; set; }

        public DateTime? WeightGross2ndDateTime { get; set; }

        public long? TripSegContainerWeightGross2nd { get; set; }

        public DateTime? WeightTareDateTime { get; set; }

        public long? TripSegContainerWeightTare { get; set; }

        public long? TripSegContainerLatitude { get; set; }

        public long? TripSegContainerLongitude { get; set; }

        [MaxLength(1)]
        public string TripSegContainerLoaded { get; set; }

        [MaxLength(1)]
        public string TripSegContainerOnTruck { get; set; }

        public int? TripSegContainerLevel { get; set; }

        // Convenience methods
        [Ignore]
        public string DefaultTripSegContainerSeqNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TripSegContainerNumber))
                {
                    return "<NO NUMBER>";
                }
                else
                {
                    return TripSegContainerNumber;
                }
                
            }
        }
    }
}