﻿namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("TerminalMaster")]
    public class TerminalMasterModel
    {
        [PrimaryKey, MaxLength(10)]
        public string TerminalId { get; set; }

        [MaxLength(10)]
        public string Region { get; set; }

        [MaxLength(38)]
        public string Address1 { get; set; }

        [MaxLength(38)]
        public string Address2 { get; set; }

        [MaxLength(30)]
        public string City { get; set; }

        [MaxLength(2)]
        public string State { get; set; }

        [MaxLength(10)]
        public string Zip { get; set; }

        [MaxLength(3)]
        public string Country { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public int? Latitude { get; set; }

        public int? Longitude { get; set; }

        [MaxLength(30)]
        public string TerminalName { get; set; }

        // These two fields come from the TerminalChange table, and are not currently columns on the remote TerminalMaster table
        [MaxLength(1)]
        public string CustType { get; set; }

        [MaxLength(15)]
        public string CustHostCode { get; set; }

        [Ignore]
        public string CityStateZipFormatted => $"{City}, {State} {Zip}";

    }
}
