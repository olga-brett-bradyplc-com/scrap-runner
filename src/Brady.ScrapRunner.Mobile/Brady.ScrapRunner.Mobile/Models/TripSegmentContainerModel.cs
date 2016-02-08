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

        public int? TripSegContainerWeightGross { get; set; }

        public DateTime? WeightGross2ndDateTime { get; set; }

        public int? TripSegContainerWeightGross2nd { get; set; }

        public DateTime? WeightTareDateTime { get; set; }

        public int? TripSegContainerWeightTare { get; set; }

        public int? TripSegContainerLatitude { get; set; }

        public int? TripSegContainerLongitude { get; set; }

        [MaxLength(1)]
        public string TripSegContainerLoaded { get; set; }

        [MaxLength(1)]
        public string TripSegContainerOnTruck { get; set; }

        public int? TripSegContainerLevel { get; set; }
    }
}