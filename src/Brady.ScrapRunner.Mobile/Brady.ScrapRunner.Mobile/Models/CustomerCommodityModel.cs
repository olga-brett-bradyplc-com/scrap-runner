namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;
    [Table("CustomerCommodity")]
    public class CustomerCommodityModel
    {
        [PrimaryKey]
        public string CompositeKey
        {
            get { return string.Format("{0};{1}", CustCommodityCode, CustHostCode); }
            set { /* NO-OP SETTER */ }
        }
        [MaxLength(15)]
        public string CustHostCode { get; set; }
        [MaxLength(10)]
        public string CustCommodityCode { get; set; }
        [MaxLength(10)]
        public string MasterCommodityCode { get; set; }
        [MaxLength(35)]
        public string CustCommodityDesc { get; set; }
        [MaxLength(5)]
        public string CustContainerType { get; set; }
        [MaxLength(5)]
        public string CustContainerSize { get; set; }
        [MaxLength(55)]
        public string CustContainerLocation { get; set; }
        [MaxLength(15)]
        public string DestCustHostCode { get; set; }
        [MaxLength(55)]
        public string DestContainerLocation { get; set; }
        public DateTime? DestExpirationDate { get; set; }
        public int? CustStandardMinutes { get; set; }

    }
}
