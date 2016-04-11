namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("CustomerDirections")]
    public class CustomerDirectionsModel
    {
        public string CompositeId
        {
            get { return string.Format("{0};{1}", CustHostCode, DirectionsSeqNo); }
            set { /* NO-OP SETTER */ }
        }
        [MaxLength(15)]
        public string CustHostCode { get; set; }
        public short DirectionsSeqNo { get; set; }
        [MaxLength(35)]
        public string DirectionsDesc { get; set; }
    }
}