namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("CustomerDirection")]
    public class CustomerDirectionModel
    {
        [PrimaryKey, AutoIncrement]
        public int? DirectionId { get; set; }
        [MaxLength(15)]
        [Indexed(Name = "CustomerDirectionCompositeKey", Order = 1, Unique = true)]
        public string CustHostCode { get; set; }
        [Indexed(Name = "CustomerDirectionCompositeKey", Order = 2, Unique = true)]
        public short DirectionsSeqNo { get; set; }
        [MaxLength(35)]
        public string DirectionsDesc { get; set; }
    }
}