﻿namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("TripSegmentContainer")]
    public class TripSegmentContainerModel
    {
        // It seems SQLite.NET doesn't like it when we create a primary key without a setter,
        // so for now we'll have a no-op setter just so the table gets created with a PK.
        [PrimaryKey]
        public string CompositeKey
        {
            get { return TripNumber + ";" + TripSegNumber + ";" + TripSegContainerSeqNumber; }
            set { /* NO-OP SETTER */ }
        }

        [MaxLength(10)]
        public string TripNumber { get; set; }

        [MaxLength(2)]
        public string TripSegNumber { get; set; }
        
        public short TripSegContainerSeqNumber { get; set; }

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

        public DateTime? TripSegContainerActionDateTime { get; set; }

        public DateTime? WeightGrossDateTime { get; set; }

        public int? TripSegContainerWeightGross { get; set; }

        public DateTime? WeightGross2ndDateTime { get; set; }

        public int? TripSegContainerWeightGross2nd { get; set; }

        public DateTime? WeightTareDateTime { get; set; }

        public int? TripSegContainerWeightTare { get; set; }

        public double? TripSegContainerLatitude { get; set; }

        public double? TripSegContainerLongitude { get; set; }

        [MaxLength(1)]
        public string TripSegContainerLoaded { get; set; }

        [MaxLength(1)]
        public string TripSegContainerOnTruck { get; set; }

        public short? TripSegContainerLevel { get; set; }

        [MaxLength(1)]
        public string TripSegContainerComplete { get; set; }

        // Convenience methods
        [Ignore]
        public string DefaultTripSegContainerNumber
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