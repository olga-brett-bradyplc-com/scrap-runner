namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("ContainerChange")]
    public class ContainerChangeModel
    {
        [PrimaryKey, MaxLength(15)]
        public string ContainerNumber { get; set; }

        [MaxLength(5)]
        public string ContainerType { get; set; }

        [MaxLength(5)]
        public string ContainerSize { get; set; }

        public DateTime? ActionDate { get; set; }

        [MaxLength(1)]
        public string ActionFlag { get; set; }

        [MaxLength(10)]
        public string TerminalId { get; set; }

        [MaxLength(10)]
        public string RegionId { get; set; }

        [MaxLength(30)]
        public string ContainerBarCodeNo { get; set; }
    }
}
