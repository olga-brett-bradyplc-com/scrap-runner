namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("TripSegmentContainer")]
    public class TripSegmentContainerModel
    {
        [PrimaryKey]
        public string CompositeKey => TripNumber + ';' + TripSegNumber + ';' + TripSegContainerNumber;

        [MaxLength(10)]
        public string TripNumber { get; set; }

        [MaxLength(2)]
        public string TripSegNumber { get; set; }

        public int? TripSegContainerSeqNumber { get; set; }

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
    }
}
