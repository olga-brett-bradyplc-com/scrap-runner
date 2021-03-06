﻿using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("Trip")]
    public class TripModel
    {
        [PrimaryKey, MaxLength(10)]
        public string TripNumber { get; set; }

        [MaxLength(1)]
        public string TripStatus { get; set; }

        [MaxLength(20)]
        public string TripStatusDesc { get; set; }

        [MaxLength(1)]
        public string TripAssignStatus { get; set; }

        [MaxLength(35)]
        public string TripAssignStatusDesc { get; set; }

        [MaxLength(4)]
        public string TripType { get; set; }

        [MaxLength(35)]
        public string TripTypeDesc { get; set; }

        public int? TripSequenceNumber { get; set; }

        [MaxLength(15)]
        public string TripCustHostCode { get; set; }

        [MaxLength(8)]
        public string TripCustCode4_4 { get; set; }

        [MaxLength(30)]
        public string TripCustName { get; set; }

        [MaxLength(38)]
        public string TripCustAddress1 { get; set; }

        [MaxLength(38)]
        public string TripCustAddress2 { get; set; }

        [MaxLength(30)]
        public string TripCustCity { get; set; }

        [MaxLength(2)]
        public string TripCustState { get; set; }

        [MaxLength(10)]
        public string TripCustZip { get; set; }

        [MaxLength(3)]
        public string TripCustCountry { get; set; }

        [MaxLength(30)]
        public string TripCustPhone1 { get; set; }

        [MaxLength(35)]
        public string TripContactName { get; set; }

        public DateTime? TripCustOpenTime { get; set; }
        public DateTime? TripCustCloseTime { get; set; }
        public DateTime? TripReadyDateTime { get; set; }

        [MaxLength(300)]
        public string TripDriverInstructions { get; set; }

        [MaxLength(60)]
        public string TripSpecInstructions { get; set; }

        [MaxLength(10)]
        public string TripTerminalId { get; set; }

        [Ignore]
        public string CityStateZipFormatted => $"{TripCustCity}, {TripCustState} {TripCustZip}";

        [Ignore]
        public string TripNumberDesc => $"{AppResources.Trip} {TripNumber}";

        [Ignore]
        public string TripCustOpenTime24Hr
            => TripCustOpenTime.HasValue ? TripCustOpenTime.Value.ToLocalTime().ToString("HH:mm") : "";

        [Ignore]
        public string TripCustCloseTime24Hr => TripCustCloseTime.HasValue ? TripCustCloseTime.Value.ToLocalTime().ToString("HH:mm") : "";
    }
}