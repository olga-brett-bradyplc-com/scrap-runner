namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("TerminalChange")]
    public class TerminalChangeModel
    {
        [PrimaryKey, MaxLength(10)]
        public string TerminalId { get; set; }

        [MaxLength(10)]
        public string RegionId { get; set; }

        [MaxLength(1)]
        public string CustType { get; set; }

        [MaxLength(15)]
        public string CustHostCode { get; set; }

        [MaxLength(8)]
        public string CustCode4_4 { get; set; }

        [MaxLength(30)]
        public string CustName { get; set; }

        [MaxLength(38)]
        public string CustAddress1 { get; set; }

        [MaxLength(38)]
        public string CustAddress2 { get; set; }

        [MaxLength(30)]
        public string CustCity { get; set; }

        [MaxLength(2)]
        public string CustState { get; set; }

        [MaxLength(10)]
        public string CustZip { get; set; }

        [MaxLength(3)]
        public string CustCountry { get; set; }

        [MaxLength(20)]
        public string CustPhone1 { get; set; }

        [MaxLength(30)]
        public string CustContact1 { get; set; }

        public DateTime? CustOpenTime { get; set; }

        public DateTime? CustCloseTime { get; set; }

        public int? CustLatitude { get; set; }

        public int? CustLongitude { get; set; }

        public int? CustRadius { get; set; }

        public DateTime? ChgDateTime { get; set; }

        [MaxLength(1)]
        public string ChgActionFlag { get; set; }

        [MaxLength(300)]
        public string CustDriverInstructions { get; set; }
    }
}
