namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;
    [Table("CustomerLocation")]
    public class CustomerLocationModel
    {
        public string CompositeKey
        {
            get { return string.Format("{0};{1}", CustHostCode, CustLocation); }
            set { /* NO-OP SETTER */ }
        }

        [MaxLength(15)]
        public string CustHostCode { get; set; }
        [MaxLength(35)]
        public string CustLocation { get; set; }
        public int? CustStandardMinutes { get; set; }
    }
}
