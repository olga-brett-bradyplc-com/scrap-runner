namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("TripSegment")]
    public class TripSegmentModel
    {
        [PrimaryKey, AutoIncrement]
        public int Tid { get; set; }
        // @TODO : Creating the primary key this way is actually creating the table without a pk, which prevents us from deleting the row
        //public string CompositeKey => TripNumber + ';' + TripSegNumber;

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
        public int? TripSegContainerQty { get; set; }
    }
}
