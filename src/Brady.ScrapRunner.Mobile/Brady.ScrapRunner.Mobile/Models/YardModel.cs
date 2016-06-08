namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("TerminalMaster")]
    public class YardModel
    {
        [PrimaryKey, MaxLength(10)]
        public string TerminalId { get; set; }

        [MaxLength(10)]
        public string Region { get; set; }

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
        public string CustPhone { get; set; }

        [MaxLength(9)]
        public string CustLatitude { get; set; }

        [MaxLength(9)]
        public string CustLongitude { get; set; }

        [MaxLength(1)]
        public string ActionFlag { get; set; }

        [MaxLength(15)]
        public string CustHostCode { get; set; }

        [MaxLength(8)]
        public string Cust4X4 { get; set; }

        [MaxLength(30)]
        public string CustName { get; set; }

        [MaxLength(30)]
        public string ContactName { get; set; }

        public DateTime? CustOpenTime { get; set; }

        public DateTime? CustCloseTime { get; set; }

        public int? CustRadius { get; set; }

        [MaxLength(60)]
        public string DriverInstructions { get; set; }
    }
}
