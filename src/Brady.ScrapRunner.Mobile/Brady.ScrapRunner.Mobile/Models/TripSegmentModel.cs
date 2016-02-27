namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("TripSegment")]
    public class TripSegmentModel
    {
        [PrimaryKey]
        public string CompositeKey => TripNumber + ';' + TripSegNumber;

        [MaxLength(10)]
        public string TripNumber { get; set; }
        [MaxLength(2)]
        public string TripSegNumber { get; set; }
        [MaxLength(4)]
        public string TripSegType { get; set; }
        [MaxLength(35)]
        public string TripSegTypeDesc { get; set; }
        [MaxLength(30)]
        public string TripSegComments { get; set; }
        [MaxLength(35)]
        public string TripSegOrigCustName { get; set; }
        [MaxLength(10)]
        public string TripSegOrigCustHostCode { get; set; }
        [MaxLength(10)]
        public string TripSegDestCustHostCode { get; set; }
    }
}
