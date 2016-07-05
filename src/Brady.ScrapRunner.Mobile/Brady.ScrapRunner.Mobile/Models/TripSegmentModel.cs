namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("TripSegment")]
    public class TripSegmentModel
    {
        // It seems SQLite.NET doesn't like it when we create a primary key without a setter,
        // so for now we'll have a no-op setter just so the table gets created with a PK.
        [PrimaryKey]
        public string CompositeKey
        {
            get { return string.Format("{0};{1}", TripNumber, TripSegNumber); }
            set { /* NO-OP SETTER */ }
        }

        [MaxLength(10)]
        public string TripNumber { get; set; }
        [MaxLength(2)]
        public string TripSegNumber { get; set; }
        [MaxLength(1)]
        public string TripSegStatus { get; set; }
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
        [MaxLength(1)]
        public string TripSegDestCustType { get; set; }
        [MaxLength(30)]
        public string TripSegDestCustName { get; set; }
        [MaxLength(10)]
        public string TripSegDestCustHostCode { get; set; }
        [MaxLength(38)]
        public string TripSegDestCustAddress1 { get; set; }
        [MaxLength(38)]
        public string TripSegDestCustAddress2 { get; set; }
        [MaxLength(30)]
        public string TripSegDestCustCity { get; set; }
        [MaxLength(2)]
        public string TripSegDestCustState { get; set; }
        [MaxLength(10)]
        public string TripSegDestCustZip { get; set; }
        public int? TripSegStartLongitude { get; set; }
        public int? TripSegEndLongitude { get; set; }
        public int? TripSegStartLatitude { get; set; }
        public int? TripSegEndLatitude { get; set; }
        public int? TripSegContainerQty { get; set; }

        [Ignore]
        public string DestCustCityStateZip => $"{TripSegDestCustCity}, {TripSegDestCustState} {TripSegDestCustZip}";
    }
}
