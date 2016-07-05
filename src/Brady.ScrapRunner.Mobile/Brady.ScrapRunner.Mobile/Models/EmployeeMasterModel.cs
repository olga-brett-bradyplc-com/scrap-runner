namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("EmployeeMaster")]
    public class EmployeeMasterModel
    {
        [PrimaryKey, MaxLength(10)]
        public string EmployeeId { get; set; }

        [MaxLength(10)]
        public string TerminalId { get; set; }

        [MaxLength(20)]
        public string AreaId { get; set; }

        [MaxLength(10)]
        public string RegionId { get; set; }

        [MaxLength(15)]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string LastName { get; set; }

        [MaxLength(15)]
        public string NickName { get; set; }

        [Ignore]
        public string FullName => FirstName + " " + LastName;
    }
}